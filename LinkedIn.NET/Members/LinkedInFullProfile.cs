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
using System.Collections.Generic;
using System.Linq;

namespace LinkedIn.NET.Members
{
    /// <summary>
    /// Represents LinkedIn full profile
    /// </summary>
    public class LinkedInFullProfile
    {
        internal LinkedInFullProfile()
        {
        }

        private List<LinkedInPublication> _Publications;
        private List<LinkedInPatent> _Patents;
        private List<LinkedInLanguage> _Languages;
        private List<LinkedInSkill> _Skills;
        private List<LinkedInEducation> _Educations;
        private List<LinkedInCertification> _Certifications;
        private List<LinkedInCourse> _Courses;
        private List<LinkedInVolunteerExperience> _Volunteer;
        private List<LinkedInPosition> _ThreeCurrentPositions;
        private List<LinkedInPosition> _ThreePastPositions;
        private List<LinkedInPerson> _RelatedProfileViews;
        private List<LinkedInMemberUrl> _MemberUrls;
        private List<LinkedInRecommendation> _RecommendationsReceived;
        private List<LinkedInJobBookmark> _JobBookmarks;
        private List<LinkedInPerson> _Connections;

        /// <summary>
        /// Gets the timestamp when the member's profile was last edited
        /// </summary>
        public DateTime? LastModifiedTimestamp { get; internal set; }
        /// <summary>
        /// Gets description how the member approaches proposals
        /// </summary>
        public string ProposalComments { get; internal set; }
        /// <summary>
        /// Gets a string enumerating the Associations a member has
        /// </summary>
        public string Associations { get; internal set; }
        /// <summary>
        /// Gets description of the member's interests
        /// </summary>
        public string Interests { get; internal set; }
        /// <summary>
        /// Gets collection of <see cref="LinkedInPublication"/> objects representing publications authored by this member
        /// </summary>
        public IEnumerable<LinkedInPublication> Publications
        {
            get { return _Publications == null ? null : _Publications.AsEnumerable(); }
        }
        /// <summary>
        /// Gets collection of <see cref="LinkedInPatent"/> objects representing patents or patent applications held by this member
        /// </summary>
        public IEnumerable<LinkedInPatent> Patents
        {
            get { return _Patents == null ? null : _Patents.AsEnumerable(); }
        }
        /// <summary>
        /// Gets collection of <see cref="LinkedInLanguage"/> objects representing languages and the level of the member's proficiency for each
        /// </summary>
        public IEnumerable<LinkedInLanguage> Languages
        {
            get { return _Languages == null ? null : _Languages.AsEnumerable(); }
        }
        /// <summary>
        /// Gets collection of <see cref="LinkedInSkill"/> objects representing skills held by this member
        /// </summary>
        public IEnumerable<LinkedInSkill> Skills
        {
            get { return _Skills == null ? null : _Skills.AsEnumerable(); }
        }
        /// <summary>
        /// Gets collection of <see cref="LinkedInEducation"/> objects representing education institutions a member has attended
        /// </summary>
        public IEnumerable<LinkedInEducation> Educations
        {
            get { return _Educations == null ? null : _Educations.AsEnumerable(); }
        }
        /// <summary>
        /// Gets collection of <see cref="LinkedInCertification"/> objects representing certifications earned by this member
        /// </summary>
        public IEnumerable<LinkedInCertification> Certifications
        {
            get { return _Certifications == null ? null : _Certifications.AsEnumerable(); }
        }
        /// <summary>
        /// Gets collection of <see cref="LinkedInCourse"/> objects representing courses a member has taken
        /// </summary>
        public IEnumerable<LinkedInCourse> Courses
        {
            get { return _Courses == null ? null : _Courses.AsEnumerable(); }
        }
        /// <summary>
        /// Gets collection of <see cref="LinkedInVolunteerExperience"/> objects representing volunteering experiences a member has participated in, including organizations and causes
        /// </summary>
        public IEnumerable<LinkedInVolunteerExperience> Volunteer
        {
            get { return _Volunteer == null ? null : _Volunteer.AsEnumerable(); }
        }
        /// <summary>
        /// Gets collection of <see cref="LinkedInPosition"/> objects representing positions a member currently holds, limited to three
        /// </summary>
        public IEnumerable<LinkedInPosition> ThreeCurrentPositions
        {
            get { return _ThreeCurrentPositions == null ? null : _ThreeCurrentPositions.AsEnumerable(); }
        }
        /// <summary>
        /// Gets collection of <see cref="LinkedInPosition"/> objects representing positions a member formerly held, limited to the three most recent
        /// </summary>
        public IEnumerable<LinkedInPosition> ThreePastPositions
        {
            get { return _ThreePastPositions == null ? null : _ThreePastPositions.AsEnumerable(); }
        }
        /// <summary>
        /// Gets collection of <see cref="LinkedInPerson"/> objects representing related profiles that were viewed before or after the member's profile
        /// </summary>
        public IEnumerable<LinkedInPerson> RelatedProfileViews
        {
            get { return _RelatedProfileViews == null ? null : _RelatedProfileViews.AsEnumerable(); }
        }
        /// <summary>
        /// Gets collection of <see cref="LinkedInPerson"/> objects representing member's connection
        /// </summary>
        public IEnumerable<LinkedInPerson> Connections
        {
            get { return _Connections == null ? null : _Connections.AsEnumerable(); }
        }
        /// <summary>
        /// Gets collection of <see cref="LinkedInMemberUrl"/> objects representing URLs the member has chosen to share on their LinkedIn profile
        /// </summary>
        public IEnumerable<LinkedInMemberUrl> MemberUrls
        {
            get { return _MemberUrls == null ? null : _MemberUrls.AsEnumerable(); }
        }
        /// <summary>
        /// Gets member's birth date
        /// </summary>
        public LinkedInDate DateOfBirth { get; internal set; }
        /// <summary>
        /// Gets the number of recommendations the member has
        /// </summary>
        public int ? NumRecommenders { get; internal set; }
        /// <summary>
        /// Gets collection of <see cref="LinkedInRecommendation"/> objects representing recommendations a member has received
        /// </summary>
        public IEnumerable<LinkedInRecommendation> RecommendationsReceived
        {
            get { return _RecommendationsReceived == null ? null : _RecommendationsReceived.AsEnumerable(); }
        }
        /// <summary>
        /// Gets a URL for the member's multiple feeds
        /// </summary>
        public string MfeedRssUrl { get; internal set; }
        /// <summary>
        /// Gets collection of <see cref="LinkedInJobBookmark"/> objects representing jobs that the member is following
        /// </summary>
        public IEnumerable<LinkedInJobBookmark> JobBookmarks
        {
            get { return _JobBookmarks == null ? null : _JobBookmarks.AsEnumerable(); }
        }
        /// <summary>
        /// Gets collection of <see cref="LinkedInFollowing"/> objects representing people, companies and industries that the member is following
        /// </summary>
        public LinkedInFollowing Following { get; internal set; }
        /// <summary>
        /// Gets collection of <see cref="LinkedInSuggestions"/> objects representing people, companies and industries suggested for the member to follow
        /// </summary>
        public LinkedInSuggestions Suggestions { get; internal set; }

