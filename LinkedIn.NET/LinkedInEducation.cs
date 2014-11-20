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
    /// Represents LinkedIn education object
    /// </summary>
    public class LinkedInEducation
    {
        /// <summary>
        /// Gets unique identifier for education
        /// </summary>
        public string Id { get; internal set; }
        /// <summary>
        /// Gets the name of the school
        /// </summary>
        public string SchoolName { get; internal set; }
        /// <summary>
        /// Gets the field of study at the school
        /// </summary>
        public string FieldOfStudy { get; internal set; }
        /// <summary>
        /// Gets <see cref="LinkedInDate"/> object indicating when the education began
        /// </summary>
        public LinkedInDate StartDate { get; internal set; }
        /// <summary>
        /// Gets <see cref="LinkedInDate"/> object indicating when the education ended
        /// </summary>
        public LinkedInDate EndDate { get; internal set; }
        /// <summary>
        /// Gets description of the degree, if any, received at this institution
        /// </summary>
        public string Degree { get; internal set; }
        /// <summary>
        /// Gets description of the activities the member was involved in while a student at this institution
        /// </summary>
        public string Activities { get; internal set; }
        /// <summary>
        /// Gets description of other details on the member's studies
        /// </summary>
        public string Notes { get; internal set; }

        internal LinkedInEducation()
        {
        }
    }
}
