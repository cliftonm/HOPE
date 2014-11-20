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

namespace LinkedIn.NET
{
    /// <summary>
    /// Represents LinkedIn volunteer-experience object
    /// </summary>
    public class LinkedInVolunteerExperience
    {
        /// <summary>
        /// Gets unique identifier for volunteer-experience
        /// </summary>
        public string Id { get; internal set; }
        /// <summary>
        /// Gets volunteer-experience's role
        /// </summary>
        public string Role { get; internal set; }
        /// <summary>
        /// Gets <see cref="LinkedInOrganization"/> object representing volunteer-experience's organization
        /// </summary>
        public LinkedInOrganization Organization { get; private set; }
        /// <summary>
        /// Gets volunteer-experience's cause
        /// </summary>
        public string Cause { get; internal set; }

        internal LinkedInVolunteerExperience()
        {
            Organization = new LinkedInOrganization();
        }
    }
}
