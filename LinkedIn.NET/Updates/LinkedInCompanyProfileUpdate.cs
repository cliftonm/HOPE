
using System.Xml.Linq;
using LinkedIn.NET.Members;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn update object for CMPY updates (profile)
    /// </summary>
    public class LinkedInCompanyProfileUpdate : LinkedInCompanyBaseUpdate
    {
        internal LinkedInCompanyProfileUpdate()
        {
            CompanyUpdateType = LinkedInCompanyUpdateType.ProfileUpdate;
        }

        /// <summary>
        /// Gets update's action code
        /// </summary>
        public string ActionCode { get; internal set; }

        /// <summary>
        /// Gets value indicating what's changed
        /// </summary>
        public string ProfileFieldCode { get; internal set; }

        /// <summary>
        /// Gets <see cref="LinkedInPerson"/> object representing the person editing the profile
        /// </summary>
        public LinkedInPerson Editor { get; internal set; }

        internal override void BuildUpdate(XElement xp)
        {
            SetBaseValues(xp);
            //update-content
            var xc = xp.Element("update-content");
            if (xc == null) return;
            ////company-update
            //xc = xc.Element("company-update");
            //if (xc == null) return;
            var xe = xc.Element("company");
            if (xe != null)
                Company = Utils.BuildCompanyBase(xe);
            //company-profile-update
            xc = xc.Element("company-profile-update");
            if (xc == null) return;
            //editor
            xe = xc.Element("editor");
            if (xe != null)
                Editor = Utils.BuildPerson(new LinkedInPerson(), xe);
            //action
            xe = xc.Element("action");
            if (xe != null)
            {
                //code
                var xn = xe.Element("code");
                if (xn != null)
                    ActionCode = xn.Value.Trim();
            }
            //profile-field
            xe = xc.Element("profile-field");
            if (xe != null)
            {
                //code
                var xn = xe.Element("code");
                if (xn != null)
                    ProfileFieldCode = xn.Value.Trim();
            }
        }
    }
}
