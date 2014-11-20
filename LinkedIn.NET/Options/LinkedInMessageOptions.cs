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

using System.Collections.Generic;

namespace LinkedIn.NET.Options
{
    /// <summary>
    /// Represents object which stores necessary settings for sending message
    /// </summary>
    public class LinkedInMessageOptions
    {
        private readonly List<string> _Recipients = new List<string>();
        private string _Subject;

        /// <summary>
        /// Gets or sets message's subject
        /// </summary>
        public string Subject
        {
            get { return _Subject; }
            set 
            { 
                _Subject = value;
                if (_Subject.Length > 200) _Subject = _Subject.Substring(0, 200);
            }
        }

        /// <summary>
        /// Gets or sets message's body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets list of message's recipients
        /// </summary>
        /// <remarks>Each recipient should be represented by member id</remarks>
        public List<string> Recipients
        {
            get { return _Recipients; }
        }

        /// <summary>
        /// Gets or sets the value indicating whether sender of message should be included in recipients
        /// </summary>
        public bool IncludeSenderInRecipients { get; set; }
    }
}
