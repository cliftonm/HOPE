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

using System;
using System.Net;
using LinkedIn.NET.Members;

namespace LinkedIn.NET
{
    internal sealed class Singleton
    {
        private static readonly Lazy<Singleton> _Lazy =
            new Lazy<Singleton>(() => new Singleton());

        internal static Singleton Instance { get { return _Lazy.Value; } }

        private Singleton()
        {
        }

        private LinkedInPerson _CurrentUser;
        private string _AccessToken;
        private string _LastRequest;
        private SecurityProtocolType _SecurityProtocol = SecurityProtocolType.Tls;

        internal string LastRequest
        {
            get { return Instance._LastRequest; }
            set
            {
                const string token = "oauth2_access_token=";
                var pos = value.IndexOf(token, StringComparison.Ordinal);
                Instance._LastRequest = pos < 0 ? value : value.Substring(0, pos + token.Length);
            }
        }
        internal string AccessToken
        {
            get { return Instance._AccessToken; }
            set { Instance._AccessToken = value; }
        }
        internal LinkedInPerson CurrentUser
        {
            get { return Instance._CurrentUser ?? (Instance._CurrentUser = Utils.GetCurrentUser()); }
            set { Instance._CurrentUser = value; }
        }

        internal SecurityProtocolType SecurityProtocol
        {
            get { return Instance._SecurityProtocol; }
            set { Instance._SecurityProtocol = value; }
        }
    }
}
