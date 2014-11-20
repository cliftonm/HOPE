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

using LinkedIn.NET.Members;

namespace LinkedIn.NET
{
    /// <summary>
    /// Represents LinkedIn base recommendation object
    /// </summary>
    public class LinkedInRecommendation
    {
        internal LinkedInRecommendation()
        {
        }

        /// <summary>
        /// Gets unique identifier for recommendation
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// Gets recommendation's type code
        /// </summary>
        public string TypeCode { get; internal set; }

        /// <summary>
        /// Gets recommendation's text
        /// </summary>
        public string Text { get; internal set; }

        /// <summary>
        /// Gets value indicating recommendation's type - given or received. Can be one of <see cref="LinkedInRecommendationType"/> enumeration values
        /// </summary>
        public LinkedInRecommendationType RecommendationType { get; internal set; }

        /// <summary>
        /// Gets recommendation's URL
        /// </summary>
        public string WebUrl { get; internal set; }

        /// <summary>
        /// Gets <see cref="LinkedInPerson"/> object representing the person who made or received the recommendation, accordingly to RecommendationType value
        /// </summary>
        public LinkedInPerson RecommendationPerson { get; internal set; }
    }
}
