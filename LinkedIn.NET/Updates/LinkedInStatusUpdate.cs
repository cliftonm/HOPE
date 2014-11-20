using System.Xml.Linq;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn update object for STAT updates
    /// </summary>
    public class LinkedInStatusUpdate : LinkedInUpdate
    {
        internal LinkedInStatusUpdate()
        {
        }

        /// <summary>
        /// Gets <see cref="LinkedInPersonStatus"/> object representing update's person
        /// </summary>
        public LinkedInPersonStatus Person { get; internal set; }

        internal override void BuildUpdate(XElement xp)
        {
            SetBaseValues(xp);

            //update-content
            var xcontent = xp.Element("update-content");
            if (xcontent == null) return;
            //person
            var xe = xcontent.Element("person");
            if (xe == null) return;
            Person = new LinkedInPersonStatus();
            Utils.BuildPerson(Person, xe);
            //current-status
            xe = xe.Element("current-status");
            if (xe != null)
                Person.CurrentStatus = xe.Value.Trim();
        }
    }
}
