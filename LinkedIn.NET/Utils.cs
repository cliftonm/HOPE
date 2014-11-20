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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using LinkedIn.NET.Groups;
using LinkedIn.NET.Members;
using LinkedIn.NET.Updates;
using LinkedIn.NET.Search;

namespace LinkedIn.NET
{
    internal static class Utils
    {
        #region Authentication URLs
        internal const string ACCESS_TOKEN_URL =
            "https://www.linkedin.com/uas/oauth2/accessToken?grant_type=authorization_code&code=";
        internal const string AUTHORIZATION_URL =
            "https://www.linkedin.com/uas/oauth2/authorization?response_type=code&client_id=";
        #endregion

        #region Updates URLs
        internal const string UPDATE_STATUS_URL = "https://api.linkedin.com/v1/people/~/shares?";
        internal const string GET_UPDATES_URL = "https://api.linkedin.com/v1/people/~/network/updates?";
        internal const string GET_UPDATES_BY_ID_URL = "https://api.linkedin.com/v1/people/id=$USER_ID$/network/updates?";
        internal const string GET_UPDATES_BY_URL_URL =
            "https://api.linkedin.com/v1/people/url=$USER_URL$/network/updates?";
        internal const string UPDATE_LIKES_URL =
            "https://api.linkedin.com/v1/people/~/network/updates/key={NETWORK UPDATE KEY}/likes";
        internal const string UPDATE_IS_LIKED_URL =
            "https://api.linkedin.com/v1/people/~/network/updates/key={NETWORK UPDATE KEY}/is-liked";
        internal const string UPDATE_COMMENTS_URL =
            "https://api.linkedin.com/v1/people/~/network/updates/key={NETWORK UPDATE KEY}/update-comments";
        #endregion

        #region Profile URLs
        internal const string PROFILE_URL = "https://api.linkedin.com/v1/people/";
        internal const string PROFILE_SELF_URL = "https://api.linkedin.com/v1/people/~";
        internal const string PROFILE_BY_ID_URL = "https://api.linkedin.com/v1/people/id=";
        internal const string PROFILE_BY_URL_URL = "https://api.linkedin.com/v1/people/url=";
        internal const string PROFILE_MULTIPLE_URL = "https://api.linkedin.com/v1/people::";
        #endregion

        #region Groups URLs
        internal const string GROUPS_SUGGESTIONS_URL = "https://api.linkedin.com/v1/people/~/suggestions/groups";
        internal const string GROUPS_MEMBERSHIP_URL = "https://api.linkedin.com/v1/people/~/group-memberships:(group";
        internal const string GROUP_POSTS_URL = "https://api.linkedin.com/v1/groups/{GROUP_ID}/posts";
        internal const string GROUP_MEMBER_POSTS_URL =
            "https://api.linkedin.com/v1/people/~/group-memberships/{GROUP_ID}/posts";
        internal const string POSTS_COMMENTS_URL = "https://api.linkedin.com/v1/posts/{POST_ID}/comments";
        internal const string GROUP_JOIN_URL = "https://api.linkedin.com/v1/people/~/group-memberships/{GROUP_ID}";
        internal const string POSTS_URL = "https://api.linkedin.com/v1/posts/{POST_ID}";
        internal const string POSTS_LIKE_URL = "https://api.linkedin.com/v1/posts/{POST_ID}/relation-to-viewer/is-liked";
        internal const string POSTS_FOLLOW_URL =
            "https://api.linkedin.com/v1/posts/{POST_ID}/relation-to-viewer/is-following";
        internal const string POSTS_FLAG_URL = "https://api.linkedin.com/v1/posts/{POST_ID}/category/code";
        internal const string COMMENTS_URL = "https://api.linkedin.com/v1/comments/{COMMENT_ID}";
        #endregion

        #region Messages URLs
        internal const string SEND_MESSAGE_URL = "https://api.linkedin.com/v1/people/~/mailbox";
        #endregion

        #region Search URLs
        internal const string PEOPLE_SEARCH_URL = "https://api.linkedin.com/v1/people-search";
        #endregion

        internal const string CALLBACK = "https://linkin/done";

        private enum CompanyUpdateType
        {
            Invalid,
            Profile,
            Status, Job,
            Person
        }

