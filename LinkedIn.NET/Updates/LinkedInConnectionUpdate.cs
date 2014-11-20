using System.Xml.Linq;
using LinkedIn.NET.Members;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn update object for NCON/CCEM updates
    /// </summary>
    public class LinkedInConnectionUpdate : LinkedInUpdate
    {
        internal LinkedInConnectionUpdate()
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
            if (xcontent != null)
            {
                //person
                var xe = xcontent.Element("person");
                if (xe != null)
                {
                    Person = Utils.BuildPerson(new LinkedInPerson(), xe);
                }
            }
        }
    }
}
