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

namespace LinkedIn.NET
{
    /// <summary>
    /// Represents LinkedIn to-follow object
    /// </summary>
    public class LinkedInToFollow
    {
        private readonly List<LinkedInPerson> _People = new List<LinkedInPerson>();
        private readonly List<LinkedInCompany> _Companies = new List<LinkedInCompany>();
        private readonly List<LinkedInIndustry> _Industries = new List<LinkedInIndustry>();
        private readonly List<LinkedInNewsSource> _NewsSources = new List<LinkedInNewsSource>();

        internal LinkedInToFollow()
        {
        }

        /// <summary>
        /// Gets collections of <see cref="LinkedInPerson"/>objects representing people suggested for the member to follow
        /// </summary>
        public IEnumerable<LinkedInPerson> People
        {
            get { return _People.AsEnumerable(); }
        }
        /// <summary>
        /// Gets collections of <see cref="LinkedInCompany"/>objects representing companies suggested for the member to follow
        /// </summary>
        public IEnumerable<LinkedInCompany> Companies
        {
            get { return _Companies.AsEnumerable(); }
        }
        /// <summary>
        /// Gets collections of <see cref="LinkedInIndustry"/>objects representing industries suggested for the member to follow
        /// </summary>
        public IEnumerable<LinkedInIndustry> Industries
        {
            get { return _Industries.AsEnumerable(); }
        }
        /// <summary>
        /// Gets collections of <see cref="LinkedInNewsSource"/>objects representing news sources suggested for the member to follow
        /// </summary>
        public IEnumerable<LinkedInNewsSource> NewsSources
        {
            get { return _NewsSources.AsEnumerable(); }
        }

        internal void AddPeople(IEnumerable<LinkedInPerson> people)
        {
            _People.AddRange(people);
        }

        internal void AddCompanies(IEnumerable<LinkedInCompany> companies)
        {
            _Companies.AddRange(companies);
        }

        internal void AddIndustries(IEnumerable<LinkedInIndustry> industries)
        {
            _Industries.AddRange(industries);
        }

        internal void AddNewsSources(IEnumerable<LinkedInNewsSource> sources)
        {
            _NewsSources.AddRange(sources);
        }
    }
}