        internal static readonly Dictionary<LinkedInCountries, string> CountryCodes = new Dictionary<LinkedInCountries, string>
        {
            {LinkedInCountries.Andorra, "ad"},
            {LinkedInCountries.UnitedArabEmirates, "ae"},
            {LinkedInCountries.Afghanistan, "af"},
            {LinkedInCountries.AntiguaAndBarbuda, "ag"},
            {LinkedInCountries.Anguilla, "ai"},
            {LinkedInCountries.Albania, "al"},
            {LinkedInCountries.Armenia, "am"},
            {LinkedInCountries.Angola, "ao"},
            {LinkedInCountries.Antarctica, "aq"},
            {LinkedInCountries.Argentina, "ar"},
            {LinkedInCountries.AmericanSamoa, "as"},
            {LinkedInCountries.Austria, "at"},
            {LinkedInCountries.Australia, "au"},
            {LinkedInCountries.Aruba, "aw"},
            {LinkedInCountries.AlandIslands, "ax"},
            {LinkedInCountries.Azerbaijan, "az"},
            {LinkedInCountries.BosniaAndHerzegovina, "ba"},
            {LinkedInCountries.Barbados, "bb"},
            {LinkedInCountries.Bangladesh, "bd"},
            {LinkedInCountries.Belgium, "be"},
            {LinkedInCountries.BurkinaFaso, "bf"},
            {LinkedInCountries.Bulgaria, "bg"},
            {LinkedInCountries.Bahrain, "bh"},
            {LinkedInCountries.Burundi, "bi"},
            {LinkedInCountries.Benin, "bj"},
            {LinkedInCountries.SaintBarthelemy, "bl"},
            {LinkedInCountries.Bermuda, "bm"},
            {LinkedInCountries.BruneiDarussalam, "bn"},
            {LinkedInCountries.BoliviaPlurinationalStateOf, "bo"},
            {LinkedInCountries.BonaireSintEustatiusAndSaba, "bq"},
            {LinkedInCountries.Brazil, "br"},
            {LinkedInCountries.Bahamas, "bs"},
            {LinkedInCountries.Bhutan, "bt"},
            {LinkedInCountries.BouvetIsland, "bv"},
            {LinkedInCountries.Botswana, "bw"},
            {LinkedInCountries.Belarus, "by"},
            {LinkedInCountries.Belize, "bz"},
            {LinkedInCountries.Canada, "ca"},
            {LinkedInCountries.CocosIslands, "cc"},
            {LinkedInCountries.CongoTheDemocraticRepublic, "cd"},
            {LinkedInCountries.CentralAfricanRepublic, "cf"},
            {LinkedInCountries.Congo, "cg"},
            {LinkedInCountries.Switzerland, "ch"},
            {LinkedInCountries.CoteDivoire, "ci"},
            {LinkedInCountries.CookIslands, "ck"},
            {LinkedInCountries.Chile, "cl"},
            {LinkedInCountries.Cameroon, "cm"},
            {LinkedInCountries.China, "cn"},
            {LinkedInCountries.Colombia, "co"},
            {LinkedInCountries.CostaRica, "cr"},
            {LinkedInCountries.Cuba, "cu"},
            {LinkedInCountries.CapeVerde, "cv"},
            {LinkedInCountries.Curacao, "cw"},
            {LinkedInCountries.ChristmasIsland, "cx"},
            {LinkedInCountries.Cyprus, "cy"},
            {LinkedInCountries.CzechRepublic, "cz"},
            {LinkedInCountries.Germany, "de"},
            {LinkedInCountries.Djibouti, "dj"},
            {LinkedInCountries.Denmark, "dk"},
            {LinkedInCountries.Dominica, "dm"},
            {LinkedInCountries.DominicanRepublic, "do"},
            {LinkedInCountries.Algeria, "dz"},
            {LinkedInCountries.Ecuador, "ec"},
            {LinkedInCountries.Estonia, "ee"},
            {LinkedInCountries.Egypt, "eg"},
            {LinkedInCountries.WesternSahara, "eh"},
            {LinkedInCountries.Eritrea, "er"},
            {LinkedInCountries.Spain, "es"},
            {LinkedInCountries.Ethiopia, "et"},
            {LinkedInCountries.Finland, "fi"},
            {LinkedInCountries.Fiji, "fj"},
            {LinkedInCountries.FalklandIslands, "fk"},
            {LinkedInCountries.MicronesiaFederatedStatesOf, "fm"},
            {LinkedInCountries.FaroeIslands, "fo"},
            {LinkedInCountries.France, "fr"},
            {LinkedInCountries.Gabon, "ga"},
            {LinkedInCountries.UnitedKingdom, "gb"},
            {LinkedInCountries.Grenada, "gd"},
            {LinkedInCountries.Georgia, "ge"},
            {LinkedInCountries.FrenchGuiana, "gf"},
            {LinkedInCountries.Guernsey, "gg"},
            {LinkedInCountries.Ghana, "gh"},
            {LinkedInCountries.Gibraltar, "gi"},
            {LinkedInCountries.Greenland, "gl"},
            {LinkedInCountries.Gambia, "gm"},
            {LinkedInCountries.Guinea, "gn"},
            {LinkedInCountries.Guadeloupe, "gp"},
            {LinkedInCountries.EquatorialGuinea, "gq"},
            {LinkedInCountries.Greece, "gr"},
            {LinkedInCountries.SouthGeorgiaAndTheSouthSandwichIslands, "gs"},
            {LinkedInCountries.Guatemala, "gt"},
            {LinkedInCountries.Guam, "gu"},
            {LinkedInCountries.GuineaBissau, "gw"},
            {LinkedInCountries.Guyana, "gy"},
            {LinkedInCountries.HongKong, "hk"},
            {LinkedInCountries.HeardIslandAndMcDonaldIslands, "hm"},
            {LinkedInCountries.Honduras, "hn"},
            {LinkedInCountries.Croatia, "hr"},
            {LinkedInCountries.Haiti, "ht"},
            {LinkedInCountries.Hungary, "hu"},
            {LinkedInCountries.Indonesia, "id"},
            {LinkedInCountries.Ireland, "ie"},
            {LinkedInCountries.Israel, "il"},
            {LinkedInCountries.IsleOfMan, "im"},
            {LinkedInCountries.India, "in"},
            {LinkedInCountries.BritishIndianOceanTerritory, "io"},
            {LinkedInCountries.Iraq, "iq"},
            {LinkedInCountries.IranIslamicRepublicOf, "ir"},
            {LinkedInCountries.Iceland, "is"},
            {LinkedInCountries.Italy, "it"},
            {LinkedInCountries.Jersey, "je"},
            {LinkedInCountries.Jamaica, "jm"},
            {LinkedInCountries.Jordan, "jo"},
            {LinkedInCountries.Japan, "jp"},
            {LinkedInCountries.Kenya, "ke"},
            {LinkedInCountries.Kyrgyzstan, "kg"},
            {LinkedInCountries.Cambodia, "kh"},
            {LinkedInCountries.Kiribati, "ki"},
            {LinkedInCountries.Comoros, "km"},
            {LinkedInCountries.SaintKittsAndNevis, "kn"},
            {LinkedInCountries.KoreaDemocraticPeopleRepublicOf, "kp"},
            {LinkedInCountries.KoreaRepublicOf, "kr"},
            {LinkedInCountries.Kuwait, "kw"},
            {LinkedInCountries.CaymanIslands, "ky"},
            {LinkedInCountries.Kazakhstan, "kz"},
            {LinkedInCountries.LaoPeopleDemocraticRepublic, "la"},
            {LinkedInCountries.Lebanon, "lb"},
            {LinkedInCountries.SaintLucia, "lc"},
            {LinkedInCountries.Liechtenstein, "li"},
            {LinkedInCountries.SriLanka, "lk"},
            {LinkedInCountries.Liberia, "lr"},
            {LinkedInCountries.Lesotho, "ls"},
            {LinkedInCountries.Lithuania, "lt"},
            {LinkedInCountries.Luxembourg, "lu"},
            {LinkedInCountries.Latvia, "lv"},
            {LinkedInCountries.Libya, "ly"},
            {LinkedInCountries.Morocco, "ma"},
            {LinkedInCountries.Monaco, "mc"},
            {LinkedInCountries.MoldovaRepublicOf, "md"},
            {LinkedInCountries.Montenegro, "me"},
            {LinkedInCountries.SaintMartinFrenchPart, "mf"},
            {LinkedInCountries.Madagascar, "mg"},
            {LinkedInCountries.MarshallIslands, "mh"},
            {LinkedInCountries.MacedoniaTheFormerYugoslavRepublicOf, "mk"},
            {LinkedInCountries.Mali, "ml"},
            {LinkedInCountries.Myanmar, "mm"},
            {LinkedInCountries.Mongolia, "mn"},
            {LinkedInCountries.Macao, "mo"},
            {LinkedInCountries.NorthernMarianaIslands, "mp"},
            {LinkedInCountries.Martinique, "mq"},
            {LinkedInCountries.Mauritania, "mr"},
            {LinkedInCountries.Montserrat, "ms"},
            {LinkedInCountries.Malta, "mt"},
            {LinkedInCountries.Mauritius, "mu"},
            {LinkedInCountries.Maldives, "mv"},
            {LinkedInCountries.Malawi, "mw"},
            {LinkedInCountries.Mexico, "mx"},
            {LinkedInCountries.Malaysia, "my"},
            {LinkedInCountries.Mozambique, "mz"},
            {LinkedInCountries.Namibia, "na"},
            {LinkedInCountries.NewCaledonia, "nc"},
            {LinkedInCountries.Niger, "ne"},
            {LinkedInCountries.NorfolkIsland, "nf"},
            {LinkedInCountries.Nigeria, "ng"},
            {LinkedInCountries.Nicaragua, "ni"},
            {LinkedInCountries.Netherlands, "nl"},
            {LinkedInCountries.Norway, "no"},
            {LinkedInCountries.Nepal, "np"},
            {LinkedInCountries.Nauru, "nr"},
            {LinkedInCountries.Niue, "nu"},
            {LinkedInCountries.NewZealand, "nz"},
            {LinkedInCountries.Oman, "om"},
            {LinkedInCountries.Panama, "pa"},
            {LinkedInCountries.Peru, "pe"},
            {LinkedInCountries.FrenchPolynesia, "pf"},
            {LinkedInCountries.PapuaNewGuinea, "pg"},
            {LinkedInCountries.Philippines, "ph"},
            {LinkedInCountries.Pakistan, "pk"},
            {LinkedInCountries.Poland, "pl"},
            {LinkedInCountries.SaintPierreAndMiquelon, "pm"},
            {LinkedInCountries.Pitcairn, "pn"},
            {LinkedInCountries.PuertoRico, "pr"},
            {LinkedInCountries.PalestineStateOf, "ps"},
            {LinkedInCountries.Portugal, "pt"},
            {LinkedInCountries.Palau, "pw"},
            {LinkedInCountries.Paraguay, "py"},
            {LinkedInCountries.Qatar, "qa"},
            {LinkedInCountries.Reunion, "re"},
            {LinkedInCountries.Romania, "ro"},
            {LinkedInCountries.Serbia, "rs"},
            {LinkedInCountries.RussianFederation, "ru"},
            {LinkedInCountries.Rwanda, "rw"},
            {LinkedInCountries.SaudiArabia, "sa"},
            {LinkedInCountries.SolomonIslands, "request"},
            {LinkedInCountries.Seychelles, "sc"},
            {LinkedInCountries.Sudan, "sd"},
            {LinkedInCountries.Sweden, "se"},
            {LinkedInCountries.Singapore, "sg"},
            {LinkedInCountries.SaintHelenaAscensionAndTristanDaCunha, "sh"},
            {LinkedInCountries.Slovenia, "si"},
            {LinkedInCountries.SvalbardAndJanMayen, "sj"},
            {LinkedInCountries.Slovakia, "sk"},
            {LinkedInCountries.SierraLeone, "sl"},
            {LinkedInCountries.SanMarino, "sm"},
            {LinkedInCountries.Senegal, "sn"},
            {LinkedInCountries.Somalia, "so"},
            {LinkedInCountries.Suriname, "sr"},
            {LinkedInCountries.SouthSudan, "ss"},
            {LinkedInCountries.SaoTomeAndPrincipe, "st"},
            {LinkedInCountries.ElSalvador, "sv"},
            {LinkedInCountries.SintMaartenDutchPart, "sx"},
            {LinkedInCountries.SyrianArabRepublic, "sy"},
            {LinkedInCountries.Swaziland, "sz"},
            {LinkedInCountries.TurksAndCaicosIslands, "tc"},
            {LinkedInCountries.Chad, "td"},
            {LinkedInCountries.FrenchSouthernTerritories, "tf"},
            {LinkedInCountries.Togo, "tg"},
            {LinkedInCountries.Thailand, "th"},
            {LinkedInCountries.Tajikistan, "tj"},
            {LinkedInCountries.Tokelau, "tk"},
            {LinkedInCountries.TimorLeste, "tl"},
            {LinkedInCountries.Turkmenistan, "tm"},
            {LinkedInCountries.Tunisia, "tn"},
            {LinkedInCountries.Tonga, "to"},
            {LinkedInCountries.Turkey, "tr"},
            {LinkedInCountries.TrinidadAndTobago, "tt"},
            {LinkedInCountries.Tuvalu, "tv"},
            {LinkedInCountries.TaiwanProvinceOfChina, "tw"},
            {LinkedInCountries.TanzaniaUnitedRepublicOf, "tz"},
            {LinkedInCountries.Ukraine, "ua"},
            {LinkedInCountries.Uganda, "ug"},
            {LinkedInCountries.UnitedStatesMinorOutlyingIslands, "um"},
            {LinkedInCountries.UnitedStates, "us"},
            {LinkedInCountries.Uruguay, "uy"},
            {LinkedInCountries.Uzbekistan, "uz"},
            {LinkedInCountries.VaticanCityState, "va"},
            {LinkedInCountries.SaintVincentAndTheGrenadines, "vc"},
            {LinkedInCountries.VenezuelaBolivarianRepublicOf, "ve"},
            {LinkedInCountries.VirginIslandsBritish, "vg"},
            {LinkedInCountries.VirginIslandsUS, "vi"},
            {LinkedInCountries.VietNam, "vn"},
            {LinkedInCountries.Vanuatu, "vu"},
            {LinkedInCountries.WallisAndFutuna, "wf"},
            {LinkedInCountries.Samoa, "ws"},
            {LinkedInCountries.Yemen, "ye"},
            {LinkedInCountries.Mayotte, "yt"},
            {LinkedInCountries.SouthAfrica, "za"},
            {LinkedInCountries.Zambia, "zm"},
            {LinkedInCountries.Zimbabwe, "zw"}
        };

