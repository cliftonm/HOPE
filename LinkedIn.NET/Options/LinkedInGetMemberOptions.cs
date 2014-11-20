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
    /// Represents object which stores necessary settings for retrieving member details from LinkedIn
    /// </summary>
    public class LinkedInGetMemberOptions
    {
        /// <summary>
        /// Initializes new instance of LinkedInGetMemberOptions
        /// </summary>
        public LinkedInGetMemberOptions()
        {
            BasicProfileOptions = new BitField<LinkedInBasicProfileFields>();
            EmailProfileOptions = new BitField<LinkedInEmailProfileFields>();
            FullProfileOptions = new BitField<LinkedInFullProfileFields>();
            Parameters = new LinkedInGetMemberParameters();
        }

        /// <summary>
        /// Gets or sets fields to be retrieved in member's basic profile
        /// </summary>
        public BitField<LinkedInBasicProfileFields> BasicProfileOptions { get; private set; }

        /// <summary>
        /// Gets or sets fields to be retrieved in member's email profile
        /// </summary>
        public BitField<LinkedInEmailProfileFields> EmailProfileOptions { get; private set; }

        /// <summary>
        /// Gets or sets fields to be retrieved in member's full profile
        /// </summary>
        public BitField<LinkedInFullProfileFields> FullProfileOptions { get; private set; }

        /// <summary>
        /// Gets value of <see cref="LinkedInGetMemberParameters"/> indicating the source of member's request (self, member's ID or member's public URL)
        /// </summary>
        public LinkedInGetMemberParameters Parameters { get; private set; }

        /// <summary>
        /// Gets the value indicating whether any field is selected in all profiles
        /// </summary>
        public bool HasValues
        {
            get
            {
                return BasicProfileOptions.HasValues | EmailProfileOptions.HasValues | FullProfileOptions.HasValues;
            }
        }
    }
}
