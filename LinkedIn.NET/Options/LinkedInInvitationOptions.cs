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

using LinkedIn.NET.Members;

namespace LinkedIn.NET.Options
{
    /// <summary>
    /// Represents object which stores necessary settings for sending invitation
    /// </summary>
    public class LinkedInInvitationOptions
    {
        private string _Subject;

        /// <summary>
        /// Gets or sets invitation's subject
        /// </summary>
        public string Subject
        {
            get { return _Subject; }
            set
            {
                _Subject = value;
                if (_Subject.Length > 200) _Subject = _Subject.Substring(0, 200);
            }
        }

        /// <summary>
        /// Gets or sets invitation's body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets invitation's type. Can be one of <see cref="LinkedInInvitationType"/> enumeration values
        /// </summary>
        public LinkedInInvitationType InvitationType { get; set; }

        /// <summary>
        /// Gets or sets recipient's ID
        /// </summary>
        /// <remarks>Required if <see cref="InvitationType"/> property is set to InviteById, otherwise can be omitted</remarks>
        public string RecipientId { get; set; }

        /// <summary>
        /// Gets or sets invitation's authorization name
        /// </summary>
        /// <remarks>Required if <see cref="InvitationType"/> property is set to InviteById, otherwise can be omitted. 
        /// To obtain this value you should take the Value property of Header property of ApiStandardProfileRquest of <see cref="LinkedInPerson"/> object, split it on the colon ':' and take the value to the left of the colon
        /// </remarks>
        public string AuthorizationName { get; set; }

        /// <summary>
        /// Gets or sets invitation's authorization value
        /// </summary>
        /// <remarks>Required if <see cref="InvitationType"/> property is set to InviteById, otherwise can be omitted. 
        /// To obtain this value you should take the Value property of Header property of ApiStandardProfileRquest of <see cref="LinkedInPerson"/> object, split it on the colon ':' and take the value to the right of the colon
        /// </remarks>
        public string AuthorizationValue { get; set; }

        /// <summary>
        /// Gets or sets recipient's e-mail
        /// </summary>
        /// <remarks>Required if <see cref="InvitationType"/> property is set to InviteByEmail, otherwise can be omitted</remarks>
        public string RecipientEmail { get; set; }

        /// <summary>
        /// Gets or sets recipient's first name
        /// </summary>
        /// <remarks>Required if <see cref="InvitationType"/> property is set to InviteByEmail, otherwise can be omitted</remarks>
        public string RecipientFirstName { get; set; }

        /// <summary>
        /// Gets or sets recipient's last name
        /// </summary>
        /// <remarks>Required if <see cref="InvitationType"/> property is set to InviteByEmail, otherwise can be omitted</remarks>
        public string RecipientLastName { get; set; }
    }
}
