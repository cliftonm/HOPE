using System.Linq;
using System.Xml.Linq;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn update object for PRFX updates
    /// </summary>
    public class LinkedInExtendedProfileUpdate : LinkedInUpdate
    {
        internal LinkedInExtendedProfileUpdate()
        {
        }

        /// <summary>
        /// Gets <see cref="LinkedInPersonExtendedProfile"/> object representing update's person
        /// </summary>
        public LinkedInPersonExtendedProfile Person { get; internal set; }

        internal override void BuildUpdate(XElement xp)
        {
            SetBaseValues(xp);

            //update-content
            var xcontent = xp.Element("update-content");
            if (xcontent == null) return;
            //person
            var xe = xcontent.Element("person");
            if (xe == null) return;
            Person = new LinkedInPersonExtendedProfile();
            Utils.BuildPerson(Person, xe);
            //main-address
            var xn = xe.Element("main-address");
            if (xn != null)
                Person.MainAddress = xn.Value.Trim();
            //date-of-birth
            xn = xe.Element("date-of-birth");
            if (xn != null)
                Person.DateOfBirth = Utils.BuildDate(xn);
            //phone-numbers
            xn = xe.Element("phone-numbers");
            if (xn != null)
                Person.AddPhoneNumbers(xn.Elements("phone-number").Select(xf => xf.Value.Trim()));
            //im-accounts
            xn = xe.Element("im-accounts");
            if (xn != null)
                Person.AddImAccounts(xn.Elements("im-account").Select(xf => xf.Value.Trim()));
            //im-accounts
            xn = xe.Element("twitter-accounts");
            if (xn != null)
                Person.AddTwitterAccounts(xn.Elements("twitter-account").Select(xf => xf.Value.Trim()));
        }
    }
}
