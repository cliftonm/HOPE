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
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using LinkedIn.NET.Groups;
using LinkedIn.NET.Members;
using LinkedIn.NET.Options;
using LinkedIn.NET.Search;
using LinkedIn.NET.Updates;

namespace LinkedIn.NET
{
    internal static class RequestRunner
    {
        internal static LinkedInResponse<IEnumerable<LinkedInMember>> GetMembers(
            LinkedInGetMultipleMembersOptions options)
        {
            MethodInfo getSyntax = null;
            FieldInfo flagsField = null;
            try
            {
                getSyntax = typeof(UriParser).GetMethod("GetSyntax", BindingFlags.Static | BindingFlags.NonPublic);
                flagsField = typeof(UriParser).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);
                setCompressionFlag(getSyntax, flagsField, false);

                var fieldsParams = RequestFields.PrepareMemberFields(options);
                var sb = new StringBuilder(Utils.PROFILE_MULTIPLE_URL);
                sb.Append("(");
                foreach (var p in options.Params)
                {
                    switch (p.GetBy)
                    {
                        case LinkedInGetMemberBy.Self:
                            sb.Append("~");
                            break;
                        case LinkedInGetMemberBy.Id:
                            sb.Append("id=");
                            sb.Append(p.RequestBy);
                            break;
                        case LinkedInGetMemberBy.Url:
                            sb.Append("url=");
                            sb.Append(Utils.NormalizeUrl(p.RequestBy));
                            break;
                    }
                    sb.Append(",");
                }
                if (sb.Length > 1) sb.Length -= 1;
                sb.Append(")");
                
                if (!string.IsNullOrEmpty(fieldsParams))
                    sb.Append(fieldsParams);

                sb.Append("?oauth2_access_token=");
                sb.Append(Singleton.Instance.AccessToken);
                var responseString = Utils.MakeRequest(sb.ToString(), "GET");
                var xdoc = XDocument.Parse(responseString);

                var members = new List<LinkedInMember>();
                if (xdoc.Root != null)
                {
                    members.AddRange(xdoc.Root.Elements("person").Select(Utils.BuildMember));
                }

                return new LinkedInResponse<IEnumerable<LinkedInMember>>(members.AsEnumerable(),
                    LinkedInResponseStatus.OK, null);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse<IEnumerable<LinkedInMember>>(null, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<IEnumerable<LinkedInMember>>(null, LinkedInResponseStatus.OtherException, null, ex);
            }
            finally
            {
                setCompressionFlag(getSyntax, flagsField, true);
            }
        }

        internal static LinkedInResponse<LinkedInMember> GetMember(LinkedInGetMemberOptions options)
        {
            var sb = new StringBuilder();
            var fieldsParams = RequestFields.PrepareMemberFields(options);

            switch (options.Parameters.GetBy)
            {
                case LinkedInGetMemberBy.Self:
                    sb.Append(Utils.PROFILE_SELF_URL);
                    break;
                case LinkedInGetMemberBy.Id:
                    sb.Append(Utils.PROFILE_BY_ID_URL);
                    sb.Append(options.Parameters.RequestBy);
                    break;
                default:    //LinkedInGetMemberBy.Url
                    sb.Append(Utils.PROFILE_BY_URL_URL);
                    sb.Append(Utils.NormalizeUrl(options.Parameters.RequestBy));
                    break;
            }

            if (!string.IsNullOrEmpty(fieldsParams))
                sb.Append(fieldsParams);

            sb.Append("?oauth2_access_token=");
            sb.Append(Singleton.Instance.AccessToken);

            return getMember(sb.ToString(), options);
        }

