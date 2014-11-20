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

namespace LinkedIn.NET.Options
{
    /// <summary>
    /// Represents object which stores necessary settings for people search
    /// </summary>
    public class LinkedInSearchOptions
    {
        internal const int MAX_COUNT = 25;

        private int ? _Count;

        /// <summary>
        /// Initializes new instance of LinkedInSearchOptions
        /// </summary>
        public LinkedInSearchOptions()
        {
            Keywords = new List<string>();
            MemberFieldOptions = new LinkedInGetMemberOptions();
            FacetFields = new BitField<LinkedInFacetFields>();
            BucketFields = new BitField<LinkedInBucketFields>();
            FacetLanguageValues = new List<LinkedInFacetLanguage>();
            FacetLocationValues = new List<string>();
            FacetIndustryValues = new List<string>();
            FacetCurrentCompanyValues = new List<string>();
            FacetPastCompanyValues = new List<string>();
            FacetSchoolValues = new List<string>();
        }

        /// <summary>
        /// Gets list of strings to search for members who have all the keywords anywhere in their profile.
        /// </summary>
        public List<string> Keywords { get; private set; }

        /// <summary>
        /// Gets or sets the value to search for members with matching first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the value to search for members with matching last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the value to search for members who have a matching company name in their profile.
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// A value of true matches members who currently work at the company specified in the CompanyNames list. A value of false matches members who once worked at the company. Omitting the parameter matches members who currently or once worked the company.
        /// </summary>
        public bool? CurrentCompany { get; set; }

        /// <summary>
        /// Gets or sets the value to search for members who have a matching title in their profile.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// A value of true matches members whose title is currently the one specified in the Titles list. A value of false matches members who once had that title. Omitting the parameter matches members who currently or once had that title.
        /// </summary>
        public bool? CurrentTitle { get; set; }

        /// <summary>
        /// Gets or sets the value to search for members who have a matching school name in their profile.
        /// </summary>
        public string SchoolName { get; set; }

        /// <summary>
        /// A value of true matches members who currently attend the school specified in the SchoolNames list. A value of false matches members who once attended the school. Omitting the parameter matches members who currently or once attended the school.
        /// </summary>
        public bool? CurrentSchool { get; set; }

        /// <summary>
        /// Gets or sets the value to search for members with a location in specified country.
        /// </summary>
        public LinkedInCountries Country { get; set; }

        /// <summary>
        /// Gets or sets the value to search for members centered around a postal code.  Must be combined with the country-code parameter. Not supported for all countries.
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets value indicating to search for members with a distance from a central point. Measured in miles. 
        /// </summary>
        public int? Distance { get; set; }

        /// <summary>
        /// Gets the value indicating start location within the result set for paginated request.
        /// </summary>
        public int? Start { get; set; }

        /// <summary>
        /// Gets the value indicating the number of profiles to return. Values can range between 0 and 25. The default value is null. In this case max 10 (LinkedIn default) results will be returned after each call.
        /// </summary>
        public int ? Count
        {
            get { return _Count; }
            set 
            { 
                _Count = value;
                if (_Count > MAX_COUNT) _Count = MAX_COUNT;
            }
        }

        /// <summary>
        /// Controls the search result order. Can be one of <see cref="LinkedinSearchResultsOrder"/> enumeration values.
        /// </summary>
        public LinkedinSearchResultsOrder Sort { get; set; }

        /// <summary>
        /// The object of type <see cref="LinkedInGetMemberOptions"/> representing retrieval options for member fields
        /// </summary>
        public LinkedInGetMemberOptions MemberFieldOptions { get; private set; }

        /// <summary>
        /// Gets or sets the value indicating aht sets of data should be returned in search results. Can be a combination of <see cref="LinkedInSearchSets"/> enumeration values.
        /// </summary>
        public LinkedInSearchSets SearchSets { get; set; }

        /// <summary>
        /// Gets or sets fields to be retrieved in search result facets.
        /// </summary>
        public BitField<LinkedInFacetFields> FacetFields { get; private set; }

        /// <summary>
        /// Gets or sets fields to be retrieved in search result buckets.
        /// </summary>
        public BitField<LinkedInBucketFields> BucketFields { get; private set; }

        /// <summary>
        /// Gets or sets the value indicating what facet types should be included in query. Can be a combination of <see cref="LinkedInFacetTypes"/> enumeration values.
        /// </summary>
        public LinkedInFacetTypes FacetTypes { get; set; }

        /// <summary>
        /// Gets or sets the value indicating which types of network facet should be included in query. Can be a combination of <see cref="LinkedInFacetNetwork"/> enumeration values.
        /// </summary>
        public LinkedInFacetNetwork FacetNetworkValues { get; set; }

        /// <summary>
        /// Gets list of <see cref="LinkedInFacetLanguage"/> enumeration values indicatinf which languages should be included in query.
        /// </summary>
        public List<LinkedInFacetLanguage> FacetLanguageValues { get; private set; }

        /// <summary>
        /// Gets list of strings indicating which geografical regions should be included in query.
        /// </summary>
        public List<string> FacetLocationValues { get; private set; }

        /// <summary>
        /// Gets list of integers indicating which industries should be included in query.
        /// </summary>
        public List<string> FacetIndustryValues { get; private set; }

        /// <summary>
        /// Gets list of integers indicating which companies should be included in query.
        /// </summary>
        public List<string> FacetCurrentCompanyValues { get; private set; }

        /// <summary>
        /// Gets list of integers indicating which companies should be included in query.
        /// </summary>
        public List<string> FacetPastCompanyValues { get; private set; }

        /// <summary>
        /// Gets list of integers indicating which schools should be included in query.
        /// </summary>
        public List<string> FacetSchoolValues { get; private set; }
    }
}
