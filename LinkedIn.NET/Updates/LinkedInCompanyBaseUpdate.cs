
namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn base update object for CMPY updates
    /// </summary>
    public abstract class LinkedInCompanyBaseUpdate : LinkedInUpdate
    {
        internal LinkedInCompanyBaseUpdate()
        {
        }

        /// <summary>
        /// Gets update's company
        /// </summary>
        public LinkedInCompanyBase Company { get; internal set; }

        /// <summary>
        /// Gets update's type
        /// </summary>
        public LinkedInCompanyUpdateType CompanyUpdateType { get; internal set; }
    }
}
