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
    /// Represents LinkedIn job bookmark object
    /// </summary>
    public class LinkedInJobBookmark
    {
        /// <summary>
        /// Gets value indicationg whether the job bookmark is applied
        /// </summary>
        public bool IsApplied { get; internal set; }
        /// <summary>
        /// Gets value indicationg whether the job bookmark is saved
        /// </summary>
        public bool IsSaved { get; internal set; }
        /// <summary>
        /// Gets job bookmark's saving timestamp
        /// </summary>
        public DateTime SavedTimestamp { get; internal set; }
        /// <summary>
        /// Gets <see cref="LinkedInJob"/> object representing bookmark's job
        /// </summary>
        public LinkedInJob Job { get; internal set; }

        internal LinkedInJobBookmark()
        {
        }
    }
}
