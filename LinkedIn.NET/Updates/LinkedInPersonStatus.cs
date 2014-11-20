using LinkedIn.NET.Members;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn person object used in STAT updates
    /// </summary>
    public class LinkedInPersonStatus : LinkedInPerson
    {
        internal LinkedInPersonStatus()
        {
        }

        /// <summary>
        /// Gets update's status text
        /// </summary>
        public string CurrentStatus { get; internal set; }
    }
}
