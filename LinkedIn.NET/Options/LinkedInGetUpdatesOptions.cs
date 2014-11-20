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

using System;
using LinkedIn.NET.Updates;

namespace LinkedIn.NET.Options
{
    /// <summary>
    /// Represents object which stores settings for retrieving updates from LinkedIn
    /// </summary>
    public class LinkedInGetUpdatesOptions
    {
        /// <summary>
        /// Initializes new instance of LinkedInGetUpdatesOptions
        /// </summary>
        public LinkedInGetUpdatesOptions()
        {
            Parameters = new LinkedInGetMemberParameters();
        }

        private const int MAX_COUNT = 250;

        private int? _UpdateCount;
        private int? _UpdateStart;
        private LinkedInUpdateSortField _SortBy = LinkedInUpdateSortField.UpdateDate;
        private LinkedInUpdateSortDirection _SortDirection = LinkedInUpdateSortDirection.Descending;

        /// <summary>
        /// Gets update's order number to start from
        /// </summary>
        public int? UpdateStart
        {
            get { return _UpdateStart; }
            set
            {
                _UpdateStart = value;
                if (_UpdateStart < 0) _UpdateStart = 0;
            }
        }
        /// <summary>
        /// Gets or sets count of updates to retrieve. Max value is 250
        /// </summary>
        public int? UpdateCount
        {
            get { return _UpdateCount; }
            set 
            { 
                _UpdateCount = value; 
                if (_UpdateCount > MAX_COUNT)_UpdateCount = MAX_COUNT; 
            }
        }
        /// <summary>
        /// Gets or sets updates types to retrieve. Can be a combination of <see cref="LinkedInUpdateType"/> enumeration values
        /// </summary>
        public LinkedInUpdateType UpdateType { get; set; }
        /// <summary>
        /// Gets or sets updates scope. Can be one of <see cref="LinkedInUpdateScope"/> enumeration values. Default value is AggregatedFeed
        /// </summary>
        public LinkedInUpdateScope UpdateScope { get; set; }
        /// <summary>
        /// Gets or sets starting date for retrieving updates
        /// </summary>
        public DateTime? After { get; set; }
        /// <summary>
        /// Gets or sets ending date for retrieving updates
        /// </summary>
        public DateTime? Before { get; set; }
        /// <summary>
        /// Gets or sets value indicating whether updates from hidden members should be retrieved
        /// </summary>
        public bool? ShowHiddenMembers { get; set; }
        /// <summary>
        /// Gets or sets value indicationg how LinkedIn updates will be sorted after retrieving. Can be one of <see cref="LinkedInUpdateSortDirection"/> enumeration values. Default value is Descending
        /// </summary>
        public LinkedInUpdateSortDirection SortDirection
        {
            get { return _SortDirection; }
            set { _SortDirection = value; }
        }
        /// <summary>
        /// Gets or sets <see cref="LinkedInUpdate"/> field which will be used as sort field while retrieving LinkedIn updates. Can be one of <see cref="LinkedInUpdateSortField"/> enumeration values. Default value is UpdateDate
        /// </summary>
        public LinkedInUpdateSortField SortBy
        {
            get { return _SortBy; }
            set { _SortBy = value; }
        }

        /// <summary>
        /// Gets value of <see cref="LinkedInGetMemberParameters"/> indicating the source of updates request (self, member's ID or member's public URL)
        /// </summary>
        public LinkedInGetMemberParameters Parameters { get; private set; }
    }
}
