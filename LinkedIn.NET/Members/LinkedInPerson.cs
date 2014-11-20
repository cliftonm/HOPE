// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

using LinkedIn.NET.Options;
using System.Linq;

namespace LinkedIn.NET.Members
{
    /// <summary>
    /// Represents LinkedIn person object 
    /// </summary>
    public class LinkedInPerson
    {
        internal LinkedInPerson()
        {
        }

        /// <summary>
        /// Gets unique identifier for member
        /// </summary>
        public string Id { get; internal set; }
        /// <summary>
        /// Gets member's first name
        /// </summary>
        public string FirstName { get; internal set; }
        /// <summary>
        /// Gets member's last name
        /// </summary>
        public string LastName { get; internal set; }
        /// <summary>
        /// Gets member's headline
        /// </summary>
        public string Headline { get; internal set; }
        /// <summary>
        /// Gets a URL to the profile picture, if the member has associated one with their profile and it is visible to the requestor
        /// </summary>
        public string PictureUrl { get; internal set; }
        /// <summary>
        /// Gets a URL to the member's authenticated profile on LinkedIn
        /// </summary>
        public string SiteStandardProfileRequest { get; internal set; }
        /// <summary>
        /// Gets <see cref="LinkedInApiStandardProfileRequest"/> object representing API standard profile request object
        /// </summary>
        public LinkedInApiStandardProfileRequest ApiStandardProfileRquest { get; internal set; }
        /// <summary>
        /// Sends message to current member
        /// </summary>
        /// <param name="subject">Message subject</param>
        /// <param name="body">Message body</param>
        /// <param name="includeSenderInRecipients">Indicates whether sender of message should be included in recipients</param>
        /// <returns>Value containing true or false, depending on operation success, and response status</returns>
        /// <exception cref="LinkedInMissingParameterException">Thrown when message's subject or body are null or empty strings</exception>
        public LinkedInResponse<bool> SendMessage(string subject, string body, bool includeSenderInRecipients)
        {
            if (string.IsNullOrEmpty(subject))
                throw new LinkedInMissingParameterException("Message subject cannot be null or empty", "Subject");
            if (string.IsNullOrEmpty(body))
                throw new LinkedInMissingParameterException("Message body cannot be null or empty", "Body");

            var options = new LinkedInMessageOptions
            {
                Body = body,
                Subject = subject,
                IncludeSenderInRecipients = includeSenderInRecipients
            };
            options.Recipients.Add(Id);
            return RequestRunner.SendMessage(options);
        }

        /// <summary>
        /// Sends invitation to current member
        /// </summary>
        /// <param name="subject">Invitation subject</param>
        /// <param name="body">Invitation body</param>
        /// <returns>Value containing true or false, depending on operation success, and response status</returns>
        /// <exception cref="LinkedInMissingParameterException">Thrown when some of the following is missing: subject, body, <see cref="ApiStandardProfileRquest"/> property or ApiStandardProfileRquest headers</exception>
        public LinkedInResponse<bool> SendInvitation(string subject, string body)
        {
            if (string.IsNullOrEmpty(subject))
                throw new LinkedInMissingParameterException("Invitation subject cannot be null or empty", "Subject");
            if (string.IsNullOrEmpty(body))
                throw new LinkedInMissingParameterException("Invitation body cannot be null or empty", "Body");
            if (ApiStandardProfileRquest == null)
                throw new LinkedInMissingParameterException("ApiStandardProfileRquest field is not set", "ApiStandardProfileRquest");
            if (!ApiStandardProfileRquest.Headers.Any())
                throw new LinkedInMissingParameterException("There are no headers in ApiStandardProfileRquest field", "Headers");
            var header = ApiStandardProfileRquest.Headers.First();
            var arr = header.Value.Split(':');

            var options = new LinkedInInvitationOptions
            {
                Body = body,
                Subject = subject,
                RecipientId = Id,
                AuthorizationName = arr[0],
                AuthorizationValue = arr[1]
            };
            return RequestRunner.SendInvitation(options);
        }
    }
}
