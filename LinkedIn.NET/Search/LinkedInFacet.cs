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

namespace LinkedIn.NET.Search
{
    /// <summary>
    /// Represents LinkedIn search facet object
    /// </summary>
    public class LinkedInFacet
    {
        internal LinkedInFacet()
        {
        }

        private readonly List<LinkedInBucket> _Buckets = new List<LinkedInBucket>();

        /// <summary>
        /// The machine processable value for the facet.
        /// </summary>
        public string Code { get; internal set; }

        /// <summary>
        /// A human readable name for the facet.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets collection of <see cref="LinkedInBucket"/> objects representing buckets of current facet
        /// </summary>
        public IEnumerable<LinkedInBucket> Buckets
        {
            get { return _Buckets.AsEnumerable(); }
        }

        internal void AddBuckets(IEnumerable<LinkedInBucket> buckets)
        {
            _Buckets.AddRange(buckets);
        }
    }
}
