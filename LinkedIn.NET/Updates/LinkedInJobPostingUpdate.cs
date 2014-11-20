using System.Xml.Linq;
using LinkedIn.NET.Members;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn update object for JOBP updates
    /// </summary>
    public class LinkedInJobPostingUpdate : LinkedInUpdate
    {
        internal LinkedInJobPostingUpdate()
        {
        }

        /// <summary>
        /// Gets <see cref="LinkedInPerson"/> object representing update's job poster
        /// </summary>
        public LinkedInPerson JobPoster { get; internal set; }

        /// <summary>
        /// Gets job's id
        /// </summary>
        public string JobId { get; internal set; }

        /// <summary>
        /// Gets job's position title
        /// </summary>
        public string PositionTitle { get; internal set; }

        /// <summary>
        /// Gets job's company name
        /// </summary>
        public string CompanyName { get; internal set; }

        /// <summary>
        /// Gets job's request URL
        /// </summary>
        public string SiteJobRequestUrl { get; internal set; }

        internal override void BuildUpdate(XElement xp)
        {
            SetBaseValues(xp);

            //update-content
            var xc = xp.Element("update-content");
            if (xc == null) return;
            xc = xc.Element("job");
            if (xc == null) return;
            //job-poster
            var xe = xc.Element("job-poster");
            if (xe != null)
            {
                JobPoster = new LinkedInPerson();
                Utils.BuildPerson(JobPoster, xe);
            }
            //id
            xe = xc.Element("id");
            if (xe != null)
                JobId = xe.Value.Trim();
            //position
            xe = xc.Element("position");
            if (xe != null)
            {
                var xn = xe.Element("title");
                if (xn != null)
                    PositionTitle = xn.Value.Trim();
            }
            //company
            xe = xc.Element("company");
            if (xe != null)
            {
                var xn = xe.Element("name");
                if (xn != null)
                    CompanyName = xn.Value.Trim();
            }
            //site-job-request
            xe = xc.Element("site-job-request");
            if (xe != null)
            {
                var xn = xe.Element("url");
                if (xn != null)
                    SiteJobRequestUrl = xn.Value.Trim();
            }
        }
    }
}
