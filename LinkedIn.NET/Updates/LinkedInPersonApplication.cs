using System.Collections.Generic;
using System.Linq;
using LinkedIn.NET.Members;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn person object used in APPS/APPM updates
    /// </summary>
    public class LinkedInPersonApplication : LinkedInPerson
    {
        internal LinkedInPersonApplication()
        {
        }

        private readonly List<LinkedInActivity> _Activities = new List<LinkedInActivity>();

        /// <summary>
        /// Gets collection of <see cref="LinkedInActivity"/> ojects representing application activities
        /// </summary>
        public IEnumerable<LinkedInActivity> Activities
        {
            get { return _Activities.AsEnumerable(); }
        }

        internal void AddActivities(IEnumerable<LinkedInActivity> activities)
        {
            _Activities.AddRange(activities);
        }
    }
}
