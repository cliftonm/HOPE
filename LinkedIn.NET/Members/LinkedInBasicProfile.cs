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

namespace LinkedIn.NET.Members
{
    /// <summary>
    /// Represents LinkedIn basic profile
    /// </summary>
    public class LinkedInBasicProfile : LinkedInPerson
    {
        internal LinkedInBasicProfile()
        {
            Location = new LinkedInLocation();
            RelationToViewer = new LinkedInRelationToViewer();
        }

        private List<LinkedInPosition> _Positions;

        /// <summary>
        /// Gets member's maiden name
        /// </summary>
        public string MaidenName { get; internal set; }
        /// <summary>
        /// Gets member's name formatted based on language
        /// </summary>
        public string FormattedName { get; internal set; }
        /// <summary>
        /// Gets member's first name spelled phonetically
        /// </summary>
        public string PhoneticFirstName { get; internal set; }
        /// <summary>
        /// Gets member's last name spelled phonetically
        /// </summary>
        public string PhoneticLastName { get; internal set; }
        /// <summary>
        /// Gets member's name spelled phonetically and formatted based on language
        /// </summary>
        public string FormattedPhoneticName { get; internal set; }
        /// <summary>
        /// Gets <see cref="LinkedInLocation"/> object representing member's location
        /// </summary>
        public LinkedInLocation Location { get; private set; }
        /// <summary>
        /// Gets the industry the member has indicated their profile belongs to
        /// </summary>
        public string Industry { get; internal set; }
        /// <summary>
        /// Gets the degree distance of the fetched profile from the member who fetched the profile
        /// </summary>
        public int? Distance { get; internal set; }
        /// <summary>
        /// Gets <see cref="LinkedInRelationToViewer"/> object representing the member's relation-to-viewer
        /// </summary>
        public LinkedInRelationToViewer RelationToViewer { get; private set; }
        /// <summary>
        /// Gets <see cref="LinkedInShare"/> object representing the member's current share, if set
        /// </summary>
        public LinkedInShare CurrentShare { get; internal set; }
        /// <summary>
        /// Gets the number of connections the member has
        /// </summary>
        public int? NumConnections { get; internal set; }
        /// <summary>
        /// Gets a value indicating whether the number of connections has been capped at 500
        /// </summary>
        public bool? NumConnectionsCapped { get; internal set; }
        /// <summary>
        /// Gets a value where the member describes their professional profile
        /// </summary>
        public string Summary { get; internal set; }
        /// <summary>
        /// Gets a value where the member enumerates their specialties
        /// </summary>
        public string Specialities { get; internal set; }
        /// <summary>
        /// Gets URL to the member's public profile, if enabled
        /// </summary>
        public string PublicProfileUrl { get; internal set; }
        /// <summary>
        /// Gets collection of <see cref="LinkedInPosition"/> objects representing positions a member has had
        /// </summary>
        public IEnumerable<LinkedInPosition> Positions
        {
            get { return _Positions == null ? null : _Positions.AsEnumerable(); }
        }

        internal void AddPositions(IEnumerable<LinkedInPosition> positions)
        {
            if (_Positions == null) _Positions = new List<LinkedInPosition>();
            _Positions.AddRange(positions);
        }
    }
}
