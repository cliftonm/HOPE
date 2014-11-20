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

namespace LinkedIn.NET.Options
{
    /// <summary>
    /// Represents object which stores necessary settings for retrieving list of groups details from LinkedIn
    /// </summary>
    public class LinkedInGetListGroupsOptions : LinkedInGetGroupOptions
    {
        internal const int MAX_COUNT = 25;

        private int? _Count;

        /// <summary>
        /// Gets the value indicating the number of groups to return. Values can range between 0 and 25. The default value is null. In this case all possible results will be fetched and number of groups returned on each subsequent request in inner loop will be equal to 25.
        /// </summary>
        public int? Count
        {
            get { return _Count; }
            set
            {
                _Count = value;
                if (_Count > MAX_COUNT)
                    _Count = MAX_COUNT;
            }
        }

        /// <summary>
        /// Gets the value indicating start location within the result set for paginated request.
        /// </summary>
        public int Start { get; set; }
    }
}