        internal void AddPublications(IEnumerable<LinkedInPublication> publications)
        {
            if (_Publications == null) _Publications = new List<LinkedInPublication>();
            _Publications.AddRange(publications);
        }

        internal void AddPatents(IEnumerable<LinkedInPatent> patents)
        {
            if (_Patents == null) _Patents = new List<LinkedInPatent>();
            _Patents.AddRange(patents);
        }

        internal void AddLanguages(IEnumerable<LinkedInLanguage> languages)
        {
            if (_Languages == null) _Languages = new List<LinkedInLanguage>();
            _Languages.AddRange(languages);
        }

        internal void AddSkills(IEnumerable<LinkedInSkill> skills)
        {
            if (_Skills == null) _Skills = new List<LinkedInSkill>();
            _Skills.AddRange(skills);
        }

        internal void AddEducations(IEnumerable<LinkedInEducation> educations)
        {
            if (_Educations == null) _Educations = new List<LinkedInEducation>();
            _Educations.AddRange(educations);
        }

        internal void AddCertifications(IEnumerable<LinkedInCertification> certifications)
        {
            if (_Certifications == null) _Certifications = new List<LinkedInCertification>();
            _Certifications.AddRange(certifications);
        }

        internal void AddCourses(IEnumerable<LinkedInCourse> courses)
        {
            if (_Courses == null) _Courses = new List<LinkedInCourse>();
            _Courses.AddRange(courses);
        }

        internal void AddVolunteer(IEnumerable<LinkedInVolunteerExperience> voluteer)
        {
            if (_Volunteer == null) _Volunteer = new List<LinkedInVolunteerExperience>();
            _Volunteer.AddRange(voluteer);
        }

        internal void AddPositions(IEnumerable<LinkedInPosition> positions, bool current)
        {
            if (current)
            {
                if (_ThreeCurrentPositions == null) _ThreeCurrentPositions = new List<LinkedInPosition>();
                _ThreeCurrentPositions.AddRange(positions);
            }
            else
            {
                if (_ThreePastPositions == null) _ThreePastPositions = new List<LinkedInPosition>();
                _ThreePastPositions.AddRange(positions);
            }
        }

        internal void AddRelatedProfileViews(IEnumerable<LinkedInPerson> persons)
        {
            if (_RelatedProfileViews == null) _RelatedProfileViews = new List<LinkedInPerson>();
            _RelatedProfileViews.AddRange(persons);
        }

        internal void AddMemberUrls(IEnumerable<LinkedInMemberUrl> urls)
        {
            if (_MemberUrls == null) _MemberUrls = new List<LinkedInMemberUrl>();
            _MemberUrls.AddRange(urls);
        }

        internal void AddRecommendations(IEnumerable<LinkedInRecommendation> recommendations)
        {
            if (_RecommendationsReceived == null) _RecommendationsReceived = new List<LinkedInRecommendation>();
            _RecommendationsReceived.AddRange(recommendations);
        }

        internal void AddJobBookmarks(IEnumerable<LinkedInJobBookmark> jbookmarks)
        {
            if (_JobBookmarks == null) _JobBookmarks = new List<LinkedInJobBookmark>();
            _JobBookmarks.AddRange(jbookmarks);
        }

        internal void AddConnections(IEnumerable<LinkedInPerson> connections)
        {
            if (_Connections == null) _Connections = new List<LinkedInPerson>();
            _Connections.AddRange(connections);
        }
    }
}