        private static LinkedInResponse<LinkedInMember> getMember(string request, LinkedInGetMemberOptions options)
        {
            try
            {
                string responseString;
                switch (options.Parameters.GetBy)
                {
                    case LinkedInGetMemberBy.Url:
                        var getSyntax = typeof(UriParser).GetMethod("GetSyntax", BindingFlags.Static | BindingFlags.NonPublic);
                        var flagsField = typeof(UriParser).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);
                        setCompressionFlag(getSyntax, flagsField, false);
                        try
                        {
                            responseString = Utils.MakeRequest(request, "GET");
                        }
                        finally
                        {
                            setCompressionFlag(getSyntax, flagsField, true);
                        }
                        break;
                    default:
                        responseString = Utils.MakeRequest(request, "GET");
                        break;
                }
                var xdoc = XDocument.Parse(responseString);

                var result = xdoc.Root != null ? Utils.BuildMember(xdoc.Root) : null;
                return new LinkedInResponse<LinkedInMember>(result, LinkedInResponseStatus.OK, null);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse<LinkedInMember>(null, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<LinkedInMember>(null, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        internal static LinkedInResponse<LinkedInSearchResult> GetSearchResult(LinkedInSearchOptions options)
        {
            try
            {
                var result = new LinkedInSearchResult();

                //if (options.MemberFieldOptions.HasValues &&
                //    (options.SearchSets & LinkedInSearchSets.People) != LinkedInSearchSets.People)
                //    options.SearchSets |= LinkedInSearchSets.People;
                if ((options.FacetFields.HasValues | options.BucketFields.HasValues) &&
                    (options.SearchSets & LinkedInSearchSets.Facets) != LinkedInSearchSets.Facets)
                    options.SearchSets |= LinkedInSearchSets.Facets;
                if (options.FacetLocationValues.Count > 0 && (options.FacetTypes & LinkedInFacetTypes.Location) != LinkedInFacetTypes.Location)
                    options.FacetTypes |= LinkedInFacetTypes.Location;
                if (options.FacetIndustryValues.Count > 0 && (options.FacetTypes & LinkedInFacetTypes.Industry) != LinkedInFacetTypes.Industry)
                    options.FacetTypes |= LinkedInFacetTypes.Industry;
                if (options.FacetLanguageValues.Count > 0 && (options.FacetTypes & LinkedInFacetTypes.Language) != LinkedInFacetTypes.Language)
                    options.FacetTypes |= LinkedInFacetTypes.Language;
                if (options.FacetCurrentCompanyValues.Count > 0 && (options.FacetTypes & LinkedInFacetTypes.CurrentCompany) != LinkedInFacetTypes.CurrentCompany)
                    options.FacetTypes |= LinkedInFacetTypes.CurrentCompany;
                if (options.FacetPastCompanyValues.Count > 0 && (options.FacetTypes & LinkedInFacetTypes.PastCompany) != LinkedInFacetTypes.PastCompany)
                    options.FacetTypes |= LinkedInFacetTypes.PastCompany;
                if (options.FacetSchoolValues.Count > 0 && (options.FacetTypes & LinkedInFacetTypes.School) != LinkedInFacetTypes.School)
                    options.FacetTypes |= LinkedInFacetTypes.School;
                if (options.FacetNetworkValues != LinkedInFacetNetwork.None && (options.FacetTypes & LinkedInFacetTypes.Network) != LinkedInFacetTypes.Network)
                    options.FacetTypes |= LinkedInFacetTypes.Network;

                var fieldsParams = RequestFields.PrepareMemberFields(options.MemberFieldOptions);
                var searchParams = RequestFields.PrepareSearchParams(options);
                var bucketsParams = RequestFields.PrepareBucketsParams(options);
                var facetsParams = RequestFields.PrepareFacetsParams(options);

                var sb = new StringBuilder(Utils.PEOPLE_SEARCH_URL);

                if (options.SearchSets != LinkedInSearchSets.None)
                    sb.Append(":(");

                if ((options.SearchSets & LinkedInSearchSets.People) == LinkedInSearchSets.People)
                {
                    sb.Append("people");
                    if (!string.IsNullOrEmpty(fieldsParams))
                        sb.Append(fieldsParams);
                    sb.Append(",");
                }

                if ((options.SearchSets & LinkedInSearchSets.Facets) == LinkedInSearchSets.Facets)
                {
                    sb.Append("facets");
                    if (!string.IsNullOrEmpty(bucketsParams))
                        sb.Append(bucketsParams);
                    sb.Append(",");
                }

                if (sb.Length > Utils.PEOPLE_SEARCH_URL.Length)
                    sb.Length -= 1;

                if (options.SearchSets != LinkedInSearchSets.None)
                    sb.Append(")");

                sb.Append("?");

                if (!string.IsNullOrEmpty(searchParams))
                    sb.Append(searchParams);

                if (!string.IsNullOrEmpty(facetsParams))
                    sb.Append(facetsParams);

                switch (options.Sort)
                {
                    case LinkedinSearchResultsOrder.Distance:
                        sb.Append("sort=distance&");
                        break;
                    case LinkedinSearchResultsOrder.Recommenders:
                        sb.Append("sort=recommenders&");
                        break;
                    case LinkedinSearchResultsOrder.Relevance:
                        sb.Append("sort=relevance&");
                        break;
                }

                if (options.Start.HasValue)
                {
                    sb.Append("start=");
                    sb.Append(options.Start.Value);
                    sb.Append("&");
                }
                if (options.Count.HasValue)
                {
                    sb.Append("count=");
                    sb.Append(options.Count.Value);
                    sb.Append("&");
                }
                sb.Append("oauth2_access_token=");
                sb.Append(Singleton.Instance.AccessToken);

                var responseString = Utils.MakeRequest(sb.ToString(), "GET");
                var xdoc = XDocument.Parse(responseString);
                var xroot = xdoc.Root;
                if (xroot == null)
                    return new LinkedInResponse<LinkedInSearchResult>(result, LinkedInResponseStatus.OK, null);

                if ((options.SearchSets & LinkedInSearchSets.Facets) == LinkedInSearchSets.Facets)
                {
                    var xp = xroot.Element("facets");
                    if (xp != null)
                        result.AddFacets(xp.Elements("facet").Select(Utils.BuildFacet));
                }
                if ((options.SearchSets & LinkedInSearchSets.People) != LinkedInSearchSets.People)
                    return new LinkedInResponse<LinkedInSearchResult>(result, LinkedInResponseStatus.OK, null);
                var xm = xroot.Element("people");
                if (xm == null)
                    return new LinkedInResponse<LinkedInSearchResult>(result, LinkedInResponseStatus.OK, null);
                var xpeople = xm.Elements("person").ToArray();
                result.AddPeople(xpeople.Select(Utils.BuildMember));
                return new LinkedInResponse<LinkedInSearchResult>(result, LinkedInResponseStatus.OK, null);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse<LinkedInSearchResult>(null, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<LinkedInSearchResult>(null, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        internal static LinkedInResponse<IEnumerable<LinkedInUpdate>> GetUpdates(LinkedInGetUpdatesOptions options)
        {
            switch (options.Parameters.GetBy)
            {
                case LinkedInGetMemberBy.Self:
                    return getUpdates(Utils.GET_UPDATES_URL, options);
                case LinkedInGetMemberBy.Id:
                    return getUpdates(Utils.GET_UPDATES_BY_ID_URL.Replace("$USER_ID$", options.Parameters.RequestBy), options);
                default:    //LinkedInGetMemberBy.Url
                    MethodInfo getSyntax = null;
                    FieldInfo flagsField = null;
                    try
                    {
                        getSyntax = typeof(UriParser).GetMethod("GetSyntax", BindingFlags.Static | BindingFlags.NonPublic);
                        flagsField = typeof(UriParser).GetField("m_Flags", BindingFlags.Instance | BindingFlags.NonPublic);
                        setCompressionFlag(getSyntax, flagsField, false);
                        return getUpdates(
                                Utils.GET_UPDATES_BY_URL_URL.Replace("$USER_URL$",
                                    Utils.NormalizeUrl(options.Parameters.RequestBy)), options);
                    }
                    finally
                    {
                        setCompressionFlag(getSyntax, flagsField, true);
                    }
            }
        }

        private static LinkedInResponse<IEnumerable<LinkedInUpdate>> getUpdates(string urlStart,
            LinkedInGetUpdatesOptions options)
        {
            try
            {
                var updates = new List<LinkedInUpdate>();

                var secondRequest = "";
                var sb = new StringBuilder(urlStart);

                sb.Append(RequestFields.PrepareGetUpdatesFields(options));

                //access token
                sb.Append("oauth2_access_token=");
                sb.Append(Singleton.Instance.AccessToken);

                if (options.UpdateScope == LinkedInUpdateScope.All && options.Parameters.GetBy == LinkedInGetMemberBy.Self)
                    secondRequest = sb.ToString().Replace("scope=self&", "");

                var responseXml = Utils.MakeRequest(sb.ToString(), "GET");
                var xdoc = XDocument.Parse(responseXml);

                if (xdoc.Root != null)
                {
                    var ac = xdoc.Root.Attribute("total");
                    if (ac != null && ac.Value != "0")
                    {
                        var ups = xdoc.Root.Elements("update");
                        updates.AddRange(ups.Select(Utils.BuildUpdate));
                    }
                }

                if (options.UpdateScope == LinkedInUpdateScope.All && options.Parameters.GetBy == LinkedInGetMemberBy.Self)
                {
                    responseXml = Utils.MakeRequest(secondRequest, "GET");
                    var secondXdoc = XDocument.Parse(responseXml);
                    if (secondXdoc.Root != null)
                    {
                        var ac = secondXdoc.Root.Attribute("total");
                        if (ac != null && ac.Value != "0")
                        {
                            var ups = secondXdoc.Root.Elements("update");
                            updates.AddRange(ups.Select(Utils.BuildUpdate));
                        }
                    }
                }
                updates.RemoveAll(u => u == null);
                IEnumerable<LinkedInUpdate> result;
                switch (options.SortBy)
                {
                    case LinkedInUpdateSortField.UpdateDate:
                        result = options.SortDirection == LinkedInUpdateSortDirection.Descending
                                   ? updates.OrderByDescending(u => u.UpdateDate)
                                   : updates.OrderBy(u => u.UpdateDate);
                        break;
                    case LinkedInUpdateSortField.UpdateKey:
                        result = options.SortDirection == LinkedInUpdateSortDirection.Descending
                                   ? updates.OrderByDescending(u => u.UpdateKey)
                                   : updates.OrderBy(u => u.UpdateKey);
                        break;
                    default:    //LinkedInUpdateSortField.UpdateType
                        result = options.SortDirection == LinkedInUpdateSortDirection.Descending
                                   ? updates.OrderByDescending(u => u.UpdateType)
                                   : updates.OrderBy(u => u.UpdateType);
                        break;
                }
                return new LinkedInResponse<IEnumerable<LinkedInUpdate>>(result, LinkedInResponseStatus.OK, null);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse<IEnumerable<LinkedInUpdate>>(null, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<IEnumerable<LinkedInUpdate>>(null, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        internal static LinkedInResponse<bool> SendInvitation(LinkedInInvitationOptions options)
        {
            try
            {
                var sb = new StringBuilder(Utils.SEND_MESSAGE_URL);
                sb.Append("?oauth2_access_token=");
                sb.Append(Singleton.Instance.AccessToken);
                var message = new StringBuilder("<?xml version='1.0' encoding='UTF-8'?><mailbox-item><recipients><recipient>");
                switch (options.InvitationType)
                {
                    case LinkedInInvitationType.InviteById:
                        message.Append("<person path='/people/id=");
                        message.Append(Utils.EscapeXml(options.RecipientId));
                        message.Append("' />");
                        break;
                    case LinkedInInvitationType.InviteByEmail:
                        message.Append("<person path='/people/email=");
                        message.Append(Utils.EscapeXml(options.RecipientEmail));
                        message.Append("'>");
                        message.Append("<first-name>");
                        message.Append(Utils.EscapeXml(options.RecipientFirstName));
                        message.Append("</first-name>");
                        message.Append("<last-name>");
                        message.Append(Utils.EscapeXml(options.RecipientLastName));
                        message.Append("</last-name>");
                        message.Append("</person>");
                        break;
                }
                message.Append("</recipient></recipients>");
                message.Append("<subject>");
                message.Append(Utils.EscapeXml(options.Subject));
                message.Append("</subject>");
                message.Append("<body>");
                message.Append(Utils.EscapeXml(options.Body));
                message.Append("</body>");
                message.Append("<item-content><invitation-request><connect-type>friend</connect-type>");
                if (options.InvitationType == LinkedInInvitationType.InviteById)
                {
                    message.Append("<authorization>");
                    message.Append("<name>");
                    message.Append(Utils.EscapeXml(options.AuthorizationName));
                    message.Append("</name>");
                    message.Append("<value>");
                    message.Append(Utils.EscapeXml(options.AuthorizationValue));
                    message.Append("</value>");
                    message.Append("</authorization>");
                }
                message.Append("</invitation-request></item-content>");
                message.Append("</mailbox-item>");

                var statusCode = HttpStatusCode.OK;
                Utils.MakeRequest(sb.ToString(), "POST", ref statusCode, message.ToString());
                return new LinkedInResponse<bool>(statusCode == HttpStatusCode.Created, LinkedInResponseStatus.OK, null);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse(false, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<bool>(false, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        internal static LinkedInResponse<bool> SendMessage(LinkedInMessageOptions options)
        {
            try
            {
                var sb = new StringBuilder(Utils.SEND_MESSAGE_URL);
                sb.Append("?oauth2_access_token=");
                sb.Append(Singleton.Instance.AccessToken);
                var message = new StringBuilder("<?xml version='1.0' encoding='UTF-8'?><mailbox-item><recipients>");
                foreach (var r in options.Recipients)
                {
                    message.Append("<recipient>");
                    message.Append("<person path='/people/");
                    message.Append(Utils.EscapeXml(r));
                    message.Append("'/></recipient>");
                }
                if (options.IncludeSenderInRecipients)
                    message.Append("<recipient><person path='/people/~'/></recipient>");
                message.Append("</recipients>");
                message.Append("<subject>");
                message.Append(Utils.EscapeXml(options.Subject));
                message.Append("</subject>");
                message.Append("<body>");
                message.Append(Utils.EscapeXml(options.Body));
                message.Append("</body>");
                message.Append("</mailbox-item>");
                var statusCode = HttpStatusCode.OK;
                Utils.MakeRequest(sb.ToString(), "POST", ref statusCode, message.ToString());
                return new LinkedInResponse<bool>(statusCode == HttpStatusCode.Created, LinkedInResponseStatus.OK, null);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse(false, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<bool>(false, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        internal static LinkedInResponse<LinkedInShareResult> ShareUpdate(LinkedInShareOptions options)
        {
            try
            {
                var url = new StringBuilder(Utils.UPDATE_STATUS_URL);
                url.Append("oauth2_access_token=");
                url.Append(Singleton.Instance.AccessToken);
                var body = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\"?><share><comment>");
                if (!string.IsNullOrEmpty(options.Comment))
                    body.Append(Utils.EscapeXml(options.Comment));
                body.Append("</comment>");
                var contentPresented = Utils.IsAnyString(options.Title, options.SubmittedUrl, options.SubmittedImageUrl,
                    options.Description);
                if (contentPresented)
                {
                    body.Append("<content><title>");
                    body.Append(Utils.EscapeXml(options.Title));
                    body.Append("</title><description>");
                    body.Append(Utils.EscapeXml(options.Description));
                    body.Append("</description><submitted-url>");
                    body.Append(Utils.EscapeXml(options.SubmittedUrl));
                    body.Append("</submitted-url><submitted-image-url>");
                    body.Append(Utils.EscapeXml(options.SubmittedImageUrl));
                    body.Append("</submitted-image-url></content>");
                }
                body.Append("<visibility><code>");
                body.Append(options.VisibilityCode == LinkedInShareVisibilityCode.Anyone ? "anyone" : "connections-only");
                body.Append("</code></visibility></share>");

                var responseString = Utils.MakeRequest(url.ToString(), "POST", body.ToString());
                var xdoc = XDocument.Parse(responseString);
                if (xdoc.Root != null)
                {
                    var eKey = xdoc.Root.Element("update-key");
                    var eUrl = xdoc.Root.Element("update-url");
                    if (eKey != null && eUrl != null)
                    {
                        return new LinkedInResponse<LinkedInShareResult>(
                            new LinkedInShareResult(eKey.Value, eUrl.Value), LinkedInResponseStatus.OK, null);
                    }
                }
                return new LinkedInResponse<LinkedInShareResult>(null, LinkedInResponseStatus.UpdateFailed, null);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse<LinkedInShareResult>(null, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<LinkedInShareResult>(null, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        internal static LinkedInResponse<IEnumerable<LinkedInLike>> GetAllUpdateLikes(string updateKey)
        {
            try
            {
                var likes = new List<LinkedInLike>();
                var request = new StringBuilder(Utils.UPDATE_LIKES_URL.Replace("{NETWORK UPDATE KEY}", updateKey));
                request.Append("?start=0&count=250&oauth2_access_token=");
                request.Append(Singleton.Instance.AccessToken);

                var responseString = Utils.MakeRequest(request.ToString(), "GET");
                var xdoc = XDocument.Parse(responseString);
                var xroot = xdoc.Root;
                if (xroot == null)
                    return new LinkedInResponse<IEnumerable<LinkedInLike>>(likes.AsEnumerable(), LinkedInResponseStatus.OK,
                        updateKey);
                likes.AddRange(xroot.Elements("like").Select(Utils.BuildLike));
                return new LinkedInResponse<IEnumerable<LinkedInLike>>(likes.AsEnumerable(), LinkedInResponseStatus.OK,
                    updateKey);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse<IEnumerable<LinkedInLike>>(null, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<IEnumerable<LinkedInLike>>(null, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        internal static LinkedInResponse<IEnumerable<LinkedInComment>> GetAllUpdateComments(string updateKey)
        {
            try
            {
                var comments = new List<LinkedInComment>();
                var request = new StringBuilder(Utils.UPDATE_COMMENTS_URL.Replace("{NETWORK UPDATE KEY}", updateKey));
                request.Append("?start=0&count=250&oauth2_access_token=");
                request.Append(Singleton.Instance.AccessToken);

                var responseString = Utils.MakeRequest(request.ToString(), "GET");
                var xdoc = XDocument.Parse(responseString);
                var xroot = xdoc.Root;
                if (xroot == null)
                    return new LinkedInResponse<IEnumerable<LinkedInComment>>(comments.AsEnumerable(),
                        LinkedInResponseStatus.OK, updateKey);
                comments.AddRange(xroot.Elements("update-comment").Select(Utils.BuildComment));
                return new LinkedInResponse<IEnumerable<LinkedInComment>>(comments.AsEnumerable(),
                    LinkedInResponseStatus.OK, updateKey);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse<IEnumerable<LinkedInComment>>(null, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<IEnumerable<LinkedInComment>>(null, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        internal static IEnumerable<LinkedInGroup> GetSuggestedGroups(string requestUrl, LinkedInGetGroupOptions options)
        {
            var results = new List<LinkedInGroup>();
            var responseString = Utils.MakeRequest(requestUrl, "GET");
            var xdoc = XDocument.Parse(responseString);
            var xroot = xdoc.Root;
            if (xroot == null)
                return results.AsEnumerable();
            results.AddRange(xroot.Elements("group").Select(Utils.BuildGroup));
            return results.AsEnumerable();
        }

        internal static IEnumerable<LinkedInGroup> GetMemberGroups(string requestUrl, LinkedInGetGroupOptions options)
        {
            var results = new List<LinkedInGroup>();
            var responseString = Utils.MakeRequest(requestUrl, "GET");
            var xdoc = XDocument.Parse(responseString);
            var xroot = xdoc.Root;
            if (xroot == null)
                return results.AsEnumerable();
            foreach (var xm in xroot.Elements("group-membership"))
            {
                var xg = xm.Element("group");
                if (xg == null) continue;
                var g = Utils.BuildGroup(xg);
                g.Settings = Utils.BuildGroupSettings(xm);
                results.Add(g);
            }
            return results.AsEnumerable();
        }

        internal static IEnumerable<LinkedInGroupComment> GetPostComments(LinkedInGetGroupPostCommentsOptions options)
        {
            var comments = new List<LinkedInGroupComment>();

            var sb = new StringBuilder(Utils.POSTS_COMMENTS_URL.Replace("{POST_ID}", options.PostId));
            var postParameters = RequestFields.PrepareGroupPostCommentFields(options);
            if (!string.IsNullOrEmpty(postParameters))
                sb.Append(postParameters);
            sb.Append("?");

            var start = 0;
            var fetched = 0;

            while (true)
            {
                var request = new StringBuilder(sb.ToString());
                request.Append("start=");
                request.Append(start);
                request.Append("&");
                request.Append("count=");
                request.Append(10);
                request.Append("&");
                request.Append("oauth2_access_token=");
                request.Append(Singleton.Instance.AccessToken);

                var responseString = Utils.MakeRequest(request.ToString(), "GET");
                var xdoc = XDocument.Parse(responseString);
                var xroot = xdoc.Root;
                if (xroot == null || xroot.Attribute("total") == null || !xroot.Elements("comment").Any())
                    break;

                var total = Convert.ToInt32(xroot.Attribute("total").Value.Trim());

                var count = xroot.Attribute("count") == null
                    ? total
                    : Convert.ToInt32(xroot.Attribute("count").Value.Trim());

                fetched += count;

                comments.AddRange(xroot.Elements("comment").Select(Utils.BuildGroupComment));

                if (fetched >= total)
                    break;

                start += count;
            }

            return comments.AsEnumerable();
        }

        internal static IEnumerable<LinkedInGroupPost> GetGroupPosts(LinkedInGetGroupPostsOptions options)
        {
            var posts = new List<LinkedInGroupPost>();

            var sb = new StringBuilder();
            sb.Append(options.Role == LinkedInGroupPostRole.NotDefined
                ? Utils.GROUP_POSTS_URL.Replace("{GROUP_ID}", options.GroupId)
                : Utils.GROUP_MEMBER_POSTS_URL.Replace("{GROUP_ID}", options.GroupId));
            var postParameters = RequestFields.PrepareGroupPostFields(options);
            if (!string.IsNullOrEmpty(postParameters))
                sb.Append(postParameters);
            sb.Append("?");

            switch (options.Role)
            {
                case LinkedInGroupPostRole.Creator:
                    sb.Append("role=creator&");
                    break;
                case LinkedInGroupPostRole.Commenter:
                    sb.Append("role=commenter&");
                    break;
                case LinkedInGroupPostRole.Follower:
                    sb.Append("role=follower&");
                    break;
            }

            var start = 0;
            var fetched = 0;

            while (true)
            {
                var request = new StringBuilder(sb.ToString());
                request.Append("start=");
                request.Append(start);
                request.Append("&");
                request.Append("count=");
                request.Append(10);
                request.Append("&");
                request.Append("oauth2_access_token=");
                request.Append(Singleton.Instance.AccessToken);

                var responseString = Utils.MakeRequest(request.ToString(), "GET");
                var xdoc = XDocument.Parse(responseString);
                var xroot = xdoc.Root;
                if (xroot == null || xroot.Attribute("total") == null || !xroot.Elements("post").Any())
                    break;

                var total = Convert.ToInt32(xroot.Attribute("total").Value.Trim());

                var count = xroot.Attribute("count") == null
                    ? total
                    : Convert.ToInt32(xroot.Attribute("count").Value.Trim());

                fetched += count;

                posts.AddRange(xroot.Elements("post").Select(Utils.BuildGroupPost));

                if (fetched >= total)
                    break;

                start += count;
            }
            return posts.AsEnumerable();
        }

        internal static LinkedInResponse<bool> RequestJoinGroup(string groupId)
        {
            try
            {
                var sb = new StringBuilder(Utils.GROUP_JOIN_URL.Replace("{GROUP_ID}", groupId));
                sb.Append("?oauth2_access_token=");
                sb.Append(Singleton.Instance.AccessToken);
                const string request =
                    "<group-membership><membership-state><code>member</code></membership-state></group-membership>";
                var statusCode = HttpStatusCode.OK;
                Utils.MakeRequest(sb.ToString(), "PUT", ref statusCode, request);
                return new LinkedInResponse<bool>(statusCode == HttpStatusCode.Created, LinkedInResponseStatus.OK, null);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse(false, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<bool>(false, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        internal static LinkedInResponse<bool> RequestLeaveGroup(string groupId)
        {
            try
            {
                var sb = new StringBuilder(Utils.GROUP_JOIN_URL.Replace("{GROUP_ID}", groupId));
                sb.Append("?oauth2_access_token=");
                sb.Append(Singleton.Instance.AccessToken);
                var statusCode = HttpStatusCode.OK;
                Utils.MakeRequest(sb.ToString(), "DELETE", ref statusCode);
                return new LinkedInResponse<bool>(statusCode == HttpStatusCode.NoContent, LinkedInResponseStatus.OK, null);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse(false, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<bool>(false, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        internal static LinkedInResponse<bool> ChangeGroupSettings(string groupId, LinkedInGroupSettings settings)
        {
            try
            {
                var sb = new StringBuilder(Utils.GROUP_JOIN_URL.Replace("{GROUP_ID}", groupId));
                sb.Append("?oauth2_access_token=");
                sb.Append(Singleton.Instance.AccessToken);
                var request = new StringBuilder("<group-membership><show-group-logo-in-profile>");
                request.Append(settings.ShowGroupLogoInProfile.ToString().ToLower());
                request.Append("</show-group-logo-in-profile><email-digest-frequency><code>");
                switch (settings.EmailDigestFrequency)
                {
                    case LinkedInEmailDigestFrequency.Daily:
                        request.Append("daily");
                        break;
                    case LinkedInEmailDigestFrequency.None:
                        request.Append("none");
                        break;
                    case LinkedInEmailDigestFrequency.Weekly:
                        request.Append("weekly");
                        break;
                }
                request.Append("</code></email-digest-frequency><email-announcements-from-managers>");
                request.Append(settings.EmailAnnouncementsFromManagers.ToString().ToLower());
                request.Append("</email-announcements-from-managers><allow-messages-from-members>");
                request.Append(settings.AllowMessagesFromMembers.ToString().ToLower());
                request.Append("</allow-messages-from-members><email-for-every-new-post>");
                request.Append(settings.EmailForEveryNewPost.ToString().ToLower());
                request.Append("</email-for-every-new-post></group-membership>");

                var statusCode = HttpStatusCode.OK;
                Utils.MakeRequest(sb.ToString(), "PUT", ref statusCode, request.ToString());
                return new LinkedInResponse<bool>(statusCode == HttpStatusCode.OK, LinkedInResponseStatus.OK, null);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse(false, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<bool>(false, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        internal static LinkedInResponse<bool> AddGroupPost(LinkedInGroupPostOptions options)
        {
            try
            {
                var sb = new StringBuilder(Utils.GROUP_POSTS_URL.Replace("{GROUP_ID}", options.GroupId));
                sb.Append("?oauth2_access_token=");
                sb.Append(Singleton.Instance.AccessToken);

                var request = new StringBuilder("<post>");

                request.Append("<title>");
                request.Append(Utils.EscapeXml(options.Title));
                request.Append("</title>");

                request.Append("<summary>");
                request.Append(Utils.EscapeXml(options.Summary));
                request.Append("</summary>");

                if (!string.IsNullOrEmpty(options.SubmittedUrl) && !string.IsNullOrEmpty(options.SubmittedImageUrl) &&
                    !string.IsNullOrEmpty(options.ContentTitle) && !string.IsNullOrEmpty(options.ContentText))
                {
                    request.Append("<content>");

                    request.Append("<submitted-url>");
                    request.Append(Utils.EscapeXml(options.SubmittedUrl));
                    request.Append("</submitted-url>");

                    request.Append("<submitted-image-url>");
                    request.Append(Utils.EscapeXml(options.SubmittedImageUrl));
                    request.Append("</submitted-image-url>");

                    request.Append("<title>");
                    request.Append(Utils.EscapeXml(options.ContentTitle));
                    request.Append("</title>");

                    request.Append("<description>");
                    request.Append(Utils.EscapeXml(options.ContentText));
                    request.Append("</description></content>");
                }
                request.Append("</post>");

                var statusCode = HttpStatusCode.OK;
                Utils.MakeRequest(sb.ToString(), "POST", ref statusCode, request.ToString());
                return new LinkedInResponse<bool>(statusCode == HttpStatusCode.Created, LinkedInResponseStatus.OK, null);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse(false, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<bool>(false, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        internal static LinkedInResponse<bool> LikeUnlikePost(string postId, bool like)
        {
            try
            {
                var sb = new StringBuilder(Utils.POSTS_LIKE_URL.Replace("{POST_ID}", postId));
                sb.Append("?oauth2_access_token=");
                sb.Append(Singleton.Instance.AccessToken);
                var body = like ? "<is-liked>true</is-liked>" : "<is-liked>false</is-liked>";

                var statusCode = HttpStatusCode.OK;
                Utils.MakeRequest(sb.ToString(), "PUT", ref statusCode, body);
                return new LinkedInResponse<bool>(statusCode == HttpStatusCode.NoContent, LinkedInResponseStatus.OK, null);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse(false, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<bool>(false, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        internal static LinkedInResponse<bool> FollowUnfollowPost(string postId, bool follow)
        {
            try
            {
                var sb = new StringBuilder(Utils.POSTS_FOLLOW_URL.Replace("{POST_ID}", postId));
                sb.Append("?oauth2_access_token=");
                sb.Append(Singleton.Instance.AccessToken);
                var body = follow ? "<is-following>true</is-following>" : "<is-following>false</is-following>";

                var statusCode = HttpStatusCode.OK;
                Utils.MakeRequest(sb.ToString(), "PUT", ref statusCode, body);
                return new LinkedInResponse<bool>(statusCode == HttpStatusCode.NoContent, LinkedInResponseStatus.OK, null);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse(false, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<bool>(false, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        internal static LinkedInResponse<bool> CategorizePost(string postId, LinkedInGroupPostFlag flag)
        {
            try
            {
                var sb = new StringBuilder(Utils.POSTS_FLAG_URL.Replace("{POST_ID}", postId));
                sb.Append("?oauth2_access_token=");
                sb.Append(Singleton.Instance.AccessToken);
                var body = flag == LinkedInGroupPostFlag.Promotion ? "<code>promotion</code>" : "<code>job</code>";

                var statusCode = HttpStatusCode.OK;
                Utils.MakeRequest(sb.ToString(), "PUT", ref statusCode, body);
                return new LinkedInResponse<bool>(statusCode == HttpStatusCode.NoContent, LinkedInResponseStatus.OK, null);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse(false, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<bool>(false, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        internal static LinkedInResponse<bool> CommentPost(string postId, string comment)
        {
            try
            {
                var sb = new StringBuilder(Utils.POSTS_COMMENTS_URL.Replace("{POST_ID}", postId));
                sb.Append("?oauth2_access_token=");
                sb.Append(Singleton.Instance.AccessToken);
                var body = new StringBuilder("<comment><text>");
                body.Append(Utils.EscapeXml(comment));
                body.Append("</text></comment>");

                var statusCode = HttpStatusCode.OK;
                Utils.MakeRequest(sb.ToString(), "POST", ref statusCode, body.ToString());
                return new LinkedInResponse<bool>(statusCode == HttpStatusCode.Created, LinkedInResponseStatus.OK, null);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse(false, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<bool>(false, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        internal static LinkedInResponse<bool> DeletePost(string postId)
        {
            try
            {
                var sb = new StringBuilder(Utils.POSTS_URL.Replace("{POST_ID}", postId));
                sb.Append("?oauth2_access_token=");
                sb.Append(Singleton.Instance.AccessToken);

                var statusCode = HttpStatusCode.OK;
                Utils.MakeRequest(sb.ToString(), "DELETE", ref statusCode);
                return new LinkedInResponse<bool>(statusCode == HttpStatusCode.NoContent, LinkedInResponseStatus.OK, null);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse(false, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<bool>(false, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        internal static LinkedInResponse<bool> DeletePostComment(string commentId)
        {
            try
            {
                var sb = new StringBuilder(Utils.COMMENTS_URL.Replace("{COMMENT_ID}", commentId));
                sb.Append("?oauth2_access_token=");
                sb.Append(Singleton.Instance.AccessToken);

                var statusCode = HttpStatusCode.OK;
                Utils.MakeRequest(sb.ToString(), "DELETE", ref statusCode);
                return new LinkedInResponse<bool>(statusCode == HttpStatusCode.NoContent, LinkedInResponseStatus.OK, null);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse(false, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<bool>(false, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        internal static LinkedInResponse<bool> CommentUpdate(string updateKey, string comment)
        {
            try
            {
                var sb = new StringBuilder(Utils.UPDATE_COMMENTS_URL.Replace("{NETWORK UPDATE KEY}", updateKey));
                sb.Append("?oauth2_access_token=");
                sb.Append(Singleton.Instance.AccessToken);
                var body = new StringBuilder("<?xml version='1.0' encoding='UTF-8'?><update-comment><comment>");
                body.Append(Utils.EscapeXml(comment));
                body.Append("</comment></update-comment>");
                var statusCode = HttpStatusCode.OK;
                Utils.MakeRequest(sb.ToString(), "POST", ref statusCode, body.ToString());
                return new LinkedInResponse<bool>(statusCode == HttpStatusCode.Created, LinkedInResponseStatus.OK, null);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse(false, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<bool>(false, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        internal static LinkedInResponse<bool> LikeUnlikeUpdate(string updateKey, bool like)
        {
            try
            {
                var body = like
                    ? "<?xml version='1.0' encoding='UTF-8'?><is-liked>true</is-liked>"
                    : "<?xml version='1.0' encoding='UTF-8'?><is-liked>false</is-liked>";
                var sb = new StringBuilder(Utils.UPDATE_IS_LIKED_URL.Replace("{NETWORK UPDATE KEY}", updateKey));
                sb.Append("?oauth2_access_token=");
                sb.Append(Singleton.Instance.AccessToken);
                var statusCode = HttpStatusCode.OK;
                Utils.MakeRequest(sb.ToString(), "PUT", ref statusCode, body);
                return new LinkedInResponse<bool>(statusCode == HttpStatusCode.Created, LinkedInResponseStatus.OK, null);
            }
            catch (WebException wex)
            {
                return Utils.GetResponse(false, wex, null);
            }
            catch (Exception ex)
            {
                return new LinkedInResponse<bool>(false, LinkedInResponseStatus.OtherException, null, ex);
            }
        }

        /// <summary>
        /// Clears or returns the CompressPath attribute of UriParser to disable URL compacting
        /// </summary>
        /// <param name="getSyntax">MethodInfo</param>
        /// <param name="flagsField">FieldInfo</param>
        /// <param name="switchOn">Flag specified wether to clear or return the attribute</param>
        /// <remarks>Based on post: https://connect.microsoft.com/VisualStudio/feedback/details/386695/system-uri-incorrectly-strips-trailing-dots</remarks>
        private static void setCompressionFlag(MethodInfo getSyntax, FieldInfo flagsField, bool switchOn)
        {
            if (getSyntax == null || flagsField == null) return;
            foreach (var scheme in new[] { "http", "https" })
            {
                var parser = (UriParser)getSyntax.Invoke(null, new object[] { scheme });
                if (parser == null) continue;
                var flagsValue = (int)flagsField.GetValue(parser);
                // Clear the CompressPath attribute to disable URL compacting
                // (see the source code for the internal enum UriSyntaxFlags within the .NET Framework for all the possible values)
                if (!switchOn)
                {
                    if ((flagsValue & 0x800000) != 0)
                        flagsField.SetValue(parser, flagsValue & ~0x800000);
                }
                else
                {
                    if ((flagsValue & 0x800000) == 0)
                        flagsField.SetValue(parser, flagsValue | 0x800000);
                }
            }
        }
    }
}
