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
    /// Represents LinkedIn base company object
    /// </summary>
    public class LinkedInCompanyBase
    {
        internal LinkedInCompanyBase()
        {
        }

        /// <summary>
        /// Gets unique identifier for company
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// Gets company's name
        /// </summary>
        public string Name { get; internal set; }
    }
}
