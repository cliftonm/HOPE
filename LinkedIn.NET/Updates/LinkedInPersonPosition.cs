using System.Collections.Generic;
using System.Linq;
using LinkedIn.NET.Members;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn person object used in PRFU updates
    /// </summary>
    public class LinkedInPersonPosition : LinkedInPerson
    {
        internal LinkedInPersonPosition()
        {
        }

        private readonly List<LinkedInPositionBase> _Positions = new List<LinkedInPositionBase>();

        /// <summary>
        /// Gets collection of <see cref="LinkedInPositionBase"/> ojects representing groups joined by member
        /// </summary>
        public IEnumerable<LinkedInPositionBase> Positions
        {
            get { return _Positions.AsEnumerable(); }
        }

        internal void AddPositions(IEnumerable<LinkedInPositionBase> positions)
        {
            _Positions.AddRange(positions);
        }
    }
}