        internal static readonly Dictionary<string, string> IndustryCodes = new Dictionary<string, string>
            {
                {"47", "corp fin|Accounting"},
                {"94", "man tech tran|Airlines/Aviation"},
                {"120", "leg org|Alternative Dispute Resolution"},
                {"125", "hlth|Alternative Medicine"},
                {"127", "art med|Animation"},
                {"19", "good|Apparel & Fashion"},
                {"50", "cons|Architecture & Planning"},
                {"111", "art med rec|Arts and Crafts"},
                {"53", "man|Automotive"},
                {"52", "gov man|Aviation & Aerospace"},
                {"41", "fin|Banking"},
                {"12", "gov hlth tech|Biotechnology"},
                {"36", "med rec|Broadcast Media"},
                {"49", "cons|Building Materials"},
                {"138", "corp man|Business Supplies and Equipment"},
                {"129", "fin|Capital Markets"},
                {"54", "man|Chemicals"},
                {"90", "org serv|Civic & Social Organization"},
                {"51", "cons gov|Civil Engineering"},
                {"128", "cons corp fin|Commercial Real Estate"},
                {"118", "tech|Computer & Network Security"},
                {"109", "med rec|Computer Games"},
                {"3", "tech|Computer Hardware"},
                {"5", "tech|Computer Networking"},
                {"4", "tech|Computer Software"},
                {"48", "cons|Construction"},
                {"24", "good man|Consumer Electronics"},
                {"25", "good man|Consumer Goods"},
                {"91", "org serv|Consumer Services"},
                {"18", "good|Cosmetics"},
                {"65", "agr|Dairy"},
                {"1", "gov tech|Defense & Space"},
                {"99", "art med|Design"},
                {"69", "edu|Education Management"},
                {"132", "edu org|E-Learning"},
                {"112", "good man|Electrical/Electronic Manufacturing"},
                {"28", "med rec|Entertainment"},
                {"86", "org serv|Environmental Services"},
                {"110", "corp rec serv|Events Services"},
                {"76", "gov|Executive Office"},
                {"122", "corp serv|Facilities Services"},
                {"63", "agr|Farming"},
                {"43", "fin|Financial Services"},
                {"38", "art med rec|Fine Art"},
                {"66", "agr|Fishery"},
                {"34", "rec serv|Food & Beverages"},
                {"23", "good man serv|Food Production"},
                {"101", "org|Fund-Raising"},
                {"26", "good man|Furniture"},
                {"29", "rec|Gambling & Casinos"},
                {"145", "cons man|Glass, Ceramics & Concrete"},
                {"75", "gov|Government Administration"},
                {"148", "gov|Government Relations"},
                {"140", "art med|Graphic Design"},
                {"124", "hlth rec|Health, Wellness and Fitness"},
                {"68", "edu|Higher Education"},
                {"14", "hlth|Hospital & Health Care"},
                {"31", "rec serv tran|Hospitality"},
                {"137", "corp|Human Resources"},
                {"134", "corp good tran|Import and Export"},
                {"88", "org serv|Individual & Family Services"},
                {"147", "cons man|Industrial Automation"},
                {"84", "med serv|Information Services"},
                {"96", "tech|Information Technology and Services"},
                {"42", "fin|Insurance"},
                {"74", "gov|International Affairs"},
                {"141", "gov org tran|International Trade and Development"},
                {"6", "tech|Internet"},
                {"45", "fin|Investment Banking"},
                {"46", "fin|Investment Management"},
                {"73", "gov leg|Judiciary"},
                {"77", "gov leg|Law Enforcement"},
                {"9", "leg|Law Practice"},
                {"10", "leg|Legal Services"},
                {"72", "gov leg|Legislative Office"},
                {"30", "rec serv tran|Leisure, Travel & Tourism"},
                {"85", "med rec serv|Libraries"},
                {"116", "corp tran|Logistics and Supply Chain"},
                {"143", "good|Luxury Goods & Jewelry"},
                {"55", "man|Machinery"},
                {"11", "corp|Management Consulting"},
                {"95", "tran|Maritime"},
                {"97", "corp|Market Research"},
                {"80", "corp med|Marketing and Advertising"},
                {"135", "cons gov man|Mechanical or Industrial Engineering"},
                {"126", "med rec|Media Production"},
                {"17", "hlth|Medical Devices"},
                {"13", "hlth|Medical Practice"},
                {"139", "hlth|Mental Health Care"},
                {"71", "gov|Military"},
                {"56", "man|Mining & Metals"},
                {"35", "art med rec|Motion Pictures and Film"},
                {"37", "art med rec|Museums and Institutions"},
                {"115", "art rec|Music"},
                {"114", "gov man tech|Nanotechnology"},
                {"81", "med rec|Newspapers"},
                {"100", "org|Non-Profit Organization Management"},
                {"57", "man|Oil & Energy"},
                {"113", "med|Online Media"},
                {"123", "corp|Outsourcing/Offshoring"},
                {"87", "serv tran|Package/Freight Delivery"},
                {"146", "good man|Packaging and Containers"},
                {"61", "man|Paper & Forest Products"},
                {"39", "art med rec|Performing Arts"},
                {"15", "hlth tech|Pharmaceuticals"},
                {"131", "org|Philanthropy"},
                {"136", "art med rec|Photography"},
                {"117", "man|Plastics"},
                {"107", "gov org|Political Organization"},
                {"67", "edu|Primary/Secondary Education"},
                {"83", "med rec|Printing"},
                {"105", "corp|Professional Training & Coaching"},
                {"102", "corp org|Program Development"},
                {"79", "gov|Public Policy"},
                {"98", "corp|Public Relations and Communications"},
                {"78", "gov|Public Safety"},
                {"82", "med rec|Publishing"},
                {"62", "man|Railroad Manufacture"},
                {"64", "agr|Ranching"},
                {"44", "cons fin good|Real Estate"},
                {"40", "rec serv|Recreational Facilities and Services"},
                {"89", "org serv|Religious Institutions"},
                {"144", "gov man org|Renewables & Environment"},
                {"70", "edu gov|Research"},
                {"32", "rec serv|Restaurants"},
                {"27", "good man|Retail"},
                {"121", "corp org serv|Security and Investigations"},
                {"7", "tech|Semiconductors"},
                {"58", "man|Shipbuilding"},
                {"20", "good rec|Sporting Goods"},
                {"33", "rec|Sports"},
                {"104", "corp|Staffing and Recruiting"},
                {"22", "good|Supermarkets"},
                {"8", "gov tech|Telecommunications"},
                {"60", "man|Textiles"},
                {"130", "gov org|Think Tanks"},
                {"21", "good|Tobacco"},
                {"108", "corp gov serv|Translation and Localization"},
                {"92", "tran|Transportation/Trucking/Railroad"},
                {"59", "man|Utilities"},
                {"106", "fin tech|Venture Capital & Private Equity"},
                {"16", "hlth|Veterinary"},
                {"93", "tran|Warehousing"},
                {"133", "good|Wholesale"},
                {"142", "good man rec|Wine and Spirits"},
                {"119", "tech|Wireless"},
                {"103", "art med rec|Writing and Editing"}
            };

        private static DateTime _unixStartDate = new DateTime(1970, 1, 1);

        internal static LinkedInPerson GetCurrentUser()
        {
            var sb = new StringBuilder(PROFILE_SELF_URL);
            sb.Append(":(id,first-name,last-name,headline)");
            sb.Append("?oauth2_access_token=");
            sb.Append(Singleton.Instance.AccessToken);
            var requestString = MakeRequest(sb.ToString(), "GET");
            var xdoc = XDocument.Parse(requestString);
            return xdoc.Root != null ? BuildPerson(new LinkedInPerson(), xdoc.Root) : null;
        }

        /// <summary>
        /// Builds and process web request
        /// </summary>
        /// <param name="url">Request url</param>
        /// <param name="method">Request method</param>
        /// <param name="postData">Post data for POST/PUT request - optional</param>
        /// <returns>The response data</returns>
        internal static string MakeRequest(string url, string method, string postData = null)
        {
            var statusCode = HttpStatusCode.OK;
            return MakeRequest(url, method, ref statusCode, postData);
        }

        /// <summary>
        /// Builds and process web request
        /// </summary>
        /// <param name="url">Request url</param>
        /// <param name="method">Request method</param>
        /// <param name="statusCode">On return, represents response status code</param>
        /// <param name="postData">Post data for POST/PUT request - optional</param>
        /// <returns>The response data</returns>
        internal static string MakeRequest(string url, string method, ref HttpStatusCode statusCode, string postData = null)
        {
            var o = new object();
            lock (o)
            {
                Singleton.Instance.LastRequest = url;
                var webRequest = WebRequest.Create(new Uri(url)) as HttpWebRequest;
                if (webRequest == null) return "";
                webRequest.Method = method;
                webRequest.Credentials = CredentialCache.DefaultCredentials;
                webRequest.AllowWriteStreamBuffering = true;
                webRequest.PreAuthenticate = true;
                webRequest.ServicePoint.Expect100Continue = false;
                ServicePointManager.SecurityProtocol = Singleton.Instance.SecurityProtocol;
                if (postData == null) return webResponseGet(webRequest, ref statusCode);
                var fileToSend = Encoding.UTF8.GetBytes(postData);
                webRequest.ContentLength = fileToSend.Length;

                using (var reqStream = webRequest.GetRequestStream())
                {
                    reqStream.Write(fileToSend, 0, fileToSend.Length);
                }
                return webResponseGet(webRequest, ref statusCode);
            }
        }

        /// <summary>
        /// Processes the web response.
        /// </summary>
        /// <param name="webRequest">The request object.</param>
        /// <param name="statusCode">On return, represents response status code</param>
        /// <returns>The response data.</returns>
        private static string webResponseGet(HttpWebRequest webRequest, ref HttpStatusCode statusCode)
        {
            var responseData = "";
            var response = (HttpWebResponse)webRequest.GetResponse();
            statusCode = response.StatusCode;
            using (var stream = response.GetResponseStream())
            {
                if (stream == null) return responseData;
                using (var responseReader = new StreamReader(stream))
                {
                    responseData = responseReader.ReadToEnd();
                }
            }
            return responseData;
        }

        internal static NameValueCollection ParseResponse(string response)
        {
            var nvc = new NameValueCollection();
            if (response.StartsWith("?")) response = response.Substring(1);
            var arr1 = response.Split('&');
            foreach (var arr2 in arr1.Select(s => s.Split('=')).Where(arr2 => arr2.Length == 2))
            {
                nvc.Add(arr2[0].Trim(), arr2[1].Trim());
            }
            return nvc;
        }

        internal static string NormalizeUrl(string url)
        {
            return url.Replace(":", "%3A").Replace("/", "%2F");
        }

        internal static string EscapeXml(string source)
        {
            string target = source;
            target = target.Replace("&", "&amp;");
            target = target.Replace("\"", "&quot;");
            target = target.Replace("<", "&lt;");
            target = target.Replace(">", "&gt;");
            return target;
        }

        internal static DateTime GetRealDateTime(double milliseconds)
        {
            return _unixStartDate.AddMilliseconds(milliseconds).ToLocalTime();
        }

        internal static LinkedInFacet BuildFacet(XElement xp)
        {
            var liFacet = new LinkedInFacet();

            //code
            var xe = xp.Element("code");
            if (xe != null)
                liFacet.Code = xe.Value.Trim();
            //name
            xe = xp.Element("name");
            if (xe != null)
                liFacet.Name = xe.Value.Trim();
            //buckets
            xe = xp.Element("buckets");
            if (xe != null)
                liFacet.AddBuckets(xe.Elements("bucket").Select(buildBucket));

            return liFacet;
        }

        internal static LinkedInCompanyBase BuildCompanyBase(XElement xp)
        {
            var liCompanyBase = new LinkedInCompanyBase();

            //id
            var xe = xp.Element("id");
            if (xe != null)
                liCompanyBase.Id = xe.Value.Trim();
            //name
            xe = xp.Element("name");
            if (xe != null)
                liCompanyBase.Name = xe.Value.Trim();

            return liCompanyBase;
        }

        internal static LinkedInDate BuildDate(XElement xe)
        {
            var liDate = new LinkedInDate();

            var xn = xe.Element("year");
            if (xn != null)
                liDate.Year = Convert.ToInt32(xn.Value.Trim());
            xn = xe.Element("month");
            if (xn != null)
                liDate.Month = Convert.ToInt32(xn.Value.Trim());
            xn = xe.Element("day");
            if (xn != null)
                liDate.Day = Convert.ToInt32(xn.Value.Trim());

            return liDate;
        }

