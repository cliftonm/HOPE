using System.Collections.Generic;
using System.Linq;
using LinkedIn.NET.Members;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn person object used in PRFX updates
    /// </summary>
    public class LinkedInPersonExtendedProfile : LinkedInPerson
    {
        internal LinkedInPersonExtendedProfile()
        {
        }

        private readonly List<string> _PhoneNumbers = new List<string>();
        private readonly List<string> _ImAccounts = new List<string>();
        private readonly List<string> _TwitterAccounts = new List<string>();

        /// <summary>
        /// Gets changed member's main address
        /// </summary>
        public string MainAddress { get; internal set; }

        /// <summary>
        /// Gets changed member's date of birth
        /// </summary>
        public LinkedInDate DateOfBirth { get; internal set; }

        /// <summary>
        /// Gets collectionof member's phone numbers
        /// </summary>
        public IEnumerable<string> PhoneNumbers
        {
            get { return _PhoneNumbers.AsEnumerable(); }
        }

        /// <summary>
        /// Gets collection of member's IM accounts
        /// </summary>
        public IEnumerable<string> ImAccounts
        {
            get { return _ImAccounts.AsEnumerable(); }
        }

        /// <summary>
        /// Gets collection of member's Twitter accounts
        /// </summary>
        public IEnumerable<string> TwitterAccounts
        {
            get { return _TwitterAccounts.AsEnumerable(); }
        }

        internal void AddPhoneNumbers(IEnumerable<string> numbers)
        {
            _PhoneNumbers.AddRange(numbers);
        }

        internal void AddImAccounts(IEnumerable<string> imaccounts)
        {
            _ImAccounts.AddRange(imaccounts);
        }

        internal void AddTwitterAccounts(IEnumerable<string> taccounts)
        {
            _TwitterAccounts.AddRange(taccounts);
        }
    }
}
