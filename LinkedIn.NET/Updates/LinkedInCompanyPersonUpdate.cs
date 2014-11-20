
using System.Xml.Linq;
using LinkedIn.NET.Members;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn update object for CMPY updates (person)
    /// </summary>
    public class LinkedInCompanyPersonUpdate : LinkedInCompanyBaseUpdate
    {
        internal LinkedInCompanyPersonUpdate()
        {
            CompanyUpdateType = LinkedInCompanyUpdateType.PersonUpdate;
        }

        /// <summary>
        /// Gets <see cref="LinkedInPerson"/> object representing the person who changed their relationship
        /// </summary>
        public LinkedInPerson Person { get; internal set; }

        /// <summary>
        /// Gets update's action code
        /// </summary>
        public string ActionCode { get; internal set; }

        /// <summary>
        /// Gets <see cref="LinkedInPositionBase"/> object representing person's old position
        /// </summary>
        public LinkedInPositionBase OldPosition { get; internal set; }

        /// <summary>
        /// Gets <see cref="LinkedInPositionBase"/> object representing person's new position
        /// </summary>
        public LinkedInPositionBase NewPosition { get; internal set; }

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
            //old-position
            xe = xc.Element("old-position");
            if (xe != null)
                OldPosition = Utils.BuildPositionBase(xe);
            //new-position
            xe = xc.Element("new-position");
            if (xe != null)
                NewPosition = Utils.BuildPositionBase(xe);
        }
    }
}