        internal static LinkedInPerson BuildPerson(LinkedInPerson liPerson, XElement xp)
        {
            //id
            var xe = xp.Element("id");
            if (xe != null)
                liPerson.Id = xe.Value.Trim();
            //first-name
            xe = xp.Element("first-name");
            if (xe != null)
                liPerson.FirstName = xe.Value.Trim();
            //last-name
            xe = xp.Element("last-name");
            if (xe != null)
                liPerson.LastName = xe.Value.Trim();
            //headline
            xe = xp.Element("headline");
            if (xe != null)
                liPerson.Headline = xe.Value.Trim();
            //picture-url
            xe = xp.Element("picture-url");
            if (xe != null)
                liPerson.PictureUrl = xe.Value.Trim();
            //api-standard-profile-request
            xe = xp.Element("api-standard-profile-request");
            if (xe != null)
                liPerson.ApiStandardProfileRquest = buildApiStandardProfileRequest(xe);
            //site-standard-profile-request
            xe = xp.Element("site-standard-profile-request");
            if (xe != null)
            {
                var xn = xe.Element("url");
                if (xn != null)
                    liPerson.SiteStandardProfileRequest = xn.Value.Trim();
            }
            return liPerson;
        }

        internal static LinkedInLike BuildLike(XElement xp)
        {
            var liLike = new LinkedInLike();

            var xe = xp.Element("person");
            if (xe != null)
            {
                liLike.Person = BuildPerson(new LinkedInPerson(), xe);
            }

            return liLike;
        }

        internal static LinkedInComment BuildComment(XElement xp)
        {
            var liComment = new LinkedInComment();

            var xe = xp.Element("id");
            if (xe != null)
            {
                liComment.Id = xe.Value.Trim();
            }
            xe = xp.Element("sequence-number");
            if (xe != null)
            {
                liComment.SequenceNumber = Convert.ToInt32(xe.Value.Trim());
            }
            xe = xp.Element("comment");
            if (xe != null)
            {
                liComment.Comment = xe.Value.Trim();
            }
            xe = xp.Element("timestamp");
            if (xe != null)
            {
                liComment.CommentDate = GetRealDateTime(Convert.ToDouble(xe.Value.Trim()));
            }
            xe = xp.Element("person");
            if (xe != null)
            {
                liComment.Person = BuildPerson(new LinkedInPerson(), xe);
            }

            return liComment;
        }

        internal static LinkedInShare BuildShare(XElement xe)
        {
            var liShare = new LinkedInShare();

            var xn = xe.Element("id");
            if (xn != null)
            {
                liShare.Id = xn.Value.Trim();
            }
            xn = xe.Element("timestamp");
            if (xn != null)
            {
                liShare.ShareDate = GetRealDateTime(Convert.ToDouble(xn.Value.Trim()));
            }
            xn = xe.Element("visibility");
            if (xn != null)
            {
                xn = xn.Element("code");
                if (xn != null)
                {
                    liShare.VisibilityCode = xn.Value.Trim() == "anyone"
                                                               ? LinkedInShareVisibilityCode.Anyone
                                                               : LinkedInShareVisibilityCode.ConnectionsOnly;
                }
            }
            xn = xe.Element("comment");
            if (xn != null)
            {
                liShare.Comment = xn.Value.Trim();
            }
            xn = xe.Element("source");
            if (xn != null)
            {
                var xs = xn.Element("service-provider");
                if (xs != null)
                {
                    xs = xs.Element("name");
                    if (xs != null)
                    {
                        liShare.SourceServiceProvider = xs.Value.Trim();
                    }
                }
                xs = xn.Element("application");
                if (xs != null)
                {
                    xs = xs.Element("name");
                    if (xs != null)
                    {
                        liShare.SourceApplication = xs.Value.Trim();
                    }
                }
            }
            xn = xe.Element("author");
            if (xn != null)
            {
                liShare.Author = BuildPerson(new LinkedInPerson(), xn);
            }
            xn = xe.Element("content");
            if (xn != null)
            {
                liShare.Content = new LinkedInShareContent();
                var xc = xn.Element("submitted-url");
                if (xc != null)
                    liShare.Content.SubmittedUrl = xc.Value.Trim();
                xc = xn.Element("resolved-url");
                if (xc != null)
                    liShare.Content.ResolvedUrl = xc.Value.Trim();
                xc = xn.Element("shortened-url");
                if (xc != null)
                    liShare.Content.ShorteneddUrl = xc.Value.Trim();
                xc = xn.Element("title");
                if (xc != null)
                    liShare.Content.Title = xc.Value.Trim();
                xc = xn.Element("description");
                if (xc != null)
                    liShare.Content.Description = xc.Value.Trim();
                xc = xn.Element("submitted-image-url");
                if (xc != null)
                    liShare.Content.SubmittedImageUrl = xc.Value.Trim();
                xc = xn.Element("thumbnail-url");
                if (xc != null)
                    liShare.Content.ThumbnailUrl = xc.Value.Trim();
                xc = xn.Element("eyebrow-url");
                if (xc != null)
                    liShare.Content.EyebrowUrl = xc.Value.Trim();
            }
            return liShare;
        }

        internal static LinkedInActionUpdate BuildUpdateAction(XElement xa)
        {
            var liUpdateAction = new LinkedInActionUpdate();

            //action code
            var xe = xa.Element("action");
            if (xe != null)
            {
                xe = xe.Element("code");
                if (xe != null)
                {
                    liUpdateAction.ActionCode = xe.Value.Trim();
                }
            }

            return liUpdateAction;
        }

        internal static LinkedInMemberGroup BuildMemberGroup(XElement xp)
        {
            var liGroup = new LinkedInMemberGroup();

            //id
            var xe = xp.Element("id");
            if (xe != null)
                liGroup.Id = xe.Value.Trim();
            //name
            xe = xp.Element("name");
            if (xe != null)
                liGroup.Name = xe.Value.Trim();
            //site-group-request
            xe = xp.Element("site-group-request");
            if (xe != null)
            {
                xe = xe.Element("url");
                if (xe != null)
                    liGroup.SiteGroupRequestUrl = xe.Value.Trim();
            }

            return liGroup;
        }

        internal static LinkedInActivity BuildActivity(XElement xp)
        {
            var liActivity = new LinkedInActivity();

            //body
            var xe = xp.Element("body");
            if (xe != null)
                liActivity.Body = xe.Value.Trim();
            //app-id
            xe = xp.Element("app-id");
            if (xe != null)
                liActivity.AppId = xe.Value.Trim();

            return liActivity;
        }

        internal static LinkedInPositionBase BuildPositionBase(XElement xp)
        {
            var liPos = new LinkedInPositionBase();

            //id
            var xe = xp.Element("id");
            if (xe != null)
                liPos.Id = xe.Value.Trim();
            //title
            xe = xp.Element("title");
            if (xe != null)
                liPos.Title = xe.Value.Trim();
            //company
            xe = xp.Element("company");
            if (xe != null)
                liPos.Company = buildCompany(xe);

            return liPos;
        }

        internal static LinkedInGroupSettings BuildGroupSettings(XElement xp)
        {
            var liSettings = new LinkedInGroupSettings();

            //show-group-logo-in-profile
            var xe = xp.Element("show-group-logo-in-profile");
            if (xe != null)
                liSettings.ShowGroupLogoInProfile = Convert.ToBoolean(xe.Value.Trim());
            //allow-messages-from-members
            xe = xp.Element("allow-messages-from-members");
            if (xe != null)
                liSettings.AllowMessagesFromMembers = Convert.ToBoolean(xe.Value.Trim());
            //email-announcements-from-managers
            xe = xp.Element("email-announcements-from-managers");
            if (xe != null)
                liSettings.EmailAnnouncementsFromManagers = Convert.ToBoolean(xe.Value.Trim());
            //email-for-every-new-post
            xe = xp.Element("email-for-every-new-post");
            if (xe != null)
                liSettings.EmailForEveryNewPost = Convert.ToBoolean(xe.Value.Trim());
            //email-digest-frequency
            xe = xp.Element("email-digest-frequency");
            if (xe != null)
            {
                var xc = xe.Element("code");
                if (xc != null)
                {
                    switch (xc.Value.Trim())
                    {
                        case "none":
                            liSettings.EmailDigestFrequency = LinkedInEmailDigestFrequency.None;
                            break;
                        case "daily":
                            liSettings.EmailDigestFrequency = LinkedInEmailDigestFrequency.Daily;
                            break;
                        case "weekly":
                            liSettings.EmailDigestFrequency = LinkedInEmailDigestFrequency.Weekly;
                            break;
                    }
                }
            }

            return liSettings;
        }

        internal static LinkedInGroup BuildGroup(XElement xp)
        {
            var liGroup = new LinkedInGroup();

            //id
            var xe = xp.Element("id");
            if (xe != null)
                liGroup.Id = xe.Value.Trim();
            //name
            xe = xp.Element("name");
            if (xe != null)
                liGroup.Name = xe.Value.Trim();
            //short-description
            xe = xp.Element("short-description");
            if (xe != null)
                liGroup.ShortDescription = xe.Value.Trim();
            //description
            xe = xp.Element("description");
            if (xe != null)
                liGroup.Description = xe.Value.Trim();
            //relation-to-viewer
            xe = xp.Element("relation-to-viewer");
            if (xe != null)
            {
                var xn = xe.Element("membership-state");
                if (xn != null)
                {
                    var xc = xn.Element("code");
                    if (xc != null)
                    {
                        switch (xc.Value.Trim())
                        {
                            case "blocked":
                                liGroup.MembershipState = LinkedInGroupRelationship.Blocked;
                                break;
                            case "non-member":
                                liGroup.MembershipState = LinkedInGroupRelationship.NonMember;
                                break;
                            case "awaiting-confirmation":
                                liGroup.MembershipState = LinkedInGroupRelationship.AwaitingConfirmation;
                                break;
                            case "awaiting-parent-group-confirmation":
                                liGroup.MembershipState = LinkedInGroupRelationship.AwaitingParentGroupConfirmation;
                                break;
                            case "member":
                                liGroup.MembershipState = LinkedInGroupRelationship.Member;
                                break;
                            case "moderator":
                                liGroup.MembershipState = LinkedInGroupRelationship.Moderator;
                                break;
                            case "manager":
                                liGroup.MembershipState = LinkedInGroupRelationship.Manager;
                                break;
                            case "owner":
                                liGroup.MembershipState = LinkedInGroupRelationship.Owner;
                                break;
                        }
                    }
                }
                var xactions = xe.Element("available-actions");
                if (xactions != null)
                {
                    foreach (
                        var xc in xactions.Elements("action").Select(xa => xa.Element("code")).Where(xc => xc != null))
                    {
                        switch (xc.Value.Trim())
                        {
                            case "add-post":
                                liGroup.AvailableAction[LinkedInGroupAction.AddPost] = true;
                                break;
                            case "leave":
                                liGroup.AvailableAction[LinkedInGroupAction.Leave] = true;
                                break;
                            case "view-posts":
                                liGroup.AvailableAction[LinkedInGroupAction.ViewPosts] = true;
                                break;
                        }
                    }
                }
            }
            //counts-by-category
            xe = xp.Element("counts-by-category");
            if (xe != null)
                liGroup.AddCountsByCategory(xe.Elements("count-for-category").Select(buildGroupCategoryCount));
            //is-open-to-non-members
            xe = xp.Element("is-open-to-non-members");
            if (xe != null)
                liGroup.IsOpenToNonMembers = Convert.ToBoolean(xe.Value.Trim());
            //category
            xe = xp.Element("category");
            if (xe != null)
            {
                var xc = xe.Element("code");
                if (xc != null)
                {
                    switch (xc.Value.Trim())
                    {
                        case "alumni":
                            liGroup.Category = LinkedInGroupCategory.Alumni;
                            break;
                        case "corporate":
                            liGroup.Category = LinkedInGroupCategory.Corporate;
                            break;
                        case "conference":
                            liGroup.Category = LinkedInGroupCategory.Conference;
                            break;
                        case "network":
                            liGroup.Category = LinkedInGroupCategory.Network;
                            break;
                        case "philanthropic":
                            liGroup.Category = LinkedInGroupCategory.Philantropic;
                            break;
                        case "professional":
                            liGroup.Category = LinkedInGroupCategory.Professional;
                            break;
                        case "other":
                            liGroup.Category = LinkedInGroupCategory.Other;
                            break;
                    }
                }
            }
            //website-url
            xe = xp.Element("website-url");
            if (xe != null)
                liGroup.WebSiteUrl = xe.Value.Trim();
            //site-group-url
            xe = xp.Element("site-group-url");
            if (xe != null)
                liGroup.SiteGroupUrl = xe.Value.Trim();
            //locale
            xe = xp.Element("locale");
            if (xe != null)
                liGroup.Locale = xe.Value.Trim();
            //allow-member-invites
            xe = xp.Element("allow-member-invites");
            if (xe != null)
                liGroup.AllowMembersInvite = Convert.ToBoolean(xe.Value.Trim());
            //small-logo-url
            xe = xp.Element("small-logo-url");
            if (xe != null)
                liGroup.SmallLogoUrl = xe.Value.Trim();
            //large-logo-url
            xe = xp.Element("large-logo-url");
            if (xe != null)
                liGroup.LargeLogoUrl = xe.Value.Trim();
            //num-members
            xe = xp.Element("num-members");
            if (xe != null)
                liGroup.NumberOfMembers = Convert.ToInt32(xe.Value.Trim());
            //location
            xe = xp.Element("location");
            if (xe != null)
            {
                var xn = xe.Element("country");
                if (xn != null)
                {
                    xn = xn.Element("code");
                    if (xn != null)
                        liGroup.LocationCountry = xn.Value.Trim();
                }
                xn = xe.Element("postal-code");
                if (xn != null)
                    liGroup.LocationPostalCode = xn.Value.Trim();
            }

            return liGroup;
        }

