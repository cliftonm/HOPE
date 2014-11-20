using System.Linq;
using System.Xml.Linq;
using LinkedIn.NET.Members;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn update object for CONN updates
    /// </summary>
    public class LinkedInConnectionMemberUpdate : LinkedInUpdate
    {
        internal LinkedInConnectionMemberUpdate()
        {
        }

        /// <summary>
        /// Gets <see cref="LinkedInPersonConnection"/> object representing update's person
        /// </summary>
        public LinkedInPersonConnection Person { get; internal set; }

        internal override void BuildUpdate(XElement xp)
        {
            SetBaseValues(xp);

            //update-content
            var xcontent = xp.Element("update-content");
            if (xcontent == null) return;
            //person
            var xe = xcontent.Element("person");
            if (xe == null) return;
            Person = new LinkedInPersonConnection();
            Utils.BuildPerson(Person, xe);
            //connections
            xe = xe.Element("connections");
            if (xe != null)
            {
                Person.AddConnections(
                    xe.Elements("person").Select(xc => Utils.BuildPerson(new LinkedInPerson(), xc)));
            }
        }
    }
}
