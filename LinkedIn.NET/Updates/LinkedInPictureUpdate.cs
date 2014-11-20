using System.Xml.Linq;
using LinkedIn.NET.Members;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn update object for PICU updates
    /// </summary>
    public class LinkedInPictureUpdate : LinkedInUpdate
    {
        internal LinkedInPictureUpdate()
        {
        }

        /// <summary>
        /// Gets <see cref="LinkedInPerson"/> object representing update's person
        /// </summary>
        public LinkedInPerson Person { get; internal set; }

        internal override void BuildUpdate(XElement xp)
        {
            SetBaseValues(xp);

            //update-content
            var xcontent = xp.Element("update-content");
            if (xcontent == null) return;
            //person
            var xe = xcontent.Element("person");
            if (xe == null) return;
            Person = new LinkedInPerson();
            Utils.BuildPerson(Person, xe);
        }
    }
}
