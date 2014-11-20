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
    /// Represents LinkedIn following object
    /// </summary>
    public class LinkedInFollowing
    {
        private readonly List<LinkedInPerson> _People = new List<LinkedInPerson>();
        private readonly List<LinkedInCompany> _Companies = new List<LinkedInCompany>();
        private readonly List<LinkedInIndustry> _Industries = new List<LinkedInIndustry>();
        private readonly List<LinkedInSpecialEdition> _SpecialEditions = new List<LinkedInSpecialEdition>();

        internal LinkedInFollowing()
        {
        }

        /// <summary>
        /// Gets collection of <see cref="LinkedInPerson"/> objects that following object has had
        /// </summary>
        public IEnumerable<LinkedInPerson> People
        {
            get { return _People.AsEnumerable(); }
        }
        /// <summary>
        /// Gets collection of <see cref="LinkedInCompany"/> objects that following object has had
        /// </summary>
        public IEnumerable<LinkedInCompany> Companies
        {
            get { return _Companies.AsEnumerable(); }
        }
        /// <summary>
        /// Gets collection of <see cref="LinkedInIndustry"/> objects that following object has had
        /// </summary>
        public IEnumerable<LinkedInIndustry> Industries
        {
            get { return _Industries.AsEnumerable(); }
        }
        /// <summary>
        /// Gets collection of <see cref="LinkedInSpecialEdition"/> objects that following object has had
        /// </summary>
        public IEnumerable<LinkedInSpecialEdition> SpecialEditions
        {
            get { return _SpecialEditions.AsEnumerable(); }
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

        internal void AddSpecialEditions(IEnumerable<LinkedInSpecialEdition> specialEditions)
        {
            _SpecialEditions.AddRange(specialEditions);
        }
    }
}
