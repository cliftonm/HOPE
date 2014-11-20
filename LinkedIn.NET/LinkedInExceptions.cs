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

namespace LinkedIn.NET
{
    /// <summary>
    /// Represents missing credential (API Key or Secret Key) exception
    /// </summary>
    public class LinkedInMissingCredentialException : Exception
    {
        internal LinkedInMissingCredentialException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Represents authentication failure exception
    /// </summary>
    public class LinkedInAuthenticationFailedException : Exception
    {
        internal LinkedInAuthenticationFailedException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Represents missing user id exception
    /// </summary>
    public class LinkedInMissingMemberIdException : ArgumentException
    {
        internal LinkedInMissingMemberIdException(string message, string paramName)
            : base(message, paramName)
        {
        }
    }

    /// <summary>
    /// Represents missing profile URL exception
    /// </summary>
    public class LinkedInMissingProfileUrlException : ArgumentException
    {
        internal LinkedInMissingProfileUrlException(string message, string paramName)
            : base(message, paramName)
        {
        }
    }

    /// <summary>
    /// Represents invalid profile URL exception
    /// </summary>
    public class LinkedInInvalidProfileUrlException : ArgumentException
    {
        internal LinkedInInvalidProfileUrlException(string message, string paramName)
            : base(message, paramName)
        {
        }
    }

    /// <summary>
    /// Represents negative parameter exception
    /// </summary>
    public class LinkedInNegativeParameterException : ArgumentException
    {
        internal LinkedInNegativeParameterException(string message, string paramName)
            : base(message, paramName)
        {
        }
    }

    /// <summary>
    /// Represents exception thrown when some parameter is missing
    /// </summary>
    public class LinkedInMissingParameterException : ArgumentException
    {
        internal LinkedInMissingParameterException(string message, string paramName)
            : base(message, paramName)
        {
        }
    }

    /// <summary>
    /// Represents exception thrown when 'count' parameter of request is equal to 0
    /// </summary>
    public class LinkedInCountIsZeroException : Exception
    {
        internal LinkedInCountIsZeroException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Represents authentication abort exception
    /// </summary>
    public class LinkedInAuthenticationAbortedException : Exception
    {
        internal LinkedInAuthenticationAbortedException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Represents exception thrown when application attempts to send message without recipients
    /// </summary>
    public class LinkedInNoRecipientsException : Exception
    {
        internal LinkedInNoRecipientsException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Represents exception thrown when user attempts to join the group he is already member of
    /// </summary>
    public class LinkedInAlreadyMemberException : Exception
    {
        internal LinkedInAlreadyMemberException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Represents exception thrown when user attempts to make operations with group he is not a member of
    /// </summary>
    public class LinkedInIsNotMemberException : Exception
    {
        internal LinkedInIsNotMemberException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Represents exception thrown when user attempts to make not allowed operations with group's post
    /// </summary>
    public class LinkedInInvalidOperationException : Exception
    {
        internal LinkedInInvalidOperationException(string message)
            : base(message)
        {
        }
    }
}
