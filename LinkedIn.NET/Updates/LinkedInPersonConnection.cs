using System.Collections.Generic;
using System.Linq;
using LinkedIn.NET.Members;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn person object used in CONN connection updates
    /// </summary>
    public class LinkedInPersonConnection : LinkedInPerson
    {
        internal LinkedInPersonConnection()
        {
        }

        private readonly List<LinkedInPerson> _Connections = new List<LinkedInPerson>();

        /// <summary>
        /// Gets collection of <see cref="LinkedInPerson"/> ojects representing members that were recently connected to person
        /// </summary>
        public IEnumerable<LinkedInPerson> Connections
        {
            get { return _Connections.AsEnumerable(); }
        }

        internal void AddConnections(IEnumerable<LinkedInPerson> connections)
        {
            _Connections.AddRange(connections);
        }
    }
}
