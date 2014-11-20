
using System.Xml.Linq;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn update object for CMPY updates (status)
    /// </summary>
    public class LinkedInCompanyStatusUpdate : LinkedInCompanyBaseUpdate
    {
        internal LinkedInCompanyStatusUpdate()
        {
            CompanyUpdateType = LinkedInCompanyUpdateType.StatusUpdate;
        }

        /// <summary>
        /// Gets <see cref="LinkedInShare"/> object representing update's share
        /// </summary>
        public LinkedInShare Share { get; internal set; }

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
            //company-status-update
            xc = xc.Element("company-status-update");
            if (xc == null) return;
            //share
            xe = xc.Element("share");
            if (xe != null)
                Share = Utils.BuildShare(xe);
        }
    }
}
