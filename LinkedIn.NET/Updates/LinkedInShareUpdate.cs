using System.Xml.Linq;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn update object for SHAR updates
    /// </summary>
    public class LinkedInShareUpdate : LinkedInUpdate
    {
        internal LinkedInShareUpdate()
        {
        }

        /// <summary>
        /// Gets <see cref="LinkedInPersonShare"/> object representing update's person
        /// </summary>
        public LinkedInPersonShare Person { get; internal set; }

        internal override void BuildUpdate(XElement xp)
        {
            SetBaseValues(xp);

            //update-content
            var xcontent = xp.Element("update-content");
            if (xcontent == null) return;
            //person
            var xe = xcontent.Element("person");
            if (xe == null) return;
            Person = new LinkedInPersonShare();
            Utils.BuildPerson(Person, xe);
            //current-share
            xe = xe.Element("current-share");
            if (xe != null)
                Person.CurrentShare = Utils.BuildShare(xe);
        }
    }
}
