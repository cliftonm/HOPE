
using System.Xml.Linq;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn update object for CMPY updates (job)
    /// </summary>
    public class LinkedInCompanyJobUpdate : LinkedInCompanyBaseUpdate
    {
        internal LinkedInCompanyJobUpdate()
        {
            CompanyUpdateType = LinkedInCompanyUpdateType.JobUpdate;
        }

        /// <summary>
        /// Gets <see cref="LinkedInJob"/> object representing update's job
        /// </summary>
        public LinkedInJob Job { get; internal set; }

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
            //company-job-update
            xc = xc.Element("company-job-update");
            if (xc == null) return;
            //job
            xe = xc.Element("job");
            if (xe != null)
                Job = Utils.BuildJob(xe);
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
