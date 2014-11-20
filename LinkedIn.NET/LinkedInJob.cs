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

namespace LinkedIn.NET
{
    /// <summary>
    /// Represents LinkedIn job object
    /// </summary>
    public class LinkedInJob
    {
        /// <summary>
        /// Gets unique identifier for job
        /// </summary>
        public string Id { get; internal set; }
        /// <summary>
        /// Gets value indicationg whether the job is active
        /// </summary>
        public bool ? Active { get; internal set; }
        /// <summary>
        /// Gets <see cref="LinkedInCompanyBase"/> object representing job's company
        /// </summary>
        public LinkedInCompanyBase Company { get; internal set; }
        /// <summary>
        /// Gets job's position title
        /// </summary>
        public string PositionTitle { get; internal set; }
        /// <summary>
        /// Gets job's description snippet
        /// </summary>
        public string DescriptionSnippet { get; internal set; }
        /// <summary>
        /// Gets job's description
        /// </summary>
        public string Description { get; internal set; }
        /// <summary>
        /// Gets job's posting timestamp
        /// </summary>
        public DateTime PostingTimestamp { get; internal set; }
        /// <summary>
        /// Gets job's location
        /// </summary>
        public string LocationDescription { get; internal set; }
        /// <summary>
        /// Gets job's site request URL
        /// </summary>
        public string JobSiteRequestUrl { get; internal set; }

        internal LinkedInJob()
        {
        }
    }
}
