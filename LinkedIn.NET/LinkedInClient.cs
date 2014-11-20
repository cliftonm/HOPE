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
using System.Collections.Generic;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using LinkedIn.NET.Groups;
using LinkedIn.NET.Members;
using LinkedIn.NET.Options;
using LinkedIn.NET.Search;
using LinkedIn.NET.Updates;

namespace LinkedIn.NET
{
    /// <summary>
    /// Represents object for authentication, posting and getting updates, posting and getting comments, working with groups, sending messages and invitations and executing queries via LinkedIn API
    /// </summary>
    public class LinkedInClient
    {
        private const string SPACE = "%20";

        #region Public properties

        /// <summary>
        /// Gets or sets API key
        /// </summary>
        public string ApiKey { get; set; }
        /// <summary>
        /// Gets or sets secret key
        /// </summary>
        public string SecretKey { get; set; }
        /// <summary>
        /// Sets access token
        /// </summary>
        public string AccessToken
        {
            private get { return Singleton.Instance.AccessToken; }
            set { Singleton.Instance.AccessToken = value; }
        }
        /// <summary>
        /// Gets basic details of currently logged in user
        /// </summary>
        public LinkedInPerson CurrentUser
        {
            get { return Singleton.Instance.CurrentUser; }
        }
        /// <summary>
        /// Gets or sets security protocol used for web requests. Can be one of <see cref="System.Net.SecurityProtocolType"/> enumeration values. The default value is SecurityProtocolType.Tls.
        /// </summary>
        public SecurityProtocolType SecurityProtocol
        {
            get { return Singleton.Instance.SecurityProtocol; }
            set { Singleton.Instance.SecurityProtocol = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes new instance of LinkedInClient
        /// </summary>
        /// <param name="apiKey">API key of the application that uses LinkedInClient</param>
        /// <param name="secretKey">Secret key of the application that uses LinkedInClient</param>
        public LinkedInClient(string apiKey, string secretKey)
        {
            ApiKey = apiKey;
            SecretKey = secretKey;
        }

        #endregion

        #region Authentication region

        /// <summary>
        /// Gets the link to LinkedIn authorization page
        /// </summary>
        /// <param name="options">Value of <see cref="LinkedInAuthorizationOptions"/> representing authorization parameters</param>
        /// <returns>The link to LinkedIn authorization page</returns>
        /// <example>
        /// This sample shows how to call this method:
        /// <code>
        /// using LinkedIn.NET;
        /// using LinkedIn.NET.Groups;
        /// using LinkedIn.NET.Members;
        /// using LinkedIn.NET.Options;
        /// using LinkedIn.NET.Search;
        /// using LinkedIn.NET.Updates;
        /// ...
        /// const string REDIRECT_URL = "http://somesite.net/auth.htm";
        /// const string STATE = "12345qwertytrewq54321";
        /// // create new LinkedInClient object
        /// var _Client = new LinkedInClient("API_KEY", "SECRET_KEY");
        /// // Create new LinkedInAuthorizationOptions object with permissions for updates and messages
        /// var options = new LinkedInAuthorizationOptions
        ///         {
        ///             RedirectUrl = REDIRECT_URL,
        ///             Permissions = LinkedInPermissions.Messages | LinkedInPermissions.Updates,
        ///             State = STATE
        ///         };
        /// // Prepare authorization URL
        /// var authLink = _Client.GetAuthorizationUrl(options);
        /// </code>
        /// </example>
        public string GetAuthorizationUrl(LinkedInAuthorizationOptions options)
        {
            var scope = new StringBuilder();
            if ((options.Permissions & LinkedInPermissions.BasicProfile) == LinkedInPermissions.BasicProfile &&
                (options.Permissions & LinkedInPermissions.FullProfile) != LinkedInPermissions.FullProfile)
            {
                scope.Append("r_basicprofile");
                scope.Append(SPACE);
            }
            if ((options.Permissions & LinkedInPermissions.FullProfile) == LinkedInPermissions.FullProfile)
            {
                scope.Append("r_fullprofile");
                scope.Append(SPACE);
            }
            if ((options.Permissions & LinkedInPermissions.EmailAddress) == LinkedInPermissions.EmailAddress)
            {
                scope.Append("r_emailaddress");
                scope.Append(SPACE);
            }
            if ((options.Permissions & LinkedInPermissions.Connections) == LinkedInPermissions.Connections)
            {
                scope.Append("r_network");
                scope.Append(SPACE);
            }
            if ((options.Permissions & LinkedInPermissions.ContactsInfo) == LinkedInPermissions.ContactsInfo)
            {
                scope.Append("r_contactinfo");
                scope.Append(SPACE);
            }
            if ((options.Permissions & LinkedInPermissions.Updates) == LinkedInPermissions.Updates)
            {
                scope.Append("rw_nus");
                scope.Append(SPACE);
            }
            if ((options.Permissions & LinkedInPermissions.GroupDiscussions) == LinkedInPermissions.GroupDiscussions)
            {
                scope.Append("rw_groups");
                scope.Append(SPACE);
            }
            if ((options.Permissions & LinkedInPermissions.Messages) == LinkedInPermissions.Messages)
            {
                scope.Append("w_messages");
                scope.Append(SPACE);
            }
            if (scope.Length >= SPACE.Length) scope.Length -= SPACE.Length;
            var sb = new StringBuilder(Utils.AUTHORIZATION_URL);
            sb.Append(ApiKey);
            sb.Append("&scope=");
            sb.Append(scope);
            sb.Append("&state=");
            sb.Append(options.State);
            sb.Append("&redirect_uri=");
            sb.Append(options.RedirectUrl);
            return sb.ToString();
        }

        /// <summary>
        /// Retrieves LinkedIn access token using previously obtained authorization code
        /// </summary>
        /// <param name="authorizationCode">The authorization code obtained from LinkedIn</param>
        /// <param name="redirectUrl">Redirect URL, has to be the same URL as passed in options parameter to <see cref="GetAuthorizationUrl"/> method used in any other way to retrieve LinkedIn authorization code</param>
        /// <returns>True if valid access token retrieved</returns>
        /// <exception cref="LinkedInMissingCredentialException">Thrown when API key or secret key are not set</exception>
        /// <exception cref="LinkedInAuthenticationFailedException">Thrown when retrieving access token failed</exception>
        /// <example>
        /// The following example assumes you have a Windows Form named DlgAuthorization with WebBrowser control named wbAuth, having Dock property set to Top. 
        /// Set form's AutoScroll property to True.
        /// <code>
        /// using LinkedIn.NET;
        /// using LinkedIn.NET.Groups;
        /// using LinkedIn.NET.Members;
        /// using LinkedIn.NET.Options;
        /// using LinkedIn.NET.Search;
        /// using LinkedIn.NET.Updates;
        /// ... 
        /// // If you use WinForms this should be a "real" URL, because WinForms WebBrowser control does not support "fictive" URLs.
        /// // If you want to redirect to "fictive" URL, consider using WPF, because WPF WebBrowser control will redirect to such URL without problem.
        /// // In this case replace browser Navigated event with Navigating respectively.
        /// private const string REDIRECT_URL = "http://somesite.net/auth.htm";
        /// // Some unique long string
        /// private const string STATE = "12345qwertytrewq54321";
        /// // LinkedInClient object
        /// private LinkedInClient _Client;
        /// 
        /// ...
        /// 
        /// private void authorizeOnLinkedIn(string accessToken)
        /// {
        ///     // create new LinkedInClient object
        ///     _Client = new LinkedInClient("API_KEY", "SECRET_KEY");
        ///     if (string.IsNullOrEmpty(accessToken))
        ///     {
        ///         // Create new LinkedInAuthorizationOptions object with full set of permissions
        ///         var options = new LinkedInAuthorizationOptions
        ///         {
        ///             RedirectUrl = REDIRECT_URL,
        ///             Permissions = LinkedInPermissions.Connections | LinkedInPermissions.ContactsInfo |
        ///                           LinkedInPermissions.EmailAddress | LinkedInPermissions.FullProfile |
        ///                           LinkedInPermissions.GroupDiscussions | LinkedInPermissions.Messages |
        ///                           LinkedInPermissions.Updates,
        ///             State = STATE
        ///         };
        ///         // Prepare authorization URL
        ///         var authLink = _Client.GetAuthorizationUrl(options);
        ///         // Show authorization dialog
        ///         var dlgAuth = new DlgAuthorization(authLink);
        ///         if (dlgAuth.ShowDialog(this) == DialogResult.OK)
        ///         {
        ///             // Retrieve access token using authorization code we've got from dialog
        ///             var response = _Client.GetAccessToken(dlgAuth.AuthorizationCode, REDIRECT_URL);
        ///             if (response.Result != null &amp;&amp; response.Status == LinkedInResponseStatus.OK)
        ///             {
        ///                 // Because this is the only point you can obtain the access token, 
        ///                 // it is a good idea to store it here for further use.
        ///                 // Just do not forget to encrypt it before storage (assuming saveEncrypted procedure exists)
        ///                 saveEncrypted(response.Result.AccessToken);
        ///                 // Store expiration as well (assuming saveExpiration procedure exists)
        ///                 saveExpiration(response.Result.Expiration);
        ///             }
        ///         }
        ///         else
        ///         {
        ///             MessageBox.Show(dlgAuth.OauthErrorDescription, dlgAuth.OauthError);
        ///             return;
        ///         }
        ///     }
        ///     else
        ///     {
        ///         // Just set access token
        ///         _Client.AccessToken = accessToken;
        ///     }
        /// }
        /// 
        /// // -----------  DlgAuthentication   -----------
        /// using System;
        /// using System.Collections.Specialized;
        /// using System.Linq;
        /// using System.Windows.Forms;
        /// 
        /// namespace LNTest
        /// {
        ///     public partial class DlgAuthorization : Form
        ///     {
        ///         public DlgAuthorization(string authLink)
        ///         {
        ///             InitializeComponent();
        ///             // Parse authorization link
        ///             var qs = parseResponse(authLink);
        ///             // Store state for further use
        ///             if (qs["state"] != null)
        ///             {
        ///                 _State = qs["state"];
        ///             }
        ///             // Store redirect URL for further use
        ///             if (qs["redirect_uri"] != null)
        ///             {
        ///                 _RedirectUri = new Uri(qs["redirect_uri"]);
        ///             }
        ///             // Navigate to authorization link
        ///             wbAuth.Navigate(new Uri(authLink));
        ///         }
        /// 
        ///         private readonly string _State;
        ///         private readonly Uri _RedirectUri;
        /// 
        ///         // Property for handling authorization code
        ///         public string AuthorizationCode { get; private set; }
        ///         // Properties for handling possible errors
        ///         public string OauthError { get; private set; }
        ///         public string OauthErrorDescription { get; private set; }
        /// 
        ///         private void DlgAuthorization_KeyDown(object sender, KeyEventArgs e)
        ///         {
        ///             if (e.KeyCode == Keys.Escape) DialogResult = DialogResult.Cancel;
        ///         }
        /// 
        ///         private NameValueCollection parseResponse(string response)
        ///         {
        ///             var nvc = new NameValueCollection();
        ///             if (response.StartsWith("?")) response = response.Substring(1);
        ///             var arr1 = response.Split('&amp;');
        ///             foreach (var arr2 in arr1.Select(s => s.Split('=')).Where(arr2 => arr2.Length == 2))
        ///             {
        ///                 nvc.Add(arr2[0].Trim(), arr2[1].Trim());
        ///             }
        ///             return nvc;
        ///         }
        /// 
        ///         private void wbAuth_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        ///         {
        ///             // Resize broser control in order to to force the windows to show vertical scrollbar
        ///             // LinkedIn authorization always fill the whole browser, so this is the only way to show all fields and buttons in window of suitable size without stretching it to screen height
        ///             wbAuth.Height = ClientSize.Height * 2;
        ///             // Check whether we are in needed end point
        ///             if (e.Url.Scheme != _RedirectUri.Scheme || e.Url.Host != _RedirectUri.Host ||
        ///                 e.Url.AbsolutePath != _RedirectUri.AbsolutePath) return;
        ///             var queryParams = e.Url.Query;
        ///             if (queryParams.Length &lt;= 1) return;
        ///             // Parse query
        ///             var qs = parseResponse(queryParams);
        ///             // Check state parameter
        ///             if (qs["state"] == null) DialogResult = DialogResult.Cancel;
        ///             if (qs["state"] != _State) DialogResult = DialogResult.Cancel;
        ///             // Check code parameter
        ///             if (qs["code"] != null)
        ///             {
        ///                 // Store code parameter and close the window
        ///                 AuthorizationCode = qs["code"];
        ///                 DialogResult = DialogResult.OK;
        ///             }
        ///             // Check for possible errors
        ///             else if (qs["error"] != null)
        ///             {
        ///                 OauthError = qs["error"];
        ///                 if (qs["error_description"] != null)
        ///                 {
        ///                     OauthErrorDescription = qs["error_description"];
        ///                 }
        ///                 DialogResult = DialogResult.Cancel;
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public LinkedInResponse<LinkedInOauth> GetAccessToken(string authorizationCode, string redirectUrl)
        {
            if (string.IsNullOrEmpty(ApiKey))
                throw new LinkedInMissingCredentialException("The API key is not set");
            if (string.IsNullOrEmpty(SecretKey))
                throw new LinkedInMissingCredentialException("The secret key is not set");
            return getAccessToken(authorizationCode, redirectUrl);
        }

        private LinkedInResponse<LinkedInOauth> getAccessToken(string authorizationCode, string redirectUrl)
        {
            try
            {
                var sb = new StringBuilder(Utils.ACCESS_TOKEN_URL);
                sb.Append(authorizationCode);
                sb.Append("&redirect_uri=");
                sb.Append(redirectUrl);
                sb.Append("&client_id=");
                sb.Append(ApiKey);
                sb.Append("&client_secret=");
                sb.Append(SecretKey);
                var responseData = Utils.MakeRequest(sb.ToString(), "POST");
                var dict = Json.DecodeToDictionary(responseData);
                if (dict.ContainsKey("expires_in") && dict.ContainsKey("access_token"))
                {
                    var exp = DateTime.Now.AddSeconds(Convert.ToDouble(dict["expires_in"]));
                    AccessToken = dict["access_token"];
                    return
                        new LinkedInResponse<LinkedInOauth>(
                            new LinkedInOauth { AccessToken = AccessToken, Expiration = exp },
                            LinkedInResponseStatus.OK, null);
                }
                return new LinkedInResponse<LinkedInOauth>(null, LinkedInResponseStatus.OK, null);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse<LinkedInOauth>(null, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<LinkedInOauth>(null, LinkedInResponseStatus.OtherException, null, ex);
            }
        }
        #endregion

        #region Get member region

        private delegate LinkedInResponse<IEnumerable<LinkedInMember>> GetMembersDelegate(LinkedInGetMultipleMembersOptions options);
        private delegate LinkedInResponse<LinkedInMember> GetMemberDelegate(LinkedInGetMemberOptions options);

        private void getMembersCallback(IAsyncResult asyncResult)
        {
            var result = (AsyncResult)asyncResult;
            var _delegate = (GetMembersDelegate)result.AsyncDelegate;
            var members = _delegate.EndInvoke(asyncResult);
            var action = (Action<LinkedInResponse<IEnumerable<LinkedInMember>>>)asyncResult.AsyncState;
            action.Invoke(members);
        }

        private void getMemberCallback(IAsyncResult asyncResult)
        {
            var result = (AsyncResult)asyncResult;
            var _delegate = (GetMemberDelegate)result.AsyncDelegate;
            var member = _delegate.EndInvoke(asyncResult);
            var action = (Action<LinkedInResponse<LinkedInMember>>)asyncResult.AsyncState;
            action.Invoke(member);
        }

        /// <summary>
        /// Gets multiple LinkedIn member details asynchronously
        /// </summary>
        /// <param name="options">The object of type <see cref="LinkedInGetMultipleMembersOptions"/> representing retrieval options</param>
        /// <param name="action">Action to be invoked when retrieval process ends</param>
        /// <returns>Status of asynchronus operation</returns>
        /// <example>
        /// This sample shows how to call this method:
        /// <code>
        /// using LinkedIn.NET;
        /// using LinkedIn.NET.Groups;
        /// using LinkedIn.NET.Members;
        /// using LinkedIn.NET.Options;
        /// using LinkedIn.NET.Search;
        /// using LinkedIn.NET.Updates;
        /// ... 
        /// // define collection for holding members
        /// private readonly List (LinkedInMember) _Members = new List&lt;LinkedInMember&gt;();
        /// 
        /// ...
        /// 
        /// // define retrieval options
        /// var options = new LinkedInGetMultipleMembersOptions();
        /// options.BasicProfileOptions.SelectAll();
        /// options.EmailProfileOptions.SelectAll();
        /// options.FullProfileOptions.SelectAll();
        /// // add retrieval parameters
        /// var parameters = new List&lt;LinkedInGetMemberParameters&gt;()
        /// // add currently logged in user
        /// parameters.Add(new LinkedInGetMemberParameters
        /// {
        ///     GetBy = LinkedInGetMemberBy.Self
        /// });
        /// // add additional user id
        /// parameters.Add(new LinkedInGetMemberParameters
        /// {
        ///     GetBy = LinkedInGetMemberBy.Id,
        ///     RequestBy = "12345"
        /// });
        /// // assign parameters and call the method where processMultipleProfiles is application defined callback function
        /// options.Params = parameters;
        /// _client.GetMembers(options, processMultipleProfiles);
        /// 
        /// ...
        /// 
        /// // application defined function
        /// private void processMultipleProfiles(LinkedInResponse&lt;IEnumerable&lt;LinkedInMember&gt;&gt; response)
        /// {
        ///     // always check response.Result and response.Status before processing
        ///     if (response.Result != null &amp;&amp; response.Status == LinkedInResponseStatus.OK)
        ///     {
        ///         _Members.Clear();
        ///         _Members.AddRange(response.Result);
        ///     }
        ///     else
        ///     {
        ///         // show possible error message LinkedIn response
        ///         MessageBox.Show(response.Message);
        ///     }
        /// }
        /// </code>
        /// </example>
        public IAsyncResult GetMembers(LinkedInGetMultipleMembersOptions options,
                                       Action<LinkedInResponse<IEnumerable<LinkedInMember>>> action)
        {
            GetMembersDelegate _delegate = GetMembers;
            return _delegate.BeginInvoke(options, getMembersCallback, action);
        }

        /// <summary>
        /// Gets multiple LinkedIn members details
        /// </summary>
        /// <param name="options">The object of type <see cref="LinkedInGetMultipleMembersOptions"/> representing retrieval options</param>
        /// <returns>Value containing collection of <see cref="LinkedInMember"/> objects and response status</returns>
        /// <example>
        /// This sample shows how to call this method:
        /// <code>
        /// using LinkedIn.NET;
        /// using LinkedIn.NET.Groups;
        /// using LinkedIn.NET.Members;
        /// using LinkedIn.NET.Options;
        /// using LinkedIn.NET.Search;
        /// using LinkedIn.NET.Updates;
        /// ...
        /// // define collection for holding members
        /// private readonly List (LinkedInMember) _Members = new List&lt;LinkedInMember&gt;();
        /// 
        /// ...
        /// 
        /// // define retrieval options
        /// var options = new LinkedInGetMultipleMembersOptions();
        /// options.BasicProfileOptions.SelectAll();
        /// options.EmailProfileOptions.SelectAll();
        /// options.FullProfileOptions.SelectAll();
        /// // add retrieval parameters
        /// var parameters = new List&lt;LinkedInGetMemberParameters&gt;()
        /// // add currently logged in user
        /// parameters.Add(new LinkedInGetMemberParameters
        /// {
        ///     GetBy = LinkedInGetMemberBy.Self
        /// });
        /// // add additional user id
        /// parameters.Add(new LinkedInGetMemberParameters
        /// {
        ///     GetBy = LinkedInGetMemberBy.Id,
        ///     RequestBy = "12345"
        /// });
        /// // assign parameters and call the method
        /// options.Params = parameters;
        /// var response = _client.GetMembers(options);
        /// // always check response.Result and response.Status before processing
        /// if (response.Result != null &amp;&amp; response.Status == LinkedInResponseStatus.OK)
        /// {
        ///     _Members.Clear();
        ///     _Members.AddRange(response.Result);
        /// }
        /// else
        /// {
        ///     // show possible error message LinkedIn response
        ///     MessageBox.Show(response.Message);
        /// }
        /// </code>
        /// </example>
        public LinkedInResponse<IEnumerable<LinkedInMember>> GetMembers(LinkedInGetMultipleMembersOptions options)
        {
            return RequestRunner.GetMembers(options);
        }

        /// <summary>
        /// Gets LinkedIn member details asynchronously
        /// </summary>
        /// <param name="options">The object of type <see cref="LinkedInGetMemberOptions"/> representing retrieval options</param>
        /// <param name="action">Action to be invoked when retrieval process ends</param>
        /// <returns>Status of asynchronus operation</returns>
        /// <exception cref="LinkedInMissingMemberIdException">Thrown when member is requested by id, but no member Id is set</exception>
        /// <exception cref="LinkedInMissingProfileUrlException">Thrown when member is requested by public URL, but no URL is set</exception>
        /// <exception cref="LinkedInInvalidProfileUrlException">Thrown when member is requested by public URL, but URL is not well formed</exception>
        /// <example>
        /// This sample shows how to call this method:
        /// <code>
        /// using LinkedIn.NET;
        /// using LinkedIn.NET.Groups;
        /// using LinkedIn.NET.Members;
        /// using LinkedIn.NET.Options;
        /// using LinkedIn.NET.Search;
        /// using LinkedIn.NET.Updates;
        /// ... 
        /// // define retrieval options
        /// var options = new LinkedInGetMemberOptions();
        /// options.BasicProfileOptions.SelectAll();
        /// options.EmailProfileOptions.SelectAll();
        /// options.FullProfileOptions.SelectAll();
        /// // get currently logged in user
        /// options.Parameters.GetBy = LinkedInGetMemberBy.Self;
        /// // call the method where processProfile is application defined callback function
        /// _client.GetMember(options, processProfile);
        /// 
        /// ...
        /// 
        /// // application defined function
        /// private void processProfile(LinkedInResponse&lt;LinkedInMember&gt; response)
        /// {
        ///     // always check response.Result and response.Status before processing
        ///     if (response.Result != null &amp;&amp; response.Status == LinkedInResponseStatus.OK)
        ///     {
        ///         Console.WriteLine(@"Member name: {0} {1}", response.Result.BasicProfile.FirstName,
        ///            response.Result.BasicProfile.LastName);
        ///     }
        ///     else
        ///     {
        ///         // show possible error message LinkedIn response
        ///         MessageBox.Show(response.Message);
        ///     }
        /// }
        /// </code>
        /// </example>
        public IAsyncResult GetMember(LinkedInGetMemberOptions options, Action<LinkedInResponse<LinkedInMember>> action)
        {
            switch (options.Parameters.GetBy)
            {
                case LinkedInGetMemberBy.Id:
                    if (string.IsNullOrEmpty(options.Parameters.RequestBy))
                        throw new LinkedInMissingMemberIdException("Member Id is not set", "RequestBy");
                    break;
                case LinkedInGetMemberBy.Url:
                    if (string.IsNullOrEmpty(options.Parameters.RequestBy))
                        throw new LinkedInMissingProfileUrlException("User profile url is not set", "RequestBy");
                    if (
                        !Uri.IsWellFormedUriString(options.Parameters.RequestBy,
                            UriKind.RelativeOrAbsolute))
                        throw new LinkedInInvalidProfileUrlException("Invalid user profile url", "RequestBy");
                    break;
            }
            GetMemberDelegate _delegate = GetMember;
            return _delegate.BeginInvoke(options, getMemberCallback, action);
        }

        /// <summary>
        /// Gets LinkedIn member details
        /// </summary>
        /// <param name="options">The object of type <see cref="LinkedInGetMemberOptions"/> representing retrieval options</param>
        /// <returns>Value containing <see cref="LinkedInMember"/> object and response status</returns>
        /// <exception cref="LinkedInMissingMemberIdException">Thrown when member is requested by id, but no member Id is set</exception>
        /// <exception cref="LinkedInMissingProfileUrlException">Thrown when member is requested by public URL, but no URL is set</exception>
        /// <exception cref="LinkedInInvalidProfileUrlException">Thrown when member is requested by public URL, but URL is not well formed</exception>
        /// <example>
        /// This sample shows how to call this method:
        /// <code>
        /// using LinkedIn.NET;
        /// using LinkedIn.NET.Groups;
        /// using LinkedIn.NET.Members;
        /// using LinkedIn.NET.Options;
        /// using LinkedIn.NET.Search;
        /// using LinkedIn.NET.Updates;
        /// ...
        /// // define retrieval options
        /// var options = new LinkedInGetMemberOptions();
        /// options.BasicProfileOptions.SelectAll();
        /// options.EmailProfileOptions.SelectAll();
        /// options.FullProfileOptions.SelectAll();
        /// // get member by ID
        /// options.Parameters.GetBy = LinkedInGetMemberBy.Id;
        /// options.Parameters.RequestBy = "12345";
        /// // call the method
        /// var response = _client.GetMember(options);
        /// // always check response.Result and response.Status before processing
        /// if (response.Result != null &amp;&amp; response.Status == LinkedInResponseStatus.OK)
        /// {
        ///     Console.WriteLine(@"Member name: {0} {1}", response.Result.BasicProfile.FirstName,
        ///        response.Result.BasicProfile.LastName);
        /// }
        /// else
        /// {
        ///     // show possible error message LinkedIn response
        ///     MessageBox.Show(response.Message);
        /// }
        /// </code>
        /// </example>
        public LinkedInResponse<LinkedInMember> GetMember(LinkedInGetMemberOptions options)
        {
            switch (options.Parameters.GetBy)
            {
                case LinkedInGetMemberBy.Id:
                    if (string.IsNullOrEmpty(options.Parameters.RequestBy))
                        throw new LinkedInMissingMemberIdException("Member Id is not set", "RequestBy");
                    break;
                case LinkedInGetMemberBy.Url:
                    if (string.IsNullOrEmpty(options.Parameters.RequestBy))
                        throw new LinkedInMissingProfileUrlException("User profile url is not set", "RequestBy");
                    if (!Uri.IsWellFormedUriString(options.Parameters.RequestBy, UriKind.RelativeOrAbsolute))
                        throw new LinkedInInvalidProfileUrlException("Invalid user profile url", "RequestBy");
                    break;
            }
            return RequestRunner.GetMember(options);
        }

        #endregion

        #region Share update region

        private delegate LinkedInResponse<LinkedInShareResult> ShareUpdateDelegate(LinkedInShareOptions options);

        private void shareUpdateCallback(IAsyncResult asyncResult)
        {
            var result = (AsyncResult)asyncResult;
            var _delegate = (ShareUpdateDelegate)result.AsyncDelegate;
            var share = _delegate.EndInvoke(asyncResult);
            var action = (Action<LinkedInResponse<LinkedInShareResult>>)asyncResult.AsyncState;
            action.Invoke(share);
        }

        /// <summary>
        /// Shares LinkedIn update asynchronously
        /// </summary>
        /// <param name="options">The object of type <see cref="LinkedInShareOptions"/> representing share options</param>
        /// <param name="action">Action to be invoked when update is shared</param>
        /// <returns>Status of asynchronous operation</returns>
        /// <exception cref="LinkedInMissingParameterException">Thrown when one or more share content's elements (Title, Description, SubmittedUrl, SubmittedImageUrl) are missing</exception>
        /// /// <example>
        /// This sample shows how to call this method:
        /// <code>
        /// using LinkedIn.NET;
        /// using LinkedIn.NET.Groups;
        /// using LinkedIn.NET.Members;
        /// using LinkedIn.NET.Options;
        /// using LinkedIn.NET.Search;
        /// using LinkedIn.NET.Updates;
        /// ...
        /// // define share options 
        /// var options = new LinkedInShareOptions();
        /// options.Title = "My share title";
        /// options.Description = "My share description";
        /// options.Comment = "This is my share comment";
        /// options.SubmittedUrl = "http://share.url.net";
        /// options.SubmittedImageUrl = "http://share.image.png";
        /// options.VisibilityCode = LinkedInShareVisibilityCode.Anyone;
        /// _Client.ShareUpdate(options, updateShared);
        /// ...
        /// // application defined function
        /// private void updateShared(LinkedInResponse&lt;LinkedInShareResult&gt; response)
        /// {
        ///     // always check response.Result and response.Status before processing
        ///     if (response.Result != null &amp;&amp; response.Status == LinkedInResponseStatus.OK)
        ///     {
        ///         MessageBox.Show(@"Update has been posted." + '\n' + @"Update key: " + response.Result.UpdateKey +
        ///                         '\n' + @"Update URL: " + response.Result.UpdateUrl);
        ///     }
        ///     else
        ///     {
        ///         // show possible error message LinkedIn response
        ///         MessageBox.Show(response.Message);
        ///     }
        /// }
        /// </code>
        /// </example>
        public IAsyncResult ShareUpdate(LinkedInShareOptions options, Action<LinkedInResponse<LinkedInShareResult>> action)
        {
            var contentPresented = Utils.IsAnyString(options.Title, options.SubmittedUrl, options.SubmittedImageUrl,
        options.Description);
            if (contentPresented)
            {
                if (string.IsNullOrEmpty(options.Title))
                    throw new LinkedInMissingParameterException("Share content's title cannot be null or empty", "Title");
                if (string.IsNullOrEmpty(options.Description))
                    throw new LinkedInMissingParameterException("Share content's description cannot be null or empty", "Description");
                if (string.IsNullOrEmpty(options.SubmittedUrl))
                    throw new LinkedInMissingParameterException("Share content's submitted URL cannot be null or empty", "SubmittedUrl");
                if (string.IsNullOrEmpty(options.SubmittedImageUrl))
                    throw new LinkedInMissingParameterException("Share content's submitted image URL cannot be null or empty", "SubmittedImageUrl");
            }
            ShareUpdateDelegate _delegate = ShareUpdate;
            return _delegate.BeginInvoke(options, shareUpdateCallback, action);
        }

        /// <summary>
        /// Shares LinkedIn update
        /// </summary>
        /// <param name="options">The object of type <see cref="LinkedInShareOptions"/> representing share options</param>
        /// <returns>Value containing <see cref="LinkedInShareResult"/> object and response status</returns>
        /// <exception cref="LinkedInMissingParameterException">Thrown when one or more share content's elements (Title, Description, SubmittedUrl, SubmittedImageUrl) are missing</exception>
        /// <example>
        /// This sample shows how to call this method:
        /// <code>
        /// using LinkedIn.NET;
        /// using LinkedIn.NET.Groups;
        /// using LinkedIn.NET.Members;
        /// using LinkedIn.NET.Options;
        /// using LinkedIn.NET.Search;
        /// using LinkedIn.NET.Updates;
        /// ...
        /// // define share options 
        /// var options = new LinkedInShareOptions();
        /// options.Title = "My share title";
        /// options.Description = "My share description";
        /// options.Comment = "This is my share comment";
        /// options.SubmittedUrl = "http://share.url.net";
        /// options.SubmittedImageUrl = "http://share.image.png";
        /// options.VisibilityCode = LinkedInShareVisibilityCode.Anyone;
        /// var response = _Client.ShareUpdate(options);
        /// // always check response.Result and response.Status before processing
        /// if (response.Result != null &amp;&amp; response.Status == LinkedInResponseStatus.OK)
        /// {
        ///     MessageBox.Show(@"Update has been posted." + '\n' + @"Update key: " + response.Result.UpdateKey +
        ///                         '\n' + @"Update URL: " + response.Result.UpdateUrl);
        /// }
        /// else
        /// {
        ///     // show possible error message LinkedIn response
        ///     MessageBox.Show(response.Message);
        /// }
        /// </code>
        /// </example>
        public LinkedInResponse<LinkedInShareResult> ShareUpdate(LinkedInShareOptions options)
        {
            var contentPresented = Utils.IsAnyString(options.Title, options.SubmittedUrl, options.SubmittedImageUrl,
                    options.Description);
            if (contentPresented)
            {
                if (string.IsNullOrEmpty(options.Title))
                    throw new LinkedInMissingParameterException("Share content's title cannot be null or empty", "Title");
                if (string.IsNullOrEmpty(options.Description))
                    throw new LinkedInMissingParameterException("Share content's description cannot be null or empty", "Description");
                if (string.IsNullOrEmpty(options.SubmittedUrl))
                    throw new LinkedInMissingParameterException("Share content's submitted URL cannot be null or empty", "SubmittedUrl");
                if (string.IsNullOrEmpty(options.SubmittedImageUrl))
                    throw new LinkedInMissingParameterException("Share content's submitted image URL cannot be null or empty", "SubmittedImageUrl");
            }
            return RequestRunner.ShareUpdate(options);
        }

        #endregion

        #region Get updates region

        private delegate LinkedInResponse<IEnumerable<LinkedInUpdate>> GetUpdatesDelegate(LinkedInGetUpdatesOptions options);

        private delegate LinkedInResponse<IEnumerable<LinkedInComment>> GetUpdateCommentsDelegate(string updateKey);

        private delegate LinkedInResponse<IEnumerable<LinkedInLike>> GetUpdateLikesDelegate(string updateKey);

        private void getUpdatesCallback(IAsyncResult asyncResult)
        {
            var result = (AsyncResult)asyncResult;
            var _delegate = (GetUpdatesDelegate)result.AsyncDelegate;
            var updates = _delegate.EndInvoke(asyncResult);
            var action = (Action<LinkedInResponse<IEnumerable<LinkedInUpdate>>>)asyncResult.AsyncState;
            action.Invoke(updates);
        }

        private void getUpdateCommentsCallback(IAsyncResult asyncResult)
        {
            var result = (AsyncResult)asyncResult;
            var _delegate = (GetUpdateCommentsDelegate)result.AsyncDelegate;
            var comments = _delegate.EndInvoke(asyncResult);
            var action = (Action<LinkedInResponse<IEnumerable<LinkedInComment>>>)asyncResult.AsyncState;
            action.Invoke(comments);
        }

        private void getUpdateLikesCallback(IAsyncResult asyncResult)
        {
            var result = (AsyncResult)asyncResult;
            var _delegate = (GetUpdateLikesDelegate)result.AsyncDelegate;
            var comments = _delegate.EndInvoke(asyncResult);
            var action = (Action<LinkedInResponse<IEnumerable<LinkedInLike>>>)asyncResult.AsyncState;
            action.Invoke(comments);
        }

        /// <summary>
        /// Gets LinkedIn updates asynchronously
        /// </summary>
        /// <param name="options">The object of type <see cref="LinkedInGetUpdatesOptions"/> representing retrieval options</param>
        /// <param name="action">Action to be invoked when the retrieval process ends</param>
        /// <returns>Status of asynchronous operation</returns>
        /// <exception cref="LinkedInNegativeParameterException">Thrown when UpdateCount or UpdateStart parameters of options are negative</exception>
        /// <exception cref="LinkedInMissingMemberIdException">Thrown when updates are requested by member's id, but no member Id is set</exception>
        /// <exception cref="LinkedInMissingProfileUrlException">Thrown when updates are requested by public member's URL, but no URL is set</exception>
        /// <exception cref="LinkedInInvalidProfileUrlException">Thrown when updates are requested by public member's URL, but URL is not well formed</exception>
        /// <exception cref="LinkedInCountIsZeroException">Thrown when UpdateCount parameter of options is equal to 0</exception>
        /// <example>
        /// This sample shows how to call this method:
        /// <code>
        /// using LinkedIn.NET;
        /// using LinkedIn.NET.Groups;
        /// using LinkedIn.NET.Members;
        /// using LinkedIn.NET.Options;
        /// using LinkedIn.NET.Search;
        /// using LinkedIn.NET.Updates;
        /// ... 
        /// // define retrieval options 
        /// var options = new LinkedInGetUpdatesOptions
        ///        {
        ///            UpdateScope = (LinkedInUpdateScope)cboScope.SelectedItem,
        ///            SortBy = (LinkedInUpdateSortField)cboSortBy.SelectedItem,
        ///            SortDirection = (LinkedInUpdateSortDirection)cboDirection.SelectedItem
        ///        };
        /// options.UpdateType = LinkedInUpdateType.AllAvailableUpdateTypes;
        /// options.Parameters.GetBy = LinkedInGetMemberBy.Self;
        /// _Client.GetUpdates(options, processUpdates);
        /// ...
        /// // application defined function
        /// private void processUpdates(LinkedInResponse&lt;IEnumerable&lt;LinkedInUpdate&gt;&gt; response)
        /// {
        ///     // always check response.Result and response.Status before processing 
        ///     if (response.Result != null &amp;&amp; response.Status == LinkedInResponseStatus.OK)
        ///     {
        ///         var linkedInUpdates = response.Result as LinkedInUpdate[] ?? response.Result.ToArray();
        ///         if (!linkedInUpdates.Any()) return;
        ///         foreach (var u in linkedInUpdates)
        ///         {
        ///             switch (u.UpdateType)
        ///             {
        ///                 case "SHAR":
        ///                     Console.WriteLine(((LinkedInShareUpdate)u).UpdateDate);
        ///                     break;
        ///                 case "PROF":
        ///                     // do something by casting: (LinkedInPositionUpdate)u
        ///                     break;
        ///                 case "CONN":
        ///                     // do something by casting: (LinkedInConnectionMemberUpdate)u
        ///                     break;
        ///                 case "NCON":
        ///                 case "CCEM":
        ///                     // do something by casting: (LinkedInConnectionUpdate)u
        ///                     break;
        ///                 case "STAT":
        ///                     // do something by casting: (LinkedInStatusUpdate)u
        ///                     break;
        ///                 case "VIRL":
        ///                     // do something by casting: (LinkedInViralUpdate)u
        ///                     break;
        ///                 case "JGRP":
        ///                     // do something by casting: (LinkedInGroupUpdate)u
        ///                     break;
        ///                    case "APPS":
        ///                 case "APPM":
        ///                     // do something by casting: (LinkedInApplicationUpdate)u
        ///                     break;
        ///                 case "PICU":
        ///                     // do something by casting: (LinkedInPictureUpdate)u
        ///                     break;
        ///                 case "PRFX":
        ///                     // do something by casting: (LinkedInExtendedProfileUpdate)u
        ///                     break;
        ///                 case "PREC":
        ///                 case "SVPR":
        ///                     // do something by casting: (LinkedInRecommendationUpdate)u
        ///                     break;
        ///                 case "JOBP":
        ///                     // do something by casting: (LinkedInJobPostingUpdate)u
        ///                     break;
        ///                 case "CMPY":
        ///                     var baseUpdate = (LinkedInCompanyBaseUpdate)u;
        ///                     switch (baseUpdate.CompanyUpdateType)
        ///                     {
        ///                         case LinkedInCompanyUpdateType.PersonUpdate:
        ///                             // do something by casting: (LinkedInCompanyPersonUpdate)u  
        ///                             break;
        ///                         case LinkedInCompanyUpdateType.JobUpdate:
        ///                             // do something by casting: (LinkedInCompanyJobUpdate)u
        ///                             break;
        ///                         case LinkedInCompanyUpdateType.ProfileUpdate:
        ///                             // do something by casting: (LinkedInCompanyProfileUpdate)u
        ///                             break;
        ///                         case LinkedInCompanyUpdateType.StatusUpdate:
        ///                             // do something by casting: (LinkedInCompanyStatusUpdate)u
        ///                             break;
        ///                     }
        ///                     break;
        ///                 case "MSFC":
        ///                     // do something by casting: (LinkedInStartFollowCompanyUpdate)u
        ///                     break;
        ///             }
        ///         }
        ///     }
        ///     else
        ///     {
        ///         // show possible error message LinkedIn response
        ///         MessageBox.Show(response.Message);
        ///     }
        /// }
        /// </code>
        /// </example>
        public IAsyncResult GetUpdates(LinkedInGetUpdatesOptions options, Action<LinkedInResponse<IEnumerable<LinkedInUpdate>>> action)
        {
            if (options.UpdateCount < 0)
                throw new LinkedInNegativeParameterException("Count of updates to retrieve cannot be negative", "UpdateCount");
            if (options.UpdateStart < 0)
                throw new LinkedInNegativeParameterException("Start point to retrieving updates cannot be negative", "UpdateStart");
            if (options.UpdateCount == 0)
                throw new LinkedInCountIsZeroException("Count parameter of query string cannot be 0");
            switch (options.Parameters.GetBy)
            {
                case LinkedInGetMemberBy.Id:
                    if (string.IsNullOrEmpty(options.Parameters.RequestBy))
                        throw new LinkedInMissingMemberIdException("Member Id is not set", "RequestBy");
                    break;
                case LinkedInGetMemberBy.Url:
                    if (string.IsNullOrEmpty(options.Parameters.RequestBy))
                        throw new LinkedInMissingProfileUrlException("User profile url is not set",
                            "RequestBy");
                    if (
                        !Uri.IsWellFormedUriString(options.Parameters.RequestBy,
                            UriKind.RelativeOrAbsolute))
                        throw new LinkedInInvalidProfileUrlException("Invalid user profile url",
                            "RequestBy");
                    break;
            }
            GetUpdatesDelegate _delegate = GetUpdates;
            return _delegate.BeginInvoke(options, getUpdatesCallback, action);
        }

        /// <summary>
        /// Gets LinkedIn updates
        /// </summary>
        /// <param name="options">The object of type <see cref="LinkedInGetUpdatesOptions"/> representing retrieval options</param>
        /// <returns>Value containing collection of <see cref="LinkedInUpdate"/> objects and response status</returns>
        /// <exception cref="LinkedInNegativeParameterException">Thrown when UpdateCount or UpdateStart parameters of options are negative</exception>
        /// <exception cref="LinkedInMissingMemberIdException">Thrown when updates are requested by member's id, but no member Id is set</exception>
        /// <exception cref="LinkedInMissingProfileUrlException">Thrown when updates are requested by public member's URL, but no URL is set</exception>
        /// <exception cref="LinkedInInvalidProfileUrlException">Thrown when updates are requested by public member's URL, but URL is not well formed</exception>
        /// <exception cref="LinkedInCountIsZeroException">Thrown when UpdateCount parameter of options is equal to 0</exception>
        /// <example>
        /// This sample shows how to call this method:
        /// <code>
        /// using LinkedIn.NET;
        /// using LinkedIn.NET.Groups;
        /// using LinkedIn.NET.Members;
        /// using LinkedIn.NET.Options;
        /// using LinkedIn.NET.Search;
        /// using LinkedIn.NET.Updates;
        /// ... 
        /// // define retrieval options 
        /// var options = new LinkedInGetUpdatesOptions
        ///        {
        ///            UpdateScope = (LinkedInUpdateScope)cboScope.SelectedItem,
        ///            SortBy = (LinkedInUpdateSortField)cboSortBy.SelectedItem,
        ///            SortDirection = (LinkedInUpdateSortDirection)cboDirection.SelectedItem
        ///        };
        /// options.UpdateType = LinkedInUpdateType.AllAvailableUpdateTypes;
        /// options.Parameters.GetBy = LinkedInGetMemberBy.Self;
        /// var response = _Client.GetUpdates(options);
        /// 
        /// // always check response.Result and response.Status before processing 
        /// if (response.Result != null &amp;&amp; response.Status == LinkedInResponseStatus.OK)
        /// {
        ///     var linkedInUpdates = response.Result as LinkedInUpdate[] ?? response.Result.ToArray();
        ///     if (!linkedInUpdates.Any()) return;
        ///     foreach (var u in linkedInUpdates)
        ///     {
        ///         switch (u.UpdateType)
        ///         {
        ///             case "SHAR":
        ///                 Console.WriteLine(((LinkedInShareUpdate)u).UpdateDate);
        ///                 break;
        ///             case "PROF":
        ///                 // do something by casting: (LinkedInPositionUpdate)u
        ///                 break;
        ///             case "CONN":
        ///                 // do something by casting: (LinkedInConnectionMemberUpdate)u
        ///                 break;
        ///             case "NCON":
        ///             case "CCEM":
        ///                 // do something by casting: (LinkedInConnectionUpdate)u
        ///                 break;
        ///             case "STAT":
        ///                 // do something by casting: (LinkedInStatusUpdate)u
        ///                 break;
        ///             case "VIRL":
        ///                 // do something by casting: (LinkedInViralUpdate)u
        ///                 break;
        ///             case "JGRP":
        ///                 // do something by casting: (LinkedInGroupUpdate)u
        ///                 break;
        ///                case "APPS":
        ///             case "APPM":
        ///                 // do something by casting: (LinkedInApplicationUpdate)u
        ///                 break;
        ///             case "PICU":
        ///                 // do something by casting: (LinkedInPictureUpdate)u
        ///                 break;
        ///             case "PRFX":
        ///                 // do something by casting: (LinkedInExtendedProfileUpdate)u
        ///                 break;
        ///             case "PREC":
        ///             case "SVPR":
        ///                 // do something by casting: (LinkedInRecommendationUpdate)u
        ///                 break;
        ///             case "JOBP":
        ///                 // do something by casting: (LinkedInJobPostingUpdate)u
        ///                 break;
        ///             case "CMPY":
        ///                 var baseUpdate = (LinkedInCompanyBaseUpdate)u;
        ///                 switch (baseUpdate.CompanyUpdateType)
        ///                 {
        ///                     case LinkedInCompanyUpdateType.PersonUpdate:
        ///                         // do something by casting: (LinkedInCompanyPersonUpdate)u  
        ///                         break;
        ///                     case LinkedInCompanyUpdateType.JobUpdate:
        ///                         // do something by casting: (LinkedInCompanyJobUpdate)u
        ///                         break;
        ///                     case LinkedInCompanyUpdateType.ProfileUpdate:
        ///                         // do something by casting: (LinkedInCompanyProfileUpdate)u
        ///                         break;
        ///                     case LinkedInCompanyUpdateType.StatusUpdate:
        ///                         // do something by casting: (LinkedInCompanyStatusUpdate)u
        ///                         break;
        ///                 }
        ///                 break;
        ///             case "MSFC":
        ///                 // do something by casting: (LinkedInStartFollowCompanyUpdate)u
        ///                 break;
        ///         }
        ///     }
        /// }
        /// else
        /// {
        ///     // show possible error message LinkedIn response
        ///     MessageBox.Show(response.Message);
        /// }
        /// </code>
        /// </example>
        public LinkedInResponse<IEnumerable<LinkedInUpdate>> GetUpdates(LinkedInGetUpdatesOptions options)
        {
            if (options.UpdateCount < 0)
                throw new LinkedInNegativeParameterException("Count of updates to retrieve cannot be negative", "UpdateCount");
            if (options.UpdateStart < 0)
                throw new LinkedInNegativeParameterException("Start point to retrieving updates cannot be negative", "UpdateStart");
            if (options.UpdateCount == 0)
                throw new LinkedInCountIsZeroException("Count parameter of query string cannot be 0");

            switch (options.Parameters.GetBy)
            {
                case LinkedInGetMemberBy.Id:
                    if (string.IsNullOrEmpty(options.Parameters.RequestBy))
                        throw new LinkedInMissingMemberIdException("Member Id is not set", "RequestBy");
                    break;
                case LinkedInGetMemberBy.Url:    //LinkedInGetMemberBy.Url
                    if (string.IsNullOrEmpty(options.Parameters.RequestBy))
                        throw new LinkedInMissingProfileUrlException("User profile url is not set",
                            "RequestBy");
                    if (!Uri.IsWellFormedUriString(options.Parameters.RequestBy, UriKind.RelativeOrAbsolute))
                        throw new LinkedInInvalidProfileUrlException("Invalid user profile url", "RequestBy");
                    break;
            }
            return RequestRunner.GetUpdates(options);
        }

        /// <summary>
        /// Gets LinkedIn update's comments asynchronously
        /// </summary>
        /// <param name="updateKey">Update key</param>
        /// <param name="action">Action to be invoked when the retrieval process ends</param>
        /// <returns>Status of asynchronous operation</returns>
        /// <exception cref="LinkedInMissingParameterException">Thrown when update key is null or empty string</exception>
        /// <example>
        /// This sample shows how to call this method:
        /// <code>
        /// using LinkedIn.NET;
        /// using LinkedIn.NET.Groups;
        /// using LinkedIn.NET.Members;
        /// using LinkedIn.NET.Options;
        /// using LinkedIn.NET.Search;
        /// using LinkedIn.NET.Updates;
        /// ... 
        /// _Client.GetUpdateComments("12345", updateCommentsReceived);
        /// ...
        /// // application defined function
        /// private void updateCommentsReceived(LinkedInResponse&lt;IEnumerable&lt;LinkedInComment&gt;&gt; response)
        /// {
        ///     // always check response.Result and response.Status before processing 
        ///     if (response.Result != null &amp;&amp; response.Status == LinkedInResponseStatus.OK)
        ///     {
        ///         foreach (var c in response.Result)
        ///         {
        ///             Console.WriteLine(c.Comment);
        ///         }
        ///     }
        ///     else
        ///     {
        ///         // show possible error message LinkedIn response
        ///         MessageBox.Show(response.Message);
        ///     }
        /// }
        /// </code>
        /// </example>
        public IAsyncResult GetUpdateComments(string updateKey,
            Action<LinkedInResponse<IEnumerable<LinkedInComment>>> action)
        {
            if (string.IsNullOrEmpty(updateKey))
                throw new LinkedInMissingParameterException("Update key cannot be null or empty", "updateKey");
            GetUpdateCommentsDelegate _delegate = getUpdateComments;
            return _delegate.BeginInvoke(updateKey, getUpdateCommentsCallback, action);
        }

        /// <summary>
        /// Gets LinkedIn update's likes asynchronously
        /// </summary>
        /// <param name="updateKey">Update key</param>
        /// <param name="action">Action to be invoked when the retrieval process ends</param>
        /// <returns>Status of asynchronous operation</returns>
        /// <exception cref="LinkedInMissingParameterException">Thrown when update key is null or empty string</exception>
        /// /// <example>
        /// This sample shows how to call this method:
        /// <code>
        /// using LinkedIn.NET;
        /// using LinkedIn.NET.Groups;
        /// using LinkedIn.NET.Members;
        /// using LinkedIn.NET.Options;
        /// using LinkedIn.NET.Search;
        /// using LinkedIn.NET.Updates;
        /// ... 
        /// _Client.GetUpdateLikes("12345", updateLikesReceived);
        /// ...
        /// // application defined function
        /// private void updateLikesReceived(LinkedInResponse&lt;IEnumerable&lt;LinkedInLike&gt;&gt; response)
        /// {
        ///     // always check response.Result and response.Status before processing 
        ///     if (response.Result != null &amp;&amp; response.Status == LinkedInResponseStatus.OK)
        ///     {
        ///         foreach (var c in response.Result)
        ///         {
        ///             Console.WriteLine(c.Person.FirstName);
        ///         }
        ///     }
        ///     else
        ///     {
        ///         // show possible error message LinkedIn response
        ///         MessageBox.Show(response.Message);
        ///     }
        /// }
        /// </code>
        /// </example>
        public IAsyncResult GetUpdateLikes(string updateKey, Action<LinkedInResponse<IEnumerable<LinkedInLike>>> action)
        {
            if (string.IsNullOrEmpty(updateKey))
                throw new LinkedInMissingParameterException("Update key cannot be null or empty", "updateKey");
            GetUpdateLikesDelegate _delegate = getUpdateLikes;
            return _delegate.BeginInvoke(updateKey, getUpdateLikesCallback, action);
        }

        private LinkedInResponse<IEnumerable<LinkedInComment>> getUpdateComments(string updateKey)
        {
            return RequestRunner.GetAllUpdateComments(updateKey);
        }

        private LinkedInResponse<IEnumerable<LinkedInLike>> getUpdateLikes(string updateKey)
        {
            return RequestRunner.GetAllUpdateLikes(updateKey);
        }

        #endregion

        #region Search region

        private delegate LinkedInResponse<LinkedInSearchResult> SearchDelegate(LinkedInSearchOptions options);

        private void searchCallback(IAsyncResult asyncResult)
        {
            var result = (AsyncResult)asyncResult;
            var _delegate = (SearchDelegate)result.AsyncDelegate;
            var searchResult = _delegate.EndInvoke(asyncResult);
            var action = (Action<LinkedInResponse<LinkedInSearchResult>>)asyncResult.AsyncState;
            action.Invoke(searchResult);
        }

        /// <summary>
        /// Asynchronously searches LinkedIn using specified options
        /// </summary>
        /// <param name="options">The object of type <see cref="LinkedInSearchOptions"/> representing search options</param>
        /// <param name="action">Action to be invoked when the search process ends</param>
        /// <returns>Status of asynchronous operation</returns>
        /// <exception cref="LinkedInNegativeParameterException">Thrown when Start or Count parameters of options are negative</exception>
        /// <exception cref="LinkedInCountIsZeroException">Thrown when Count parameter of options is equal to 0</exception>
        /// <remarks>For better understanding of LinkedIn People Search API, please visit <a href="https://developer.linkedin.com/documents/people-search-api">LinkedIn developers page</a></remarks>
        /// <example>
        /// This sample shows how to call this method:
        /// <code>
        /// using LinkedIn.NET;
        /// using LinkedIn.NET.Groups;
        /// using LinkedIn.NET.Members;
        /// using LinkedIn.NET.Options;
        /// using LinkedIn.NET.Search;
        /// using LinkedIn.NET.Updates;
        /// ... 
        /// // define search options
        /// var options = new LinkedInSearchOptions();
        /// // set various options parameters
        /// ...
        /// _Client.Search(options, getSearchResult);
        /// ...
        /// // application defined function
        /// private void getSearchResult(LinkedInResponse&lt;LinkedInSearchResult&gt; response)
        /// {
        ///     // always check response.Result and response.Status before processing 
        ///     if (response.Result != null &amp;&amp; response.Status == LinkedInResponseStatus.OK)
        ///     {
        ///         // do something with response.Result.People and/or response.Result.Facets
        ///     }
        ///     else
        ///     {
        ///         // show possible error message LinkedIn response
        ///         MessageBox.Show(response.Message);
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="LinkedInSearchOptions"/>
        public IAsyncResult Search(LinkedInSearchOptions options, Action<LinkedInResponse<LinkedInSearchResult>> action)
        {
            if (options.Start.HasValue && options.Start < 0)
                throw new LinkedInNegativeParameterException(
                    "Start parameter of query string cannot be less than zero", "Start");
            if (options.Count.HasValue && options.Count < 0)
                throw new LinkedInNegativeParameterException(
                    "Count parameter of query string cannot be less than zero", "Count");
            if (options.Count.HasValue && options.Count == 0)
                throw new LinkedInCountIsZeroException("Count parameter of query string cannot be 0");

            SearchDelegate _delegate = Search;
            return _delegate.BeginInvoke(options, searchCallback, action);
        }

        /// <summary>
        /// Searches LinkedIn using specified options
        /// </summary>
        /// <param name="options">The object of type <see cref="LinkedInSearchOptions"/> representing search options</param>
        /// <returns>Value containing <see cref="LinkedInSearchResult"/> object and response status</returns>
        /// <exception cref="LinkedInNegativeParameterException">Thrown when Start or Count parameters of options are negative</exception>
        /// <exception cref="LinkedInCountIsZeroException">Thrown when Count parameter of options is equal to 0</exception>
        /// <remarks>For better understanding of LinkedIn People Search API, please visit <a href="https://developer.linkedin.com/documents/people-search-api">LinkedIn developers page</a></remarks>
        /// <example>
        /// This sample shows how to call this method:
        /// <code>
        /// using LinkedIn.NET;
        /// using LinkedIn.NET.Groups;
        /// using LinkedIn.NET.Members;
        /// using LinkedIn.NET.Options;
        /// using LinkedIn.NET.Search;
        /// using LinkedIn.NET.Updates;
        /// ... 
        /// // define search options
        /// var options = new LinkedInSearchOptions();
        /// // set various options parameters
        /// ...
        /// var response = _Client.Search(options);
        /// 
        /// // always check response.Result and response.Status before processing 
        /// if (response.Result != null &amp;&amp; response.Status == LinkedInResponseStatus.OK)
        /// {
        ///     // do something with response.Result.People and/or response.Result.Facets
        /// }
        /// else
        /// {
        ///     // show possible error message LinkedIn response
        ///     MessageBox.Show(response.Message);
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="LinkedInSearchOptions"/>
        public LinkedInResponse<LinkedInSearchResult> Search(LinkedInSearchOptions options)
        {
            if (options.Start.HasValue && options.Start < 0)
                throw new LinkedInNegativeParameterException(
                    "Start parameter of query string cannot be less than zero", "Start");
            if (options.Count.HasValue && options.Count < 0)
                throw new LinkedInNegativeParameterException(
                    "Count parameter of query string cannot be less than zero", "Count");
            if (options.Count.HasValue && options.Count == 0)
                throw new LinkedInCountIsZeroException("Count parameter of query string cannot be 0");

            return RequestRunner.GetSearchResult(options);
        }

        #endregion

        #region Groups region

        private delegate LinkedInResponse<IEnumerable<LinkedInGroup>> ListGroupsDelegate(LinkedInGetGroupOptions options);
        private delegate LinkedInResponse<IEnumerable<LinkedInGroupPost>> ListPostsDelegate(LinkedInGetGroupPostsOptions options);
        private delegate LinkedInResponse<IEnumerable<LinkedInGroupComment>> ListCommentsDelegate(LinkedInGetGroupPostCommentsOptions options);

        private void getCommentsCallback(IAsyncResult asyncResult)
        {
            var result = (AsyncResult)asyncResult;
            var _delegate = (ListCommentsDelegate)result.AsyncDelegate;
            var getResult = _delegate.EndInvoke(asyncResult);
            var action = (Action<LinkedInResponse<IEnumerable<LinkedInGroupComment>>>)asyncResult.AsyncState;
            action.Invoke(getResult);
        }

        private void getPostsCallback(IAsyncResult asyncResult)
        {
            var result = (AsyncResult)asyncResult;
            var _delegate = (ListPostsDelegate)result.AsyncDelegate;
            var getResult = _delegate.EndInvoke(asyncResult);
            var action = (Action<LinkedInResponse<IEnumerable<LinkedInGroupPost>>>)asyncResult.AsyncState;
            action.Invoke(getResult);
        }

        private void listGroupsCallback(IAsyncResult asyncResult)
        {
            var result = (AsyncResult)asyncResult;
            var _delegate = (ListGroupsDelegate)result.AsyncDelegate;
            var getResult = _delegate.EndInvoke(asyncResult);
            var action = (Action<LinkedInResponse<IEnumerable<LinkedInGroup>>>)asyncResult.AsyncState;
            action.Invoke(getResult);
        }

        /// <summary>
        /// Gets group post's comments asynchronously
        /// </summary>
        /// <param name="options">The object of type <see cref="LinkedInGetGroupPostCommentsOptions"/> representing retrieval options</param>
        /// <param name="action">Action to be invoked when the retrieval process ends</param>
        /// <returns>Status of asynchronous operation</returns>
        /// <example>
        /// This sample shows how to call this method:
        /// <code>
        /// using LinkedIn.NET;
        /// using LinkedIn.NET.Groups;
        /// using LinkedIn.NET.Members;
        /// using LinkedIn.NET.Options;
        /// using LinkedIn.NET.Search;
        /// using LinkedIn.NET.Updates;
        /// ... 
        /// // define retrieval options 
        /// var options = new LinkedInGetGroupPostCommentsOptions();
        /// options.CommentOptions.SelectAll();
        /// options.PostId = 12345;
        /// _Client.GetPostComments(options, commentsReceived);
        /// ...
        /// // application defined function
        /// private void commentsReceived(LinkedInResponse&lt;IEnumerable&lt;LinkedInGroupComment&gt;&gt; response)
        /// {
        ///     // always check response.Result and response.Status before processing 
        ///     if (response.Result != null &amp;&amp; response.Status == LinkedInResponseStatus.OK)
        ///     {
        ///         foreach (var c in response.Result)
        ///         {
        ///             Console.WriteLine(c.Text);
        ///         }
        ///     }
        ///     else
        ///     {
        ///         // show possible error message LinkedIn response
        ///         MessageBox.Show(response.Message);
        ///     }
        ///  }
        /// </code>
        /// </example>
        public IAsyncResult GetPostComments(LinkedInGetGroupPostCommentsOptions options,
            Action<LinkedInResponse<IEnumerable<LinkedInGroupComment>>> action)
        {
            ListCommentsDelegate _delegate = getComments;
            return _delegate.BeginInvoke(options, getCommentsCallback, action);
        }

        private LinkedInResponse<IEnumerable<LinkedInGroupComment>> getComments(
            LinkedInGetGroupPostCommentsOptions options)
        {
            try
            {
                return new LinkedInResponse<IEnumerable<LinkedInGroupComment>>(RequestRunner.GetPostComments(options),
                        LinkedInResponseStatus.OK, options.PostId);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse<IEnumerable<LinkedInGroupComment>>(null, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<IEnumerable<LinkedInGroupComment>>(null, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        /// <summary>
        /// Asynchronously gets group's posts
        /// </summary>
        /// <param name="options">The object of type <see cref="LinkedInGetGroupPostsOptions"/> representing retrieval options</param>
        /// <param name="action">Action to be invoked when the retrieval process ends</param>
        /// <returns>Status of asynchronous operation</returns>
        /// <example>
        /// This sample shows how to call this method:
        /// <code>
        /// using LinkedIn.NET;
        /// using LinkedIn.NET.Groups;
        /// using LinkedIn.NET.Members;
        /// using LinkedIn.NET.Options;
        /// using LinkedIn.NET.Search;
        /// using LinkedIn.NET.Updates;
        /// ... 
        /// // define retrieval options 
        /// var options = new LinkedInGetGroupPostsOptions();
        /// options.PostOptions.SelectAll();
        /// options.GroupId = 12345;
        /// _Client.GetGroupPosts(options, postsMemberReceived);
        /// ...
        /// // application defined function
        /// private void postsMemberReceived(LinkedInResponse&lt;IEnumerable&lt;LinkedInGroupPost&gt;&gt; response)
        /// {
        ///     // always check response.Result and response.Status before processing 
        ///     if (response.Result != null &amp;&amp; response.Status == LinkedInResponseStatus.OK)
        ///     {
        ///         foreach (var post in response.Result)
        ///         {
        ///             Console.WriteLine(post.Summary);
        ///         }
        ///     }
        ///     else
        ///     {
        ///         // show possible error message LinkedIn response
        ///         MessageBox.Show(response.Message);
        ///     }
        ///  }
        /// </code>
        /// </example>
        public IAsyncResult GetGroupPosts(LinkedInGetGroupPostsOptions options, Action<LinkedInResponse<IEnumerable<LinkedInGroupPost>>> action)
        {
            ListPostsDelegate _delegate = getPosts;
            return _delegate.BeginInvoke(options, getPostsCallback, action);
        }

        private LinkedInResponse<IEnumerable<LinkedInGroupPost>> getPosts(LinkedInGetGroupPostsOptions options)
        {
            try
            {
                return new LinkedInResponse<IEnumerable<LinkedInGroupPost>>(RequestRunner.GetGroupPosts(options),
                        LinkedInResponseStatus.OK, options.GroupId);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse<IEnumerable<LinkedInGroupPost>>(null, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<IEnumerable<LinkedInGroupPost>>(null, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        /// <summary>
        /// Asynchronously gets groups that currently logged in user belongs to
        /// </summary>
        /// <param name="options">The object of type <see cref="LinkedInGetGroupOptions"/> representing retrieval options</param>
        /// <param name="action">Action to be invoked when the retrieval process ends</param>
        /// <returns>Status of asynchronous operation</returns>
        /// <example>
        /// This sample shows how to call this method:
        /// <code>
        /// using LinkedIn.NET;
        /// using LinkedIn.NET.Groups;
        /// using LinkedIn.NET.Members;
        /// using LinkedIn.NET.Options;
        /// using LinkedIn.NET.Search;
        /// using LinkedIn.NET.Updates;
        /// ...
        /// // define retrieval options
        /// var options = new LinkedInGetGroupOptions();
        /// options.GroupOptions.SelectAll();
        /// _Client.GetMemberGroups(options, listMemberGroups)
        /// ...
        /// // application defined function
        /// private void listMemberGroups(LinkedInResponse&lt;IEnumerable&lt;LinkedInGroup&gt;&gt; result)
        /// {
        ///     // always check response.Result and response.Status before processing
        ///     if (result.Result != null &amp;&amp; result.Status == LinkedInResponseStatus.OK)
        ///     {
        ///         foreach (var r in result.Result)
        ///         {
        ///             Console.WriteLine(r.Name);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public IAsyncResult GetMemberGroups(LinkedInGetGroupOptions options,
            Action<LinkedInResponse<IEnumerable<LinkedInGroup>>> action)
        {
            ListGroupsDelegate _delegate = GetMemberGroups;
            return _delegate.BeginInvoke(options, listGroupsCallback, action);
        }

        /// <summary>
        /// Gets groups that currently logged in user belongs to
        /// </summary>
        /// <param name="options">The object of type <see cref="LinkedInGetGroupOptions"/> representing retrieval options</param>
        /// <returns>Value containing collection of <see cref="LinkedInGroup"/> objects and response status</returns>
        /// <example>
        /// This sample shows how to call this method:
        /// <code>
        /// using LinkedIn.NET;
        /// using LinkedIn.NET.Groups;
        /// using LinkedIn.NET.Members;
        /// using LinkedIn.NET.Options;
        /// using LinkedIn.NET.Search;
        /// using LinkedIn.NET.Updates;
        /// ...
        /// // define retrieval options
        /// var options = new LinkedInGetGroupOptions();
        /// options.GroupOptions.SelectAll();
        /// var result = _Client.GetMemberGroups(options)
        /// // always check response.Result and response.Status before processing
        /// if (result.Result != null &amp;&amp; result.Status == LinkedInResponseStatus.OK)
        /// {
        ///     foreach (var r in result.Result)
        ///     {
        ///         Console.WriteLine(r.Name);
        ///     }
        /// }
        /// </code>
        /// </example>
        public LinkedInResponse<IEnumerable<LinkedInGroup>> GetMemberGroups(LinkedInGetGroupOptions options)
        {
            try
            {
                var sb = new StringBuilder(Utils.GROUPS_MEMBERSHIP_URL);
                var paramsGroup = RequestFields.PrepareGroupFields(options);
                if (!string.IsNullOrEmpty(paramsGroup))
                    sb.Append(paramsGroup);
                sb.Append(",membership-state,show-group-logo-in-profile,allow-messages-from-members,email-digest-frequency,email-announcements-from-managers,email-for-every-new-post)?start=0&count=250&oauth2_access_token=");
                sb.Append(Singleton.Instance.AccessToken);
                return
                    new LinkedInResponse<IEnumerable<LinkedInGroup>>(
                        RequestRunner.GetMemberGroups(sb.ToString(), options),
                        LinkedInResponseStatus.OK, null);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse<IEnumerable<LinkedInGroup>>(null, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<IEnumerable<LinkedInGroup>>(null, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        /// <summary>
        /// Get groups suggested to member
        /// </summary>
        /// <param name="options">The object of type <see cref="LinkedInGetGroupOptions"/> representing retrieval options</param>
        /// <returns>Value containing collection of <see cref="LinkedInGroup"/> objects and response status</returns>
        /// <example>
        /// This sample shows how to call this method:
        /// <code>
        /// using LinkedIn.NET;
        /// using LinkedIn.NET.Groups;
        /// using LinkedIn.NET.Members;
        /// using LinkedIn.NET.Options;
        /// using LinkedIn.NET.Search;
        /// using LinkedIn.NET.Updates;
        /// ...
        /// // define retrieval options
        /// var options = new LinkedInGetGroupOptions();
        /// options.GroupOptions.SelectAll();
        /// var result = _Client.GetSuggestedGroups(options)
        /// // always check response.Result and response.Status before processing
        /// if (result.Result != null &amp;&amp; result.Status == LinkedInResponseStatus.OK)
        /// {
        ///     foreach (var r in result.Result)
        ///     {
        ///         Console.WriteLine(r.Name);
        ///     }
        /// }
        /// </code>
        /// </example>
        public LinkedInResponse<IEnumerable<LinkedInGroup>> GetSuggestedGroups(LinkedInGetGroupOptions options)
        {
            try
            {
                var sb = new StringBuilder(Utils.GROUPS_SUGGESTIONS_URL);
                var paramsGroup = RequestFields.PrepareGroupFields(options);
                if (!string.IsNullOrEmpty(paramsGroup))
                    sb.Append(paramsGroup);

                sb.Append("?start=0&count=250&oauth2_access_token=");
                sb.Append(Singleton.Instance.AccessToken);
                return
                    new LinkedInResponse<IEnumerable<LinkedInGroup>>(
                        RequestRunner.GetSuggestedGroups(sb.ToString(), options),
                        LinkedInResponseStatus.OK, null);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse<IEnumerable<LinkedInGroup>>(null, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<IEnumerable<LinkedInGroup>>(null, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        /// <summary>
        /// Asynchronously gets groups suggested to member
        /// </summary>
        /// <param name="options">The object of type <see cref="LinkedInGetGroupOptions"/> representing retrieval options</param>
        /// <param name="action">Action to be invoked when the retrieval process ends</param>
        /// <returns>Status of asynchronous operation</returns>
        /// <example>
        /// This sample shows how to call this method:
        /// <code>
        /// using LinkedIn.NET;
        /// using LinkedIn.NET.Groups;
        /// using LinkedIn.NET.Members;
        /// using LinkedIn.NET.Options;
        /// using LinkedIn.NET.Search;
        /// using LinkedIn.NET.Updates;
        /// ...
        /// // define retrieval options
        /// var options = new LinkedInGetGroupOptions();
        /// options.GroupOptions.SelectAll();
        /// _Client.GetSuggestedGroups(options, listSuggestedGroups)
        /// ...
        /// // application defined function
        /// private void listSuggestedGroups(LinkedInResponse&lt;IEnumerable&lt;LinkedInGroup&gt;&gt; result)
        /// {
        ///     // always check response.Result and response.Status before processing
        ///     if (result.Result != null &amp;&amp; result.Status == LinkedInResponseStatus.OK)
        ///     {
        ///         foreach (var r in result.Result)
        ///         {
        ///             Console.WriteLine(r.Name);
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example> 
        public IAsyncResult GetSuggestedGroups(LinkedInGetGroupOptions options,
            Action<LinkedInResponse<IEnumerable<LinkedInGroup>>> action)
        {
            ListGroupsDelegate _delegate = GetSuggestedGroups;
            return _delegate.BeginInvoke(options, listGroupsCallback, action);
        }

        #endregion

        #region Messages region
        /// <summary>
        /// Sends message over LinkedIn network
        /// </summary>
        /// <param name="options">The oject of type <see cref="LinkedInMessageOptions"/> representing message options</param>
        /// <returns>Value containing <see cref="LinkedInSearchResult"/> object and response status</returns>
        /// <exception cref="LinkedInMissingParameterException">Thrown when message's subject or body are null or empty strings</exception>
        /// <exception cref="LinkedInNoRecipientsException">Thrown when there are no recipients</exception>
        /// <example>
        /// This sample shows how to call this method:
        /// <code>
        /// using LinkedIn.NET;
        /// using LinkedIn.NET.Groups;
        /// using LinkedIn.NET.Members;
        /// using LinkedIn.NET.Options;
        /// using LinkedIn.NET.Search;
        /// using LinkedIn.NET.Updates;
        /// ...
        /// // define message options
        /// var options = new LinkedInMessageOptions
        ///        {
        ///            Subject = "Message subject",
        ///            Body = "Message body",
        ///            IncludeSenderInRecipients = true
        ///        };
        /// // add recipients
        /// options.Recipients.Add("John Smith");
        /// // add more recipients
        /// options.Recipients.Add("Homer Simpson");
        /// // send message
        /// var response = _Client.SendMessage(options);
        /// // always check response.Result and response.Status before processing
        /// if (result.Result != null &amp;&amp; result.Status == LinkedInResponseStatus.OK)
        /// {
        ///     MessageBox.Show(@"Message sent successfully.");
        /// }
        /// </code>
        /// </example>
        public LinkedInResponse<bool> SendMessage(LinkedInMessageOptions options)
        {
            if (string.IsNullOrEmpty(options.Subject))
                throw new LinkedInMissingParameterException("Message's subject cannot be null or empty", "Subject");
            if (string.IsNullOrEmpty(options.Body))
                throw new LinkedInMissingParameterException("Message's body cannot be null or empty", "Body");
            if (options.Recipients.Count == 0 && !options.IncludeSenderInRecipients)
                throw new LinkedInNoRecipientsException("Recipients list is empty");
            return RequestRunner.SendMessage(options);
        }

        /// <summary>
        /// Sends invitation over LinkedIn network
        /// </summary>
        /// <param name="options">The object of type <see cref="LinkedInInvitationOptions"/> representing invitation options</param>
        /// <returns>Value containing <see cref="LinkedInSearchResult"/> object and response status</returns>
        /// <exception cref="LinkedInMissingParameterException">Thrown when options.InvitationType is set to InviteById and either RecipientId or AuthorizationName or AuthorizationValue is missing, 
        /// or when options.InvitationType is set to InviteByEmail and either RecipientEmail or RecipientFirstName or RecipientLastName is missing
        /// </exception>
        /// /// <example>
        /// This sample shows how to call this method:
        /// <code>
        /// using LinkedIn.NET;
        /// using LinkedIn.NET.Groups;
        /// using LinkedIn.NET.Members;
        /// using LinkedIn.NET.Options;
        /// using LinkedIn.NET.Search;
        /// using LinkedIn.NET.Updates;
        /// ...
        /// // define invitation options
        /// var options = new LinkedInInvitationOptions
        ///        {
        ///            InvitationType = LinkedInInvitationType.InviteByEmail,
        ///            Subject = "Invitation subject",
        ///            Body = "Invitation body",
        ///            RecipientEmail = "recip@recip.com",
        ///            RecipientFirstName = "Recipient first name",
        ///            RecipientLastName = "Recipient last name"
        ///        };
        /// // send invitation
        /// var response = _Client.SendInvitation(options);
        /// // always check response.Result and response.Status before processing
        /// if (result.Result != null &amp;&amp; result.Status == LinkedInResponseStatus.OK)
        /// {
        ///     MessageBox.Show(@"Invitation sent successfully.");
        /// }
        /// </code>
        /// </example>
        public LinkedInResponse<bool> SendInvitation(LinkedInInvitationOptions options)
        {
            if (string.IsNullOrEmpty(options.Subject))
                throw new LinkedInMissingParameterException("Invitation's subject cannot be null or empty", "Subject");
            if (string.IsNullOrEmpty(options.Body))
                throw new LinkedInMissingParameterException("Invitation's body cannot be null or empty", "Body");
            switch (options.InvitationType)
            {
                case LinkedInInvitationType.InviteById:
                    if (string.IsNullOrEmpty(options.RecipientId))
                        throw new LinkedInMissingParameterException("Invitation's recipient ID cannot be null or empty", "RecipientId");
                    if (string.IsNullOrEmpty(options.AuthorizationName))
                        throw new LinkedInMissingParameterException("Invitation's authorization name cannot be null or empty", "AuthorizationName");
                    if (string.IsNullOrEmpty(options.AuthorizationValue))
                        throw new LinkedInMissingParameterException("Invitation's authorization value cannot be null or empty", "AuthorizationValue");
                    break;
                case LinkedInInvitationType.InviteByEmail:
                    if (string.IsNullOrEmpty(options.RecipientEmail))
                        throw new LinkedInMissingParameterException("Invitation's recipient email cannot be null or empty", "RecipientEmail");
                    if (string.IsNullOrEmpty(options.RecipientFirstName))
                        throw new LinkedInMissingParameterException("Invitation's recipient first name cannot be null or empty", "RecipientFirstName");
                    if (string.IsNullOrEmpty(options.RecipientLastName))
                        throw new LinkedInMissingParameterException("Invitation's recipient last name cannot be null or empty", "RecipientLastName");
                    break;
            }
            return RequestRunner.SendInvitation(options);
        }
        #endregion

        /// <summary>
        /// Gets the last request string, not including access token
        /// </summary>
        public string LastRequest
        {
            get { return Singleton.Instance.LastRequest; }
        }
    }
}
