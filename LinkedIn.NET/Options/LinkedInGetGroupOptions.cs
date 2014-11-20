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
    /// Represents base class for groups retrieving options
    /// </summary>
    public class LinkedInGetGroupOptions
    {
        /// <summary>
        /// Initializes new instance of LinkedInGetGroupsOptions.
        /// </summary>
        public LinkedInGetGroupOptions()
        {
            GroupOptions = new BitField<LinkedInGroupFields>();
        }

        /// <summary>
        /// Gets or sets the fields to be retrieved for group.
        /// </summary>
        public BitField<LinkedInGroupFields> GroupOptions { get; private set; }
    }
}
