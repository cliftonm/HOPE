using System.Linq;
using System.Xml.Linq;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn update object for PREC/SVPR updates
    /// </summary>
    public class LinkedInRecommendationUpdate : LinkedInUpdate
    {
        internal LinkedInRecommendationUpdate()
        {
        }

        /// <summary>
        /// Gets <see cref="LinkedInPersonRecommendation"/> object representing update's person
        /// </summary>
        public LinkedInPersonRecommendation Person { get; internal set; }

        internal override void BuildUpdate(XElement xp)
        {
            SetBaseValues(xp);

            //update-content
            var xcontent = xp.Element("update-content");
            if (xcontent == null) return;
            //person
            var xe = xcontent.Element("person");
            if (xe == null) return;
            Person = new LinkedInPersonRecommendation();
            Utils.BuildPerson(Person, xe);
            //recommendations-given
            var xn = xe.Element("recommendations-given");
            if (xn != null)
                Person.AddRecommendations(xn.Elements("recommendation").Select(Utils.BuildRecommendationGiven));
            //recommendations-received
            xn = xe.Element("recommendations-received");
            if (xn != null)
                Person.AddRecommendations(xn.Elements("recommendation").Select(Utils.BuildRecommendationReceived));
        }
    }
}