        internal static LinkedInGroupPost BuildGroupPost(XElement xp)
        {
            var liPost = new LinkedInGroupPost();

            //id
            var xe = xp.Element("id");
            if (xe != null)
                liPost.Id = xe.Value.Trim();
            //type
            xe = xp.Element("type");
            if (xe != null)
            {
                var xn = xe.Element("code");
                if (xn != null)
                {
                    switch (xn.Value.Trim())
                    {
                        case "standard":
                            liPost.PostType = LinkedInGroupPostType.Standard;
                            break;
                        case "news":
                            liPost.PostType = LinkedInGroupPostType.News;
                            break;
                    }
                }
            }
            //category
            xe = xp.Element("category");
            if (xe != null)
            {
                var xn = xe.Element("code");
                if (xn != null)
                {
                    switch (xn.Value.Trim())
                    {
                        case "discussion":
                            liPost.Category = LinkedInGroupPostCategory.Discussion;
                            break;
                    }
                }
            }
            //creator
            xe = xp.Element("creator");
            if (xe != null)
                liPost.Creator = BuildPerson(new LinkedInPerson(), xe);
            //creation-timestamp
            xe = xp.Element("creation-timestamp");
            if (xe != null)
                liPost.CreationTime = GetRealDateTime(Convert.ToDouble(xe.Value.Trim()));
            //title
            xe = xp.Element("title");
            if (xe != null)
                liPost.Title = xe.Value.Trim();
            //summary
            xe = xp.Element("summary");
            if (xe != null)
                liPost.Summary = xe.Value.Trim();
            //relation-to-viewer
            xe = xp.Element("relation-to-viewer");
            if (xe != null)
            {
                //is-following
                var xn = xe.Element("is-following");
                if (xn != null)
                    liPost.IsFollowingByUser = Convert.ToBoolean(xn.Value.Trim());
                //is-liked
                xn = xe.Element("is-liked");
                if (xn != null)
                    liPost.IsLikedByUser = Convert.ToBoolean(xn.Value.Trim());
                var xactions = xe.Element("available-actions");
                if (xactions != null)
                {
                    foreach (var xc in xactions.Elements("action").Select(xa => xa.Element("code")).Where(xc => xc != null))
                    {
                        switch (xc.Value.Trim())
                        {
                            case "add-comment":
                                liPost.AvailableAction[LinkedInGroupPostAction.AddComment] = true;
                                break;
                            case "flag-as-inappropriate":
                            case "delete":
                                liPost.AvailableAction[LinkedInGroupPostAction.FlagAsInappropriate] = true;
                                break;
                            case "categorize-as-job":
                                liPost.AvailableAction[LinkedInGroupPostAction.CategorizeAsJob] = true;
                                break;
                            case "categorize-as-promotion":
                                liPost.AvailableAction[LinkedInGroupPostAction.CategorizeAsPromotion] = true;
                                break;
                            case "follow":
                            case "unfollow":
                                liPost.AvailableAction[LinkedInGroupPostAction.Follow] = true;
                                break;
                            case "like":
                            case "unlike":
                                liPost.AvailableAction[LinkedInGroupPostAction.Like] = true;
                                break;
                            case "reply-privately":
                                liPost.AvailableAction[LinkedInGroupPostAction.ReplyPrivately] = true;
                                break;
                        }
                    }
                }
            }
            //likes
            xe = xp.Element("likes");
            if (xe != null)
                liPost.AddLikes(xe.Elements("like").Select(BuildLike));
            //attachment
            xe = xp.Element("attachment");
            if (xe != null)
                liPost.Attachment = buildPostAttachment(xe);
            //site-group-post-url
            xe = xp.Element("site-group-post-url");
            if (xe != null)
                liPost.SiteGroupPostUrl = xe.Value.Trim();

            return liPost;
        }

        internal static LinkedInGroupComment BuildGroupComment(XElement xp)
        {
            var liComment = new LinkedInGroupComment();

            //id
            var xe = xp.Element("id");
            if (xe != null)
                liComment.Id = xe.Value.Trim();
            //text
            xe = xp.Element("text");
            if (xe != null)
                liComment.Text = xe.Value.Trim();
            //creator
            xe = xp.Element("creator");
            if (xe != null)
                liComment.Creator = BuildPerson(new LinkedInPerson(), xe);
            //creation-timestamp
            xe = xp.Element("creation-timestamp");
            if (xe != null)
                liComment.CreationTime = GetRealDateTime(Convert.ToDouble(xe.Value.Trim()));
            //relation-to-viewer
            xe = xp.Element("relation-to-viewer");
            if (xe == null) return liComment;
            var xactions = xe.Element("available-actions");
            if (xactions == null) return liComment;
            foreach (var xc in xactions.Elements("action").Select(xa => xa.Element("code")).Where(xc => xc != null))
            {
                switch (xc.Value.Trim())
                {
                    case "flag-as-inappropriate":
                        liComment.AvailableAction[LinkedInGroupCommentAction.FlagAsInappropriate] = true;
                        break;
                    case "delete":
                        liComment.AvailableAction[LinkedInGroupCommentAction.Delete] = true;
                        break;
                }
            }

            return liComment;
        }

        internal static LinkedInRecommendation BuildRecommendationReceived(XElement xp)
        {
            var liRecommendation = new LinkedInRecommendation();

            //id
            var xe = xp.Element("id");
            if (xe != null)
                liRecommendation.Id = xe.Value.Trim();
            //recommendation-type
            xe = xp.Element("recommendation-type");
            if (xe != null)
            {
                xe = xe.Element("code");
                if (xe != null)
                    liRecommendation.TypeCode = xe.Value.Trim();
            }
            //recommendation-text
            xe = xp.Element("recommendation-text");
            if (xe != null)
                liRecommendation.Text = xe.Value.Trim();
            //recommender
            xe = xp.Element("recommender");
            if (xe != null)
                liRecommendation.RecommendationPerson = BuildPerson(new LinkedInPerson(), xe);
            //web-url
            xe = xp.Element("web-url");
            if (xe != null)
                liRecommendation.WebUrl = xe.Value.Trim();

            liRecommendation.RecommendationType = LinkedInRecommendationType.Received;

            return liRecommendation;
        }

        internal static LinkedInRecommendation BuildRecommendationGiven(XElement xp)
        {
            var liRecommendation = new LinkedInRecommendation();

            //id
            var xe = xp.Element("id");
            if (xe != null)
                liRecommendation.Id = xe.Value.Trim();
            //recommendation-type
            xe = xp.Element("recommendation-type");
            if (xe != null)
            {
                xe = xe.Element("code");
                if (xe != null)
                    liRecommendation.TypeCode = xe.Value.Trim();
            }
            //recommendation-snippet
            xe = xp.Element("recommendation-snippet");
            if (xe != null)
                liRecommendation.Text = xe.Value.Trim();
            //recommendee
            xe = xp.Element("recommendee");
            if (xe != null)
                liRecommendation.RecommendationPerson = BuildPerson(new LinkedInPerson(), xe);
            //web-url
            xe = xp.Element("web-url");
            if (xe != null)
                liRecommendation.WebUrl = xe.Value.Trim();

            liRecommendation.RecommendationType = LinkedInRecommendationType.Given;

            return liRecommendation;
        }

        internal static LinkedInJob BuildJob(XElement xp)
        {
            var liJob = new LinkedInJob();

            //id
            var xe = xp.Element("id");
            if (xe != null)
                liJob.Id = xe.Value.Trim();
            //active
            xe = xp.Element("active");
            if (xe != null)
                liJob.Active = Convert.ToBoolean(xe.Value.Trim());
            //company
            xe = xp.Element("company");
            if (xe != null)
                liJob.Company = BuildCompanyBase(xe);
            //position
            xe = xp.Element("position");
            if (xe != null)
            {
                xe = xe.Element("title");
                if (xe != null)
                    liJob.PositionTitle = xe.Value.Trim();
            }
            //description-snippet
            xe = xp.Element("description-snippet");
            if (xe != null)
                liJob.DescriptionSnippet = xe.Value.Trim();
            //description
            xe = xp.Element("description");
            if (xe != null)
                liJob.Description = xe.Value.Trim();
            //posting-timestamp
            xe = xp.Element("posting-timestamp");
            if (xe != null)
                liJob.PostingTimestamp = GetRealDateTime(Convert.ToDouble(xe.Value.Trim()));
            //site-job-request
            xe = xp.Element("site-job-request");
            if (xe != null)
            {
                xe = xe.Element("url");
                if (xe != null)
                    liJob.JobSiteRequestUrl = xe.Value.Trim();
            }
            //location-description
            xe = xp.Element("location-description");
            if (xe != null)
                liJob.LocationDescription = xe.Value.Trim();

            return liJob;
        }

