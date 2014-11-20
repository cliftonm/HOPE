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

namespace LinkedIn.NET.Groups
{
    /// <summary>
    /// Represents LinkedIn group's membership settings
    /// </summary>
    public class LinkedInGroupSettings
    {
        internal LinkedInGroupSettings()
        {
        }

        /// <summary>
        /// Gets or sets the value indicating whether or not to show the group logo in the member's profile
        /// </summary>
        public bool ShowGroupLogoInProfile { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the member allows email contact from non-members
        /// </summary>
        public bool AllowMessagesFromMembers { get; set; }

        /// <summary>
        /// Gets or sets the value indicating the frequency at which the member receives group emails. Can be one of <see cref="LinkedInEmailDigestFrequency"/> enumeration values
        /// </summary>
        public LinkedInEmailDigestFrequency EmailDigestFrequency { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the member allows email messages from group managers
        /// </summary>
        public bool EmailAnnouncementsFromManagers { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the member wants to receive an email message for each new post
        /// </summary>
        public bool EmailForEveryNewPost { get; set; }
    }
}
