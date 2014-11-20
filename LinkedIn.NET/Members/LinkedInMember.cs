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

namespace LinkedIn.NET.Members
{
    /// <summary>
    /// Represents LinkedIn member object
    /// </summary>
    public class LinkedInMember
    {
        internal LinkedInMember()
        {
        }

        /// <summary>
        /// Gets member's basic profile.
        /// </summary>
        public LinkedInBasicProfile BasicProfile { get; internal set; }

        /// <summary>
        /// Gets member's email profile.
        /// </summary>
        public LinkedInEmailProfile EmailProfile { get; internal set; }

        /// <summary>
        /// Gets members full profile.
        /// </summary>
        public LinkedInFullProfile FullProfile { get; internal set; }
    }
}