        internal static LinkedInUpdate BuildUpdate(XElement xu)
        {
            var updType = "";
            var xe = xu.Element("update-type");
            if (xe != null)
                updType = xe.Value.Trim();
            LinkedInUpdate update = null;
            switch (updType)
            {
                case "CONN":
                    update = new LinkedInConnectionMemberUpdate();
                    break;
                case "NCON":
                case "CCEM":
                    update = new LinkedInConnectionUpdate();
                    break;
                case "SHAR":
                    update = new LinkedInShareUpdate();
                    break;
                case "STAT":
                    update = new LinkedInStatusUpdate();
                    break;
                case "VIRL":
                    update = new LinkedInViralUpdate();
                    break;
                case "JGRP":
                    update = new LinkedInGroupUpdate();
                    break;
                case "APPS":
                case "APPM":
                    update = new LinkedInApplicationUpdate();
                    break;
                case "PICU":
                    update = new LinkedInPictureUpdate();
                    break;
                case "PROF":
                    update = new LinkedInPositionUpdate();
                    break;
                case "PRFX":
                    update = new LinkedInExtendedProfileUpdate();
                    break;
                case "PREC":
                case "SVPR":
                    update = new LinkedInRecommendationUpdate();
                    break;
                case "JOBP":
                    update = new LinkedInJobPostingUpdate();
                    break;
                case "CMPY":
                    switch (getCompanyUpdateType(xu))
                    {
                        case CompanyUpdateType.Profile:
                            update = new LinkedInCompanyProfileUpdate();
                            break;
                        case CompanyUpdateType.Status:
                            update = new LinkedInCompanyStatusUpdate();
                            break;
                        case CompanyUpdateType.Job:
                            update = new LinkedInCompanyJobUpdate();
                            break;
                        case CompanyUpdateType.Person:
                            update = new LinkedInCompanyPersonUpdate();
                            break;
                    }
                    break;
                case "MSFC":
                    update = new LinkedInStartFollowCompanyUpdate();
                    break;
            }
            if (update != null)
                update.BuildUpdate(xu);
            return update;
        }

        internal static LinkedInMember BuildMember(XElement xp)
        {
            return new LinkedInMember
            {
                BasicProfile = buildBasicProfile(xp),
                EmailProfile = buildEmailProfile(xp),
                FullProfile = buildFullProfile(xp)
            };
        }

        private static LinkedInFullProfile buildFullProfile(XElement xp)
        {
            var liFull = new LinkedInFullProfile();

            //last-modified-timestamp
            var xe = xp.Element("last-modified-timestamp");
            if (xe != null)
                liFull.LastModifiedTimestamp = GetRealDateTime(Convert.ToDouble(xe.Value.Trim()));
            //proposal-comments
            xe = xp.Element("proposal-comments");
            if (xe != null)
                liFull.ProposalComments = xe.Value.Trim();
            //interests
            xe = xp.Element("interests");
            if (xe != null)
                liFull.Interests = xe.Value.Trim();
            //associations
            xe = xp.Element("associations");
            if (xe != null)
                liFull.Associations = xe.Value.Trim();
            //publications
            xe = xp.Element("publications");
            if (xe != null)
                liFull.AddPublications(xe.Elements("publication").Select(buildPublication));
            //patents
            xe = xp.Element("patents");
            if (xe != null)
                liFull.AddPatents(xe.Elements("patent").Select(buildPatent));
            //languages
            xe = xp.Element("languages");
            if (xe != null)
                liFull.AddLanguages(xe.Elements("language").Select(buildLanguage));
            //skills
            xe = xp.Element("skills");
            if (xe != null)
                liFull.AddSkills(xe.Elements("skill").Select(buildSkill));
            //educations
            xe = xp.Element("educations");
            if (xe != null)
                liFull.AddEducations(xe.Elements("education").Select(buildEducation));
            //certifications
            xe = xp.Element("certifications");
            if (xe != null)
                liFull.AddCertifications(xe.Elements("certification").Select(buildCertification));
            //courses
            xe = xp.Element("courses");
            if (xe != null)
                liFull.AddCourses(xe.Elements("course").Select(buildCourse));
            //volunteer
            xe = xp.Element("volunteer");
            if (xe != null)
            {
                xe = xe.Element("volunteer-experiences");
                if (xe != null)
                    liFull.AddVolunteer(xe.Elements("volunteer-experience").Select(buildVoluteerExpirience));
            }
            //three-current-positions
            xe = xp.Element("three-current-positions");
            if (xe != null)
                liFull.AddPositions(xe.Elements("position").Select(buildPosition), true);
            //three-past-positions
            xe = xp.Element("three-past-positions");
            if (xe != null)
                liFull.AddPositions(xe.Elements("position").Select(buildPosition), false);
            //related-profile-views
            xe = xp.Element("related-profile-views");
            if (xe != null)
                liFull.AddRelatedProfileViews(
                    xe.Elements("person").Select(p => BuildPerson(new LinkedInPerson(), p)));
            //member-url-resources
            xe = xp.Element("member-url-resources");
            if (xe != null)
                liFull.AddMemberUrls(xe.Elements("member-url").Select(buildMemberUrls));
            //date-of-birth
            xe = xp.Element("date-of-birth");
            if (xe != null)
                liFull.DateOfBirth = BuildDate(xe);
            //num-recommenders
            xe = xp.Element("num-recommenders");
            if (xe != null)
                liFull.NumRecommenders = Convert.ToInt32(xe.Value.Trim());
            //recommendations-received
            xe = xp.Element("recommendations-received");
            if (xe != null)
                liFull.AddRecommendations(xe.Elements("recommendation").Select(BuildRecommendationReceived));
            //mfeed-rss-url
            xe = xp.Element("mfeed-rss-url");
            if (xe != null)
                liFull.MfeedRssUrl = xe.Value.Trim();
            //job-bookmarks
            xe = xp.Element("job-bookmarks");
            if (xe != null)
                liFull.AddJobBookmarks(xe.Elements("job-bookmark").Select(buildJobBookmark));
            //following
            xe = xp.Element("following");
            if (xe != null)
                liFull.Following = buildFollowing(xe);
            //suggestions
            xe = xp.Element("suggestions");
            if (xe != null)
                liFull.Suggestions = buildSuggestions(xe);
            //connections
            xe = xp.Element("connections");
            if (xe != null)
                liFull.AddConnections(
                    xe.Elements("person").Select(p => BuildPerson(new LinkedInPerson(), p)));

            return liFull;
        }

        private static LinkedInHttpHeader buildHeader(XElement xp)
        {
            var liHeader = new LinkedInHttpHeader();

            //name
            var xe = xp.Element("name");
            if (xe != null)
                liHeader.Name = xe.Value.Trim();
            //value
            xe = xp.Element("value");
            if (xe != null)
                liHeader.Value = xe.Value.Trim();

            return liHeader;
        }

        private static LinkedInApiStandardProfileRequest buildApiStandardProfileRequest(XElement xp)
        {
            var liRequest = new LinkedInApiStandardProfileRequest();

            //url
            var xn = xp.Element("url");
            if (xn != null)
                liRequest.Url = xn.Value.Trim();
            //headers
            xn = xp.Element("headers");
            if (xn != null)
            {
                liRequest.AddHeaders(xn.Elements("http-header").Select(buildHeader));
            }

            return liRequest;
        }

        private static LinkedInCompany buildCompany(XElement xp)
        {
            var liCompany = new LinkedInCompany();

            //id
            var xe = xp.Element("id");
            if (xe != null)
                liCompany.Id = xe.Value.Trim();
            //name
            xe = xp.Element("name");
            if (xe != null)
                liCompany.Name = xe.Value.Trim();
            //size
            xe = xp.Element("size");
            if (xe != null)
                liCompany.Size = xe.Value.Trim();
            //type
            xe = xp.Element("type");
            if (xe != null)
                liCompany.Type = xe.Value.Trim();
            //industry
            xe = xp.Element("industry");
            if (xe != null)
                liCompany.Industry = xe.Value.Trim();
            //ticker
            xe = xp.Element("ticker");
            if (xe != null)
                liCompany.Ticker = xe.Value.Trim();

            return liCompany;
        }

        private static LinkedInSuggestions buildSuggestions(XElement xp)
        {
            var liSuggestion = new LinkedInSuggestions();

            var xe = xp.Element("to-follow");
            if (xe != null)
                liSuggestion.ToFollow = buildToFollow(xe);
            return liSuggestion;
        }

        private static LinkedInToFollow buildToFollow(XElement xp)
        {
            var liToFollow = new LinkedInToFollow();

            //people
            var xe = xp.Element("people");
            if (xe != null)
                liToFollow.AddPeople(
                    xe.Elements("person").Select(p => BuildPerson(new LinkedInPerson(), p)));
            //companies
            xe = xp.Element("companies");
            if (xe != null)
                liToFollow.AddCompanies(xe.Elements("company").Select(buildCompany));
            //industries
            xe = xp.Element("industries");
            if (xe != null)
                liToFollow.AddIndustries(xe.Elements("industry").Select(buildIndustry));
            //news-sources
            xe = xp.Element("news-sources");
            if (xe != null)
                liToFollow.AddNewsSources(xe.Elements("news-source").Select(buildNewsSource));

            return liToFollow;
        }

        private static LinkedInNewsSource buildNewsSource(XElement xp)
        {
            var liNewsSource = new LinkedInNewsSource();

            //id
            var xe = xp.Element("id");
            if (xe != null)
                liNewsSource.Id = xe.Value.Trim();
            //name
            xe = xp.Element("name");
            if (xe != null)
                liNewsSource.Name = xe.Value.Trim();

            return liNewsSource;
        }

        private static LinkedInJobBookmark buildJobBookmark(XElement xp)
        {
            var liJbookmark = new LinkedInJobBookmark();

            //is-applied
            var xe = xp.Element("is-applied");
            if (xe != null)
                liJbookmark.IsApplied = Convert.ToBoolean(xe.Value.Trim());
            //is-saved
            xe = xp.Element("is-saved");
            if (xe != null)
                liJbookmark.IsSaved = Convert.ToBoolean(xe.Value.Trim());
            //saved-timestamp
            xe = xp.Element("saved-timestamp");
            if (xe != null)
                liJbookmark.SavedTimestamp = GetRealDateTime(Convert.ToDouble(xe.Value.Trim()));
            //job
            xe = xp.Element("job");
            if (xe != null)
                liJbookmark.Job = BuildJob(xe);

            return liJbookmark;
        }

