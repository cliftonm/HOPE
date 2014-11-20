using System.Xml.Linq;
using LinkedIn.NET.Members;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn update object for MSFC updates
    /// </summary>
    public class LinkedInStartFollowCompanyUpdate : LinkedInCompanyBaseUpdate
    {
        internal LinkedInStartFollowCompanyUpdate()
        {
        }

        /// <summary>
        /// Gets <see cref="LinkedInPerson"/> object representing the person who starts the following
        /// </summary>
        public LinkedInPerson Person { get; internal set; }

        /// <summary>
        /// Gets update's action code
        /// </summary>
        public string ActionCode { get; internal set; }

        internal override void BuildUpdate(XElement xp)
        {
            SetBaseValues(xp);
            //update-content
            var xc = xp.Element("update-content");
            if (xc == null) return;
            //company
            var xe = xc.Element("company");
            if (xe != null)
                Company = Utils.BuildCompanyBase(xe);
            //company-person-update
            xc = xc.Element("company-person-update");
            if (xc == null) return;
            //person
            xe = xc.Element("person");
            if (xe != null)
                Person = Utils.BuildPerson(new LinkedInPerson(), xe);
            //action
            xe = xc.Element("action");
            if (xe != null)
            {
                //code
                var xn = xe.Element("code");
                if (xn != null)
                    ActionCode = xn.Value.Trim();
            }
        }
    }
}
