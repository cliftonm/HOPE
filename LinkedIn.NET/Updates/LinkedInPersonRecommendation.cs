using System.Collections.Generic;
using System.Linq;
using LinkedIn.NET.Members;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn person object used in PREC/SVPR updates
    /// </summary>
    public class LinkedInPersonRecommendation : LinkedInPerson
    {
        internal LinkedInPersonRecommendation()
        {
        }

        private readonly List<LinkedInRecommendation> _Recommendations = new List<LinkedInRecommendation>();

        /// <summary>
        /// Gets collection of <see cref="LinkedInRecommendation"/> objects representing given recommendations
        /// </summary>
        public IEnumerable<LinkedInRecommendation> Recommendations
        {
            get { return _Recommendations.AsEnumerable(); }
        }


        internal void AddRecommendations(IEnumerable<LinkedInRecommendation> recommendations)
        {
                _Recommendations.AddRange(recommendations);
        }
    }
}
