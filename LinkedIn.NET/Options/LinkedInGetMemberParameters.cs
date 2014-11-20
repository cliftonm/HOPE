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

namespace LinkedIn.NET.Options
{
    /// <summary>
    /// Represents parameters for member request
    /// </summary>
    public class LinkedInGetMemberParameters
    {
        /// <summary>
        /// Gets or sets the value indicating how the Param property should be treated. Can be one of <see cref="LinkedInGetMemberBy"/> enumeration values
        /// </summary>
        public LinkedInGetMemberBy GetBy { get; set; }

        /// <summary>
        /// Gets or sets each member source for multiple members request. If <see cref="GetBy"/> property is set to <see cref="LinkedInGetMemberBy.Self"/> the value is ignored. If GetBy property is set to <see cref="LinkedInGetMemberBy.Id"/> the value should be member's id. If GetBy property is set to <see cref="LinkedInGetMemberBy.Url"/> the value should be member's public URL.
        /// </summary>
        public string RequestBy { get; set; }
    }
}