        private static LinkedInFollowing buildFollowing(XElement xp)
        {
            var liFoll = new LinkedInFollowing();

            //people
            var xe = xp.Element("people");
            if (xe != null)
                liFoll.AddPeople(xe.Elements("person").Select(p => BuildPerson(new LinkedInPerson(), p)));
            //companies
            xe = xp.Element("companies");
            if (xe != null)
                liFoll.AddCompanies(xe.Elements("company").Select(buildCompany));
            //industries
            xe = xp.Element("industries");
            if (xe != null)
                liFoll.AddIndustries(xe.Elements("industry").Select(buildIndustry));
            //special-editions
            xe = xp.Element("special-editions");
            if (xe != null)
                liFoll.AddSpecialEditions(xe.Elements("special-edition").Select(buildSpecialEdition));

            return liFoll;
        }

        private static LinkedInSpecialEdition buildSpecialEdition(XElement xp)
        {
            var liSpecialEd = new LinkedInSpecialEdition();

            return liSpecialEd;
        }

        private static LinkedInIndustry buildIndustry(XElement xp)
        {
            var liIndustry = new LinkedInIndustry();

            //id
            var xe = xp.Element("id");
            if (xe != null)
                liIndustry.Id = xe.Value.Trim();
            if (IndustryCodes.ContainsKey(liIndustry.Id))
            {
                var temp = IndustryCodes[liIndustry.Id];
                var arr = temp.Split('|');
                liIndustry.Group = arr[0];
                liIndustry.Description = arr[1];
            }
            return liIndustry;
        }

        private static LinkedInMemberUrl buildMemberUrls(XElement xp)
        {
            var liUrl = new LinkedInMemberUrl();

            //url
            var xe = xp.Element("url");
            if (xe != null)
                liUrl.Url = xe.Value.Trim();
            //name
            xe = xp.Element("name");
            if (xe != null)
                liUrl.Name = xe.Value.Trim();

            return liUrl;
        }

        private static LinkedInPublication buildPublication(XElement xp)
        {
            var liPub = new LinkedInPublication();
            //id
            var xe = xp.Element("id");
            if (xe != null)
                liPub.Id = xe.Value.Trim();
            //title
            xe = xp.Element("title");
            if (xe != null)
                liPub.Title = xe.Value.Trim();
            //summary
            xe = xp.Element("summary");
            if (xe != null)
                liPub.Summary = xe.Value.Trim();
            //url
            xe = xp.Element("url");
            if (xe != null)
                liPub.Url = xe.Value.Trim();
            //publisher
            xe = xp.Element("publisher");
            if (xe != null)
            {
                var xn = xe.Element("name");
                if (xn != null)
                    liPub.Publisher = xn.Value.Trim();
            }
            //date
            xe = xp.Element("date");
            if (xe != null)
            {
                liPub.Date = BuildDate(xe);
            }
            //authors
            xe = xp.Element("authors");
            if (xe != null)
            {
                liPub.AddAuthors(xe.Elements("author").Select(buildAuthor));
            }

            return liPub;
        }

        private static LinkedInVolunteerExperience buildVoluteerExpirience(XElement xp)
        {
            var liExp = new LinkedInVolunteerExperience();

            //id
            var xe = xp.Element("id");
            if (xe != null)
                liExp.Id = xe.Value.Trim();
            //role
            xe = xp.Element("role");
            if (xe != null)
                liExp.Role = xe.Value.Trim();
            //organization
            xe = xp.Element("organization");
            if (xe != null)
            {
                var xn = xe.Element("id");
                if (xn != null)
                    liExp.Organization.Id = xn.Value.Trim();
                xn = xe.Element("name");
                if (xn != null)
                    liExp.Organization.Name = xn.Value.Trim();
            }
            //cause
            xe = xp.Element("cause");
            if (xe != null)
            {
                var xn = xe.Element("name");
                if (xn != null)
                    liExp.Cause = xn.Value.Trim();
            }

            return liExp;
        }

        private static LinkedInCourse buildCourse(XElement xp)
        {
            var liCourse = new LinkedInCourse();

            //id
            var xe = xp.Element("id");
            if (xe != null)
                liCourse.Id = xe.Value.Trim();
            //name
            xe = xp.Element("name");
            if (xe != null)
                liCourse.Name = xe.Value.Trim();
            //number
            xe = xp.Element("number");
            if (xe != null)
                liCourse.Number = xe.Value.Trim();

            return liCourse;
        }

        private static LinkedInEducation buildEducation(XElement xp)
        {
            var liEducation = new LinkedInEducation();

            //id
            var xe = xp.Element("id");
            if (xe != null)
                liEducation.Id = xe.Value.Trim();
            //school-name
            xe = xp.Element("school-name");
            if (xe != null)
                liEducation.SchoolName = xe.Value.Trim();
            //field-of-study
            xe = xp.Element("field-of-study");
            if (xe != null)
                liEducation.FieldOfStudy = xe.Value.Trim();
            //start-date
            xe = xp.Element("start-date");
            if (xe != null)
                liEducation.StartDate = BuildDate(xe);
            //end-date
            xe = xp.Element("end-date");
            if (xe != null)
                liEducation.EndDate = BuildDate(xe);
            //degree
            xe = xp.Element("degree");
            if (xe != null)
                liEducation.Degree = xe.Value.Trim();
            //activities
            xe = xp.Element("activities");
            if (xe != null)
                liEducation.Activities = xe.Value.Trim();
            //notes
            xe = xp.Element("notes");
            if (xe != null)
                liEducation.Notes = xe.Value.Trim();

            return liEducation;
        }

        private static LinkedInPatent buildPatent(XElement xp)
        {
            var liPatent = new LinkedInPatent();

            //id
            var xe = xp.Element("id");
            if (xe != null)
                liPatent.Id = xe.Value.Trim();
            //title
            xe = xp.Element("title");
            if (xe != null)
                liPatent.Title = xe.Value.Trim();
            //summary
            xe = xp.Element("summary");
            if (xe != null)
                liPatent.Summary = xe.Value.Trim();
            //number
            xe = xp.Element("number");
            if (xe != null)
                liPatent.Number = xe.Value.Trim();
            //status
            xe = xp.Element("status");
            if (xe != null)
            {
                var xn = xe.Element("id");
                if (xn != null)
                    liPatent.Status.Type = (LinkedInPatentStatusType)Convert.ToInt32(xn.Value.Trim());
                xn = xe.Element("name");
                if (xn != null)
                    liPatent.Status.Name = xn.Value.Trim();
            }
            //office
            xe = xp.Element("office");
            if (xe != null)
            {
                var xn = xe.Element("name");
                if (xn != null)
                    liPatent.Office.Name = xn.Value.Trim();
            }
            //inventors
            xe = xp.Element("inventors");
            if (xe != null)
                liPatent.AddInventors(xe.Elements("inventor").Select(buildInventor));
            //date
            xe = xp.Element("date");
            if (xe != null)
                liPatent.Date = BuildDate(xe);

            return liPatent;
        }

        private static LinkedInInventor buildInventor(XElement xp)
        {
            var liInventor = new LinkedInInventor();

            //id
            var xe = xp.Element("id");
            if (xe != null)
                liInventor.Id = xe.Value.Trim();
            //person
            xe = xp.Element("person");
            if (xe != null)
                liInventor.Person = BuildPerson(new LinkedInPerson(), xe);

            return liInventor;
        }

        private static LinkedInAuthor buildAuthor(XElement xp)
        {
            var liAuthor = new LinkedInAuthor();
            //id
            var xe = xp.Element("id");
            if (xe != null)
                liAuthor.Id = xe.Value.Trim();
            //person
            xe = xp.Element("person");
            if (xe != null)
                liAuthor.Person = BuildPerson(new LinkedInPerson(), xe);

            return liAuthor;
        }

        private static LinkedInSkill buildSkill(XElement xp)
        {
            var liSkill = new LinkedInSkill();

            //id
            var xe = xp.Element("id");
            if (xe != null)
                liSkill.Id = xe.Value.Trim();
            //name
            xe = xp.Element("skill");
            if (xe != null)
            {
                var xn = xe.Element("name");
                if (xn != null)
                    liSkill.Name = xn.Value.Trim();
            }

            return liSkill;
        }

        private static LinkedInLanguage buildLanguage(XElement xp)
        {
            var liLang = new LinkedInLanguage();

            //id
            var xe = xp.Element("id");
            if (xe != null)
                liLang.Id = xe.Value.Trim();
            //name
            xe = xp.Element("language");
            if (xe != null)
            {
                var xn = xe.Element("name");
                if (xn != null)
                    liLang.Name = xn.Value.Trim();
            }
            //proficiency
            xe = xp.Element("proficiency");
            if (xe != null)
            {
                var xn = xe.Element("level");
                if (xn != null)
                    liLang.Proficiency.Level = xn.Value.Trim();
                xn = xe.Element("name");
                if (xn != null)
                    liLang.Proficiency.Name = xn.Value.Trim();
            }

            return liLang;
        }

        private static LinkedInEmailProfile buildEmailProfile(XElement xp)
        {
            var liEmail = new LinkedInEmailProfile();

            //email-address
            var xe = xp.Element("email-address");
            if (xe != null)
                liEmail.EmailAddress = xe.Value.Trim();

            return liEmail;
        }

