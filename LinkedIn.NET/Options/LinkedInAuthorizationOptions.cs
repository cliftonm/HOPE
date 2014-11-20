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

namespace LinkedIn.NET.Options
{
    /// <summary>
    /// Represents LinkedIn authorization options
    /// </summary>
    public class LinkedInAuthorizationOptions
    {
        private LinkedInPermissions _Permissions = LinkedInPermissions.BasicProfile;

        /// <summary>
        /// Gets or sets permissions set. Can be a combination of <see cref="LinkedInPermissions"/> enumeration values. If not supplied, defaults to <see cref="LinkedInPermissions.BasicProfile"/>
        /// </summary>
        /// <example>
        /// See <see cref="LinkedInClient.GetAccessToken"/> for example
        /// </example>
        public LinkedInPermissions Permissions
        {
            get { return _Permissions; }
            set { _Permissions = value; }
        }

        /// <summary>
        /// Gets or sets the url where users will be sent after authorization
        /// </summary>
        /// <example>
        /// See <see cref="LinkedInClient.GetAccessToken"/> for example
        /// </example>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// A long unique string value of your choice that is hard to guess. Used to prevent CSRF (cross-site request forgery)
        /// </summary>
        /// <example>
        /// See <see cref="LinkedInClient.GetAccessToken"/> for example
        /// </example>
        public string State { get; set; }
    }
}
