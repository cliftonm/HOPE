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
using LinkedIn.NET.Members;

namespace LinkedIn.NET.Search
{
    /// <summary>
    /// Represents object to store results of search at LinkedIn.
    /// </summary>
    public class LinkedInSearchResult
    {
        internal LinkedInSearchResult()
        {
        }

        private List<LinkedInMember> _People;
        private List<LinkedInFacet> _Facets;

        /// <summary>
        /// Gets value indicating total results count available for current search.
        /// </summary>
        public int ? TotalResultsCount { get; internal set; }

        /// <summary>
        /// Gets value indicating fetched results count for current search.
        /// </summary>
        public int ? FetchedResultsCount { get; internal set; }

        /// <summary>
        /// Gets collection of <see cref="LinkedInMember"/> objects returned by current search.
        /// </summary>
        public IEnumerable<LinkedInMember> People
        {
            get { return _People == null ? null : _People.AsEnumerable(); }
        }

        /// <summary>
        /// Gets collection of <see cref="LinkedInFacet"/> objects returned by current search.
        /// </summary>
        public IEnumerable<LinkedInFacet> Facets
        {
            get { return _Facets == null ? null : _Facets.AsEnumerable(); }
        }

        internal void AddPeople(IEnumerable<LinkedInMember> people)
        {
            if (_People == null) _People = new List<LinkedInMember>();
            _People.AddRange(people);
        }

        internal void AddFacets(IEnumerable<LinkedInFacet> facets)
        {
            if (_Facets == null) _Facets = new List<LinkedInFacet>();
            _Facets.AddRange(facets);
        }
    }
}
