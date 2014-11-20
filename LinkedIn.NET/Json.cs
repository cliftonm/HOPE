// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LinkedIn.NET
{
    /// <summary>
    /// Provides very simple parsing of JSON string into dictionary
    /// Based on original code of Patrick van Bergen
    /// http://techblog.procurios.nl/k/618/news/view/14605/14863/how-do-i-write-my-own-parser-(for-json).html
    /// </summary>
    internal class Json
    {
        private enum Tokens
        {
            TOKEN_NONE,
            TOKEN_CURLY_OPEN,
            TOKEN_CURLY_CLOSE,
            TOKEN_BRACKET_OPEN,
            TOKEN_BRACKET_CLOSE,
            TOKEN_COLON,
            TOKEN_COMMA,
            TOKEN_STRING,
            TOKEN_NUMBER,
            TOKEN_TRUE,
            TOKEN_FALSE,
            TOKEN_NULL
        }

        /// <summary>
        /// Parses the JSON string into dictionary
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <returns>Dictionary</returns>
        internal static Dictionary<string, string> DecodeToDictionary(string json)
        {

            var success = true;
            if (json == null)
            {
                return null;
            }
            var charArray = json.ToCharArray();
            var index = 0;
            var value = parseObject(charArray, ref index, ref success);
            return value;
        }

        private static Dictionary<string, string> parseObject(char[] json, ref int index, ref bool success)
        {
            var table = new Dictionary<string, string>();
            nextToken(json, ref index);

            while (true)
            {
                var token = lookAhead(json, index);
                switch (token)
                {
                    case Tokens.TOKEN_NONE:
                        success = false;
                        return null;
                    case Tokens.TOKEN_COMMA:
                        nextToken(json, ref index);
                        break;
                    case Tokens.TOKEN_CURLY_CLOSE:
                        nextToken(json, ref index);
                        return table;
                    default:
                        // key
                        var name = parseString(json, ref index, ref success);
                        if (!success)
                        {
                            success = false;
                            return null;
                        }
                        // :
                        token = nextToken(json, ref index);
                        if (token != Tokens.TOKEN_COLON)
                        {
                            success = false;
                            return null;
                        }
                        // value
                        var value = Convert.ToString(parseValue(json, ref index, ref success), CultureInfo.InvariantCulture);
                        if (!success)
                        {
                            success = false;
                            return null;
                        }
                        table.Add(name, value);
                        break;
                }
            }
        }

        private static object parseValue(char[] json, ref int index, ref bool success)
        {
            switch (lookAhead(json, index))
            {
                case Tokens.TOKEN_STRING:
                    return parseString(json, ref index, ref success);
                case Tokens.TOKEN_NUMBER:
                    return parseNumber(json, ref index, ref success);
                case Tokens.TOKEN_CURLY_OPEN:
                    return parseObject(json, ref index, ref success);
                case Tokens.TOKEN_TRUE:
                    nextToken(json, ref index);
                    return true;
                case Tokens.TOKEN_FALSE:
                    nextToken(json, ref index);
                    return false;
                case Tokens.TOKEN_NULL:
                    nextToken(json, ref index);
                    return null;
                case Tokens.TOKEN_NONE:
                    break;
            }

            success = false;
            return null;
        }

        private static string parseString(char[] json, ref int index, ref bool success)
        {
            var sb = new StringBuilder();

            eatWhitespace(json, ref index);

            // "
            index++;
            var complete = false;
            while (true)
            {
                if (index == json.Length)
                {
                    break;
                }

                var c = json[index++];
                if (c == '"')
                {
                    complete = true;
                    break;
                }
                if (c == '\\')
                {
                    if (index == json.Length)
                    {
                        break;
                    }
                    c = json[index++];
                    if (c == '"')
                    {
                        sb.Append('"');
                    }
                    else if (c == '\\')
                    {
                        sb.Append('\\');
                    }
                    else if (c == '/')
                    {
                        sb.Append('/');
                    }
                    else if (c == 'b')
                    {
                        sb.Append('\b');
                    }
                    else if (c == 'f')
                    {
                        sb.Append('\f');
                    }
                    else if (c == 'n')
                    {
                        sb.Append('\n');
                    }
                    else if (c == 'r')
                    {
                        sb.Append('\r');
                    }
                    else if (c == 't')
                    {
                        sb.Append('\t');
                    }
                    else if (c == 'u')
                    {
                        var remainingLength = json.Length - index;
                        if (remainingLength >= 4)
                        {
                            // parse the 32 bit hex into an integer codepoint
                            uint codePoint;
                            if (!(success = UInt32.TryParse(new string(json, index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out codePoint)))
                            {
                                return "";
                            }
                            // convert the integer codepoint to a unicode char and add to string
                            sb.Append(Char.ConvertFromUtf32((int)codePoint));
                            // skip 4 chars
                            index += 4;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            if (!complete)
            {
                success = false;
                return null;
            }

            return sb.ToString();
        }

        private static double parseNumber(char[] json, ref int index, ref bool success)
        {
            eatWhitespace(json, ref index);

            var lastIndex = getLastIndexOfNumber(json, index);
            var charLength = (lastIndex - index) + 1;

            double number;
            success = Double.TryParse(new string(json, index, charLength), NumberStyles.Any, CultureInfo.InvariantCulture, out number);

            index = lastIndex + 1;
            return number;
        }

        private static int getLastIndexOfNumber(char[] json, int index)
        {
            int lastIndex;

            for (lastIndex = index; lastIndex < json.Length; lastIndex++)
            {
                if ("0123456789+-.eE".IndexOf(json[lastIndex]) == -1)
                {
                    break;
                }
            }
            return lastIndex - 1;
        }

        private static void eatWhitespace(char[] json, ref int index)
        {
            for (; index < json.Length; index++)
            {
                if (" \t\n\r".IndexOf(json[index]) == -1)
                {
                    break;
                }
            }
        }

        private static Tokens lookAhead(char[] json, int index)
        {
            var saveIndex = index;
            return nextToken(json, ref saveIndex);
        }

        private static Tokens nextToken(char[] json, ref int index)
        {
            eatWhitespace(json, ref index);

            if (index == json.Length)
            {
                return Tokens.TOKEN_NONE;
            }

            var c = json[index];
            index++;
            switch (c)
            {
                case '{':
                    return Tokens.TOKEN_CURLY_OPEN;
                case '}':
                    return Tokens.TOKEN_CURLY_CLOSE;
                case '[':
                    return Tokens.TOKEN_BRACKET_OPEN;
                case ']':
                    return Tokens.TOKEN_BRACKET_CLOSE;
                case ',':
                    return Tokens.TOKEN_COMMA;
                case '"':
                    return Tokens.TOKEN_STRING;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '-':
                    return Tokens.TOKEN_NUMBER;
                case ':':
                    return Tokens.TOKEN_COLON;
            }
            index--;

            var remainingLength = json.Length - index;

            // false
            if (remainingLength >= 5)
            {
                if (json[index] == 'f' &&
                    json[index + 1] == 'a' &&
                    json[index + 2] == 'l' &&
                    json[index + 3] == 's' &&
                    json[index + 4] == 'e')
                {
                    index += 5;
                    return Tokens.TOKEN_FALSE;
                }
            }

            // true
            if (remainingLength >= 4)
            {
                if (json[index] == 't' &&
                    json[index + 1] == 'r' &&
                    json[index + 2] == 'u' &&
                    json[index + 3] == 'e')
                {
                    index += 4;
                    return Tokens.TOKEN_TRUE;
                }
            }

            // null
            if (remainingLength >= 4)
            {
                if (json[index] == 'n' &&
                    json[index + 1] == 'u' &&
                    json[index + 2] == 'l' &&
                    json[index + 3] == 'l')
                {
                    index += 4;
                    return Tokens.TOKEN_NULL;
                }
            }

            return Tokens.TOKEN_NONE;
        }
    }
}

