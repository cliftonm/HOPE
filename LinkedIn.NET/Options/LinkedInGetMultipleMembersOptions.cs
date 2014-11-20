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
    /// Represents object which stores necessary settings for retrieving multiple members details from LinkedIn
    /// </summary>
    public class LinkedInGetMultipleMembersOptions : LinkedInGetMemberOptions
    {
        /// <summary>
        /// Hides unused prperty
        /// </summary>
        private new LinkedInGetMemberParameters Parameters { get; set; }

        /// <summary>
        /// Gets or sets collection of <see cref="LinkedInGetMemberParameters"/> objects representing retrieval parameters
        /// </summary>
        public IEnumerable<LinkedInGetMemberParameters> Params { get; set; }
    }
}
