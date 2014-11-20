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
using LinkedIn.NET.Members;

namespace LinkedIn.NET
{
    /// <summary>
    /// Represents LinkedIn share object
    /// </summary>
    public class LinkedInShare
    {
        /// <summary>
        /// Gets unique identifier for share
        /// </summary>
        public string Id { get; internal set; }
        /// <summary>
        /// Gets share's timestamp
        /// </summary>
        public DateTime ShareDate { get; internal set; }
        /// <summary>
        /// Gets <see cref="LinkedInShareVisibilityCode"/> object representing the share's visibility
        /// </summary>
        public LinkedInShareVisibilityCode VisibilityCode { get; internal set; }
        /// <summary>
        /// Gets share's comment text
        /// </summary>
        public string Comment { get; internal set; }
        /// <summary>
        /// Gets share's source service provider
        /// </summary>
        public string SourceServiceProvider { get; internal set; }
        /// <summary>
        /// Gets share's source application, if any
        /// </summary>
        public string SourceApplication { get; internal set; }
        /// <summary>
        /// Gets <see cref="LinkedInShareContent"/> object representing the share's content
        /// </summary>
        public LinkedInShareContent Content { get; internal set; }
        /// <summary>
        /// Gets <see cref="LinkedInPerson"/> object representing the share's author
        /// </summary>
        public LinkedInPerson Author { get; internal set; }

        internal LinkedInShare()
        {
        }
    }
}
