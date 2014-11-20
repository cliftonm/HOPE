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

namespace LinkedIn.NET.Search
{
    /// <summary>
    /// Represents LinkedIn search bucket object
    /// </summary>
    public class LinkedInBucket
    {
        internal LinkedInBucket()
        {
        }

        /// <summary>
        /// The machine processable value for the bucket.
        /// </summary>
        public string Code { get; internal set; }

        /// <summary>
        /// A human readable name for the bucket.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// The number of results inside the bucket.
        /// </summary>
        public int? Count { get; internal set; }

        /// <summary>
        /// Gets the value indicating if this bucket's results are included in your search query.
        /// </summary>
        public bool? Selected { get; internal set; }
    }
}
