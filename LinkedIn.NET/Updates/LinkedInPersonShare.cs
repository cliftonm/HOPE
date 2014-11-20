using LinkedIn.NET.Members;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn person object used in SHAR updates
    /// </summary>
    public class LinkedInPersonShare : LinkedInPerson
    {
        internal LinkedInPersonShare()
        {
        }

        /// <summary>
        /// Gets <see cref="LinkedInShare"/> object representing the member's current share, if set
        /// </summary>
        public LinkedInShare CurrentShare { get; internal set; }
    }
}
