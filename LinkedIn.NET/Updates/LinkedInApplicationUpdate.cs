using System.Linq;
using System.Xml.Linq;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn update object for APPS/APPM updates
    /// </summary>
    public class LinkedInApplicationUpdate : LinkedInUpdate
    {
        internal LinkedInApplicationUpdate()
        {
        }

        /// <summary>
        /// Gets <see cref="LinkedInPersonApplication"/> object representing update's person
        /// </summary>
        public LinkedInPersonApplication Person { get; internal set; }

        internal override void BuildUpdate(XElement xp)
        {
            SetBaseValues(xp);

            //update-content
            var xcontent = xp.Element("update-content");
            if (xcontent == null) return;
            //person
            var xe = xcontent.Element("person");
            if (xe == null) return;
            Person = new LinkedInPersonApplication();
            Utils.BuildPerson(Person, xe);
            //person-activities
            xe = xe.Element("person-activities");
            if (xe != null)
            {
                Person.AddActivities(xe.Elements("activity").Select(Utils.BuildActivity));
            }
        }
    }
}
