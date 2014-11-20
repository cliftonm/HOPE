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
    /// Represents LinkedIn company object
    /// </summary>
    public class LinkedInCompany : LinkedInCompanyBase
    {
        /// <summary>
        /// Gets the name of industry in which the company operates
        /// </summary>
        public string Industry { get; internal set; }
        /// <summary>
        /// Gets company's number of employees
        /// </summary>
        public string Size { get; internal set; }
        /// <summary>
        /// Gets stock market name for the company, if the company type is public
        /// </summary>
        public string Ticker { get; internal set; }
        /// <summary>
        /// Gets a value indicating whether the company is public or private
        /// </summary>
        public string Type { get; internal set; }

        internal LinkedInCompany()
        {
        }
    }
}
