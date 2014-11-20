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

using System.Collections.Generic;
using System.Linq;

namespace LinkedIn.NET
{
    /// <summary>
    /// Represents LinkedIn API standard profile request object
    /// </summary>
    public class LinkedInApiStandardProfileRequest
    {
        private readonly List<LinkedInHttpHeader> _Headers = new List<LinkedInHttpHeader>();

        /// <summary>
        /// Gets URL representing the resource you would request for programmatic access to the member's profile
        /// </summary>
        public string Url { get; internal set; }
        /// <summary>
        /// Gets collection of <see cref="LinkedInHttpHeader"/> objects representing HTTP headers
        /// </summary>
        public IEnumerable<LinkedInHttpHeader> Headers
        {
            get { return _Headers.AsEnumerable(); }
        }

        internal LinkedInApiStandardProfileRequest()
        {
        }

        internal void AddHeader(LinkedInHttpHeader h)
        {
            _Headers.Add(h);
        }

        internal void AddHeaders(IEnumerable<LinkedInHttpHeader> headers)
        {
            _Headers.AddRange(headers);
        }
    }
}
