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
    /// Represents LinkedIn certification object
    /// </summary>
    public class LinkedInCertification
    {
        /// <summary>
        /// Gets unique identifier for certification
        /// </summary>
        public string Id { get; internal set; }
        /// <summary>
        /// Gets certification's name
        /// </summary>
        public string Name { get; internal set; }
        /// <summary>
        /// Gets certification's authority name
        /// </summary>
        public string AuthorityName { get; internal set; }
        /// <summary>
        /// Gets certification's number
        /// </summary>
        public string Number { get; internal set; }
        /// <summary>
        /// Gets <see cref="LinkedInDate"/> object representing certification's start date
        /// </summary>
        public LinkedInDate StartDate { get; internal set; }

        internal LinkedInCertification()
        {
        }
    }
}
