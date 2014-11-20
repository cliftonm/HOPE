using System.Xml.Linq;
using LinkedIn.NET.Members;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn update object for VIRL updates
    /// </summary>
    public class LinkedInViralUpdate : LinkedInUpdate
    {
        internal LinkedInViralUpdate()
        {
        }

        /// <summary>
        /// Gets <see cref="LinkedInPerson"/> object representing update's person
        /// </summary>
        public LinkedInPerson Person { get; internal set; }

        /// <summary>
        /// Gets <see cref="LinkedInActionUpdate"/> object representing update's action
        /// </summary>
        public LinkedInActionUpdate UpdateAction { get; internal set; }

        internal override void BuildUpdate(XElement xp)
        {
            SetBaseValues(xp);

            //update-content
            var xcontent = xp.Element("update-content");
            if (xcontent == null) return;
            //person
            var xe = xcontent.Element("person");
            if (xe != null)
            {
                Person = new LinkedInPerson();
                Utils.BuildPerson(Person, xe);
            }
            //update-action
            xe = xcontent.Element("update-action");
            if (xe == null) return;
            UpdateAction = Utils.BuildUpdateAction(xe);
            //original update
            var xa = xe.Element("original-update");
            if (xa == null) return;
            var update = new LinkedInShareUpdate();
            update.BuildUpdate(xa);
            UpdateAction.OriginalUpdate = update;
        }
    }
}