        private static LinkedInBasicProfile buildBasicProfile(XElement xp)
        {
            var liBasic = new LinkedInBasicProfile();

            //id
            var xe = xp.Element("id");
            if (xe != null)
                liBasic.Id = xe.Value.Trim();
            //first-name
            xe = xp.Element("first-name");
            if (xe != null)
                liBasic.FirstName = xe.Value.Trim();
            //last-name
            xe = xp.Element("last-name");
            if (xe != null)
                liBasic.LastName = xe.Value.Trim();
            //maiden-name
            xe = xp.Element("maiden-name");
            if (xe != null)
                liBasic.MaidenName = xe.Value.Trim();
            //formatted-name
            xe = xp.Element("formatted-name");
            if (xe != null)
                liBasic.FormattedName = xe.Value.Trim();
            //phonetic-first-name
            xe = xp.Element("phonetic-first-name");
            if (xe != null)
                liBasic.PhoneticFirstName = xe.Value.Trim();
            //phonetic-last-name
            xe = xp.Element("phonetic-last-name");
            if (xe != null)
                liBasic.PhoneticLastName = xe.Value.Trim();
            //formatted-phonetic-name
            xe = xp.Element("formatted-phonetic-name");
            if (xe != null)
                liBasic.FormattedPhoneticName = xe.Value.Trim();
            //headline
            xe = xp.Element("headline");
            if (xe != null)
                liBasic.Headline = xe.Value.Trim();
            //location
            xe = xp.Element("location");
            if (xe != null)
            {
                var xn = xe.Element("name");
                if (xn != null)
                    liBasic.Location.Name = xn.Value.Trim();
                xn = xe.Element("country");
                if (xn != null)
                {
                    xn = xn.Element("code");
                    if (xn != null)
                        liBasic.Location.CountryCode = xn.Value.Trim();
                }
            }
            //industry
            xe = xp.Element("industry");
            if (xe != null)
                liBasic.Industry = xe.Value.Trim();
            //distance
            xe = xp.Element("distance");
            if (xe != null)
                liBasic.Distance = Convert.ToInt32(xe.Value.Trim());
            //relation-to-viewer
            xe = xp.Element("relation-to-viewer");
            if (xe != null)
            {
                var xn = xe.Element("relation-to-viewer");
                if (xn != null)
                {
                    xn = xn.Element("distance");
                    if (xn != null)
                        liBasic.RelationToViewer.Distance = Convert.ToInt32(xn.Value.Trim());
                }
            }
            //current-share
            xe = xp.Element("current-share");
            if (xe != null)
            {
                liBasic.CurrentShare = BuildShare(xe);
            }
            //num-connections
            xe = xp.Element("num-connections");
            if (xe != null)
                liBasic.NumConnections = Convert.ToInt32(xe.Value.Trim());
            //num-connections-capped
            xe = xp.Element("num-connections-capped");
            if (xe != null)
                liBasic.NumConnectionsCapped = Convert.ToBoolean(xe.Value.Trim());
            //summary
            xe = xp.Element("summary");
            if (xe != null)
                liBasic.Summary = xe.Value.Trim();
            //specialties
            xe = xp.Element("specialties");
            if (xe != null)
                liBasic.Specialities = xe.Value.Trim();
            //positions
            xe = xp.Element("positions");
            if (xe != null)
            {
                liBasic.AddPositions(xe.Elements("position").Select(buildPosition));
            }
            //picture-url
            xe = xp.Element("picture-url");
            if (xe != null)
                liBasic.PictureUrl = xe.Value.Trim();
            //site-standard-profile-request
            xe = xp.Element("site-standard-profile-request");
            if (xe != null)
            {
                var xn = xe.Element("url");
                if (xn != null)
                    liBasic.SiteStandardProfileRequest = xn.Value.Trim();
            }
            //api-standard-profile-request
            xe = xp.Element("api-standard-profile-request");
            if (xe != null)
            {
                liBasic.ApiStandardProfileRquest = buildApiStandardProfileRequest(xe);
            }
            //public-profile-url
            xe = xp.Element("public-profile-url");
            if (xe != null)
                liBasic.PublicProfileUrl = xe.Value.Trim();

            return liBasic;
        }

        private static LinkedInPosition buildPosition(XElement xp)
        {
            var liPosition = new LinkedInPosition();

            //id
            var xe = xp.Element("id");
            if (xe != null)
                liPosition.Id = xe.Value.Trim();
            //title
            xe = xp.Element("title");
            if (xe != null)
                liPosition.Title = xe.Value.Trim();
            //summary
            xe = xp.Element("summary");
            if (xe != null)
                liPosition.Summary = xe.Value.Trim();
            //start-date
            xe = xp.Element("start-date");
            if (xe != null)
                liPosition.StartDate = BuildDate(xe);
            //end-date
            xe = xp.Element("end-date");
            if (xe != null)
                liPosition.EndDate = BuildDate(xe);
            //company
            xe = xp.Element("company");
            if (xe != null)
                liPosition.Company = buildCompany(xe);
            //is-current
            xe = xp.Element("is-current");
            if (xe != null)
                liPosition.IsCurrent = Convert.ToBoolean(xe.Value.Trim());

            return liPosition;
        }

        private static LinkedInCertification buildCertification(XElement xp)
        {
            var liCert = new LinkedInCertification();

            //id
            var xe = xp.Element("id");
            if (xe != null)
                liCert.Id = xe.Value.Trim();
            //name
            xe = xp.Element("name");
            if (xe != null)
                liCert.Name = xe.Value.Trim();
            //authority
            xe = xp.Element("authority");
            if (xe != null)
            {
                var xn = xe.Element("name");
                if (xn != null)
                    liCert.AuthorityName = xn.Value.Trim();
            }
            //number
            xe = xp.Element("number");
            if (xe != null)
                liCert.Number = xe.Value.Trim();
            //start-date
            xe = xp.Element("start-date");
            if (xe != null)
                liCert.StartDate = BuildDate(xe);

            return liCert;
        }

        private static LinkedInBucket buildBucket(XElement xp)
        {
            var liBucket = new LinkedInBucket();

            //code
            var xe = xp.Element("code");
            if (xe != null)
                liBucket.Code = xe.Value.Trim();
            //name
            xe = xp.Element("name");
            if (xe != null)
                liBucket.Name = xe.Value.Trim();
            //count
            xe = xp.Element("count");
            if (xe != null)
                liBucket.Count = Convert.ToInt32(xe.Value.Trim());
            //selected
            xe = xp.Element("selected");
            if (xe != null)
                liBucket.Selected = Convert.ToBoolean(xe.Value.Trim());

            return liBucket;
        }

        private static LinkedInGroupCategoryCount buildGroupCategoryCount(XElement xp)
        {
            var liCount = new LinkedInGroupCategoryCount();

            //category
            var xe = xp.Element("category");
            if (xe != null)
            {
                var xc = xe.Element("code");
                if (xc != null)
                    liCount.Category = xc.Value.Trim();
            }
            xe = xp.Element("count");
            if (xe != null)
                liCount.Count = Convert.ToInt32(xe.Value.Trim());

            return liCount;
        }

        private static LinkedInGroupPostAttachment buildPostAttachment(XElement xp)
        {
            var liAttach = new LinkedInGroupPostAttachment();

            //content-url
            var xe = xp.Element("content-url");
            if (xe != null)
                liAttach.ContentUrl = xe.Value.Trim();
            //title
            xe = xp.Element("title");
            if (xe != null)
                liAttach.Title = xe.Value.Trim();
            //summary
            xe = xp.Element("summary");
            if (xe != null)
                liAttach.Summary = xe.Value.Trim();
            //image-url
            xe = xp.Element("image-url");
            if (xe != null)
                liAttach.ImageUrl = xe.Value.Trim();
            //content-domain
            xe = xp.Element("content-domain");
            if (xe != null)
                liAttach.ContentDomain = xe.Value.Trim();

            return liAttach;
        }

        private static CompanyUpdateType getCompanyUpdateType(XElement xp)
        {
            //update-content
            var xc = xp.Element("update-content");
            if (xc != null)
            {
                if (xc.Elements().Any(xe => xe.Name.ToString() == "company-profile-update"))
                    return CompanyUpdateType.Profile;
                if (xc.Elements().Any(xe => xe.Name.ToString() == "company-status-update"))
                    return CompanyUpdateType.Status;
                if (xc.Elements().Any(xe => xe.Name.ToString() == "company-job-update"))
                    return CompanyUpdateType.Job;
                if (xc.Elements().Any(xe => xe.Name.ToString() == "company-person-update"))
                    return CompanyUpdateType.Person;
            }
            return CompanyUpdateType.Invalid;
        }

        internal static Tuple<LinkedInResponseStatus, string> GetResponseError(WebException wex)
        {
            var result = new Tuple<LinkedInResponseStatus, string>(LinkedInResponseStatus.UnparsedWebException, wex.Message);
            if (wex.Response == null) return result;
            var stream = wex.Response.GetResponseStream();
            if (stream == null) return result;
            using (var reader = new StreamReader(stream))
            {
                try
                {
                    var xdoc = XDocument.Parse(reader.ReadToEnd());
                    var xroot = xdoc.Root;
                    if (xroot == null) return result;
                    var xStatus = xroot.Element("status");
                    var xMessage = xroot.Element("message");
                    if (xStatus == null || xMessage == null) return result;
                    switch (Convert.ToInt32(xStatus.Value))
                    {
                        case 400:
                            return new Tuple<LinkedInResponseStatus, string>(LinkedInResponseStatus.BadRequest,
                                xMessage.Value);
                        case 401:
                            if (xMessage.Value.ToLower().Contains("expired"))
                                return new Tuple<LinkedInResponseStatus, string>(LinkedInResponseStatus.ExpiredToken,
                                xMessage.Value);
                            if (xMessage.Value.ToLower().Contains("invalid access token") || xMessage.Value.ToLower().Contains("unable to verify access token"))
                                return new Tuple<LinkedInResponseStatus, string>(LinkedInResponseStatus.InvalidAccessToken,
                                xMessage.Value);
                            if (xMessage.Value.ToLower().Contains("ssl required"))
                                return new Tuple<LinkedInResponseStatus, string>(LinkedInResponseStatus.SslRequired,
                                xMessage.Value);
                            break;
                        case 403:
                            if (xMessage.Value.ToLower().Contains("throttle limit for calls to this resource"))
                                return new Tuple<LinkedInResponseStatus, string>(LinkedInResponseStatus.TrottleLimitReached,
                                xMessage.Value);
                            if (xMessage.Value.ToLower().Contains("unauthorized change of forum type") || xMessage.Value.ToLower().Contains("anetauthexception: applicationrequired"))
                                return new Tuple<LinkedInResponseStatus, string>(LinkedInResponseStatus.UnauthorizedAction,
                                    xMessage.Value);
                            return new Tuple<LinkedInResponseStatus, string>(LinkedInResponseStatus.UnparsedWebException,
                                xMessage.Value);
                        case 404:
                            return new Tuple<LinkedInResponseStatus, string>(LinkedInResponseStatus.PageNotFound,
                                xMessage.Value);
                        case 500:
                            return new Tuple<LinkedInResponseStatus, string>(LinkedInResponseStatus.InternalServerError,
                                xMessage.Value);
                    }
                    return result;
                }
                catch (Exception)
                {
                    return new Tuple<LinkedInResponseStatus, string>(LinkedInResponseStatus.UnparsedWebException, reader.ReadToEnd());
                }
            }
        }

        internal static LinkedInResponse<T> GetResponse<T>(T type, WebException wex, object userState)
        {
            var err = GetResponseError(wex);
            return new LinkedInResponse<T>(type, err.Item1, userState, err.Item2);
        }

        internal static bool IsAnyString(params string[] strings)
        {
            return strings.Any(s => !string.IsNullOrEmpty(s));
        }
    }
}
