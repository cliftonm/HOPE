using System.Collections.Generic;
using System.Linq;
using LinkedIn.NET.Members;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn person object used in JGRP updates
    /// </summary>
    public class LinkedInPersonGroup : LinkedInPerson
    {
        internal LinkedInPersonGroup()
        {
        }

        private readonly List<LinkedInMemberGroup> _MemberGroups = new List<LinkedInMemberGroup>();

        /// <summary>
        /// Gets collection of <see cref="LinkedInMemberGroup"/> ojects representing groups joined by member
        /// </summary>
        public IEnumerable<LinkedInMemberGroup> MemberGroups
        {
            get { return _MemberGroups.AsEnumerable(); }
        }

        internal void AddGroups(IEnumerable<LinkedInMemberGroup> groups)
        {
            _MemberGroups.AddRange(groups);
        }
    }
}
