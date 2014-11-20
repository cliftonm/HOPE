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
using System.Text;
using LinkedIn.NET.Options;

namespace LinkedIn.NET
{
    internal static class RequestFields
    {
        private const string SPACE = "%20";

        internal static string PrepareSearchParams(LinkedInSearchOptions options)
        {
            var sb = new StringBuilder();
            if (options.Keywords.Count > 0)
            {
                var keywords = string.Join(SPACE, options.Keywords);
                sb.Append("keywords=");
                sb.Append(keywords.Replace("'", "%5C").Replace(" ",SPACE));
                sb.Append("&");
            }
            if (!string.IsNullOrEmpty(options.FirstName))
            {
                sb.Append("first-name=");
                sb.Append(options.FirstName.Replace("'", "%5C").Replace(" ",SPACE));
                sb.Append("&");
            }
            if (!string.IsNullOrEmpty(options.LastName))
            {
                sb.Append("last-name=");
                sb.Append(options.LastName.Replace("'", "%5C").Replace(" ",SPACE));
                sb.Append("&");
            }
            if (!string.IsNullOrEmpty(options.CompanyName))
            {
                sb.Append("company-name=");
                sb.Append(options.CompanyName.Replace("'", "%5C").Replace(" ",SPACE));
                sb.Append("&");
                if (options.CurrentCompany.HasValue)
                {
                    sb.Append(options.CurrentCompany.Value ? "current-company=true" : "current-company=false");
                    sb.Append("&");
                }
            }
            if (!string.IsNullOrEmpty(options.Title))
            {
                sb.Append("title=");
                sb.Append(options.Title.Replace("'", "%5C").Replace(" ",SPACE));
                sb.Append("&");
                if (options.CurrentTitle.HasValue)
                {
                    sb.Append(options.CurrentTitle.Value ? "current-title=true" : "current-title=false");
                    sb.Append("&");
                }
            }
            if (!string.IsNullOrEmpty(options.SchoolName))
            {
                sb.Append("school-name=");
                sb.Append(options.SchoolName.Replace("'", "%5C").Replace(" ",SPACE));
                sb.Append("&");
                if (options.CurrentSchool.HasValue)
                {
                    sb.Append(options.CurrentSchool.Value ? "current-school=true" : "current-school=false");
                    sb.Append("&");
                }
            }
            if (options.Country != LinkedInCountries.None)
            {
                if (Utils.CountryCodes.ContainsKey(options.Country))
                {
                    sb.Append("country-code=");
                    sb.Append(Utils.CountryCodes[options.Country]);
                    sb.Append("&");
                    if (!string.IsNullOrEmpty(options.PostalCode))
                    {
                        sb.Append("postal-code=");
                        sb.Append(options.PostalCode.Replace("'", "%5C").Replace(" ",SPACE));
                        sb.Append("&");
                    }
                    if (options.Distance.HasValue)
                    {
                        sb.Append("distance=");
                        sb.Append(options.Distance.Value);
                        sb.Append("&");
                    }
                }
            }
            return sb.ToString();
        }

        internal static string PrepareBucketsParams(LinkedInSearchOptions options)
        {
            var sb = new StringBuilder();
            if (options.FacetFields.HasValues)
            {
                sb.Append(":(");
                if (options.FacetFields[LinkedInFacetFields.Code])
                    sb.Append("code,");
                if (options.FacetFields[LinkedInFacetFields.Name])
                    sb.Append("name,");
                if (options.BucketFields.HasValues)
                {
                    sb.Append("buckets:(");
                    if (options.BucketFields[LinkedInBucketFields.Code])
                        sb.Append("code,");
                    if (options.BucketFields[LinkedInBucketFields.Name])
                        sb.Append("name,");
                    if (options.BucketFields[LinkedInBucketFields.Count])
                        sb.Append("count,");
                    if (options.BucketFields[LinkedInBucketFields.Selected])
                        sb.Append("selected,");
                    sb.Length -= 1;
                    sb.Append(")");
                }
                sb.Append(")");
            }
            return sb.ToString();
        }

        internal static string PrepareFacetsParams(LinkedInSearchOptions options)
        {
            var sb = new StringBuilder();
            if (options.FacetTypes != LinkedInFacetTypes.None)
            {
                sb.Append("facets=");
                if ((options.FacetTypes & LinkedInFacetTypes.Location) == LinkedInFacetTypes.Location)
                    sb.Append("location,");
                if ((options.FacetTypes & LinkedInFacetTypes.Network) == LinkedInFacetTypes.Network)
                    sb.Append("network,");
                if ((options.FacetTypes & LinkedInFacetTypes.Industry) == LinkedInFacetTypes.Industry)
                    sb.Append("industry,");
                if ((options.FacetTypes & LinkedInFacetTypes.CurrentCompany) == LinkedInFacetTypes.CurrentCompany)
                    sb.Append("current-company,");
                if ((options.FacetTypes & LinkedInFacetTypes.PastCompany) == LinkedInFacetTypes.PastCompany)
                    sb.Append("past-company,");
                if ((options.FacetTypes & LinkedInFacetTypes.Language) == LinkedInFacetTypes.Language)
                    sb.Append("language,");
                if ((options.FacetTypes & LinkedInFacetTypes.School) == LinkedInFacetTypes.School)
                    sb.Append("school,");
                sb.Length -= 1;
                if (options.FacetLocationValues.Count > 0)
                {
                    sb.Append("&facet=location,");
                    sb.Append(string.Join(",", options.FacetLocationValues));
                }
                if (options.FacetNetworkValues != LinkedInFacetNetwork.None)
                {
                    sb.Append("&facet=network,");
                    if ((options.FacetNetworkValues & LinkedInFacetNetwork.FirstDegree) ==
                        LinkedInFacetNetwork.FirstDegree)
                    {
                        sb.Append("F,");
                    }
                    if ((options.FacetNetworkValues & LinkedInFacetNetwork.SecondDegree) ==
                        LinkedInFacetNetwork.SecondDegree)
                    {
                        sb.Append("S,");
                    }
                    if ((options.FacetNetworkValues & LinkedInFacetNetwork.InsideGroup) ==
                        LinkedInFacetNetwork.InsideGroup)
                    {
                        sb.Append("A,");
                    }
                    if ((options.FacetNetworkValues & LinkedInFacetNetwork.OutOfNetwork) ==
                        LinkedInFacetNetwork.OutOfNetwork)
                    {
                        sb.Append("O,");
                    }
                    sb.Length -= 1;
                }
                if (options.FacetIndustryValues.Count > 0)
                {
                    sb.Append("&facet=industry,");
                    sb.Append(string.Join(",", options.FacetIndustryValues));
                }
                if (options.FacetCurrentCompanyValues.Count > 0)
                {
                    sb.Append("&facet=current-company,");
                    sb.Append(string.Join(",", options.FacetCurrentCompanyValues));
                }
                if (options.FacetPastCompanyValues.Count > 0)
                {
                    sb.Append("&facet=past-company,");
                    sb.Append(string.Join(",", options.FacetPastCompanyValues));
                }
                if (options.FacetSchoolValues.Count > 0)
                {
                    sb.Append("&facet=school,");
                    sb.Append(string.Join(",", options.FacetSchoolValues));
                }
                if (options.FacetLanguageValues.Count > 0)
                {
                    sb.Append("&facet=language,");
                    foreach (var f in options.FacetLanguageValues)
                    {
                        switch (f)
                        {
                            case LinkedInFacetLanguage.English:
                                sb.Append("en,");
                                break;
                            case LinkedInFacetLanguage.Russian:
                                sb.Append("ru,");
                                break;
                            case LinkedInFacetLanguage.French:
                                sb.Append("fr,");
                                break;
                            case LinkedInFacetLanguage.German:
                                sb.Append("de,");
                                break;
                            case LinkedInFacetLanguage.Italian:
                                sb.Append("it,");
                                break;
                            case LinkedInFacetLanguage.Portuguese:
                                sb.Append("pt,");
                                break;
                            case LinkedInFacetLanguage.Spanish:
                                sb.Append("es,");
                                break;
                            case LinkedInFacetLanguage.Others:
                                sb.Append("_o,");
                                break;
                        }
                    }
                    sb.Length -= 1;
                }
                sb.Append("&");
            }
            var result = sb.ToString().Replace(",", "%2C").Replace(":", "%3A");
            return result;
        }

        internal static string PrepareMemberFields(LinkedInGetMemberOptions options)
        {
            var fields = new StringBuilder();

            var addFields = options.BasicProfileOptions.HasValues | options.EmailProfileOptions.HasValues |
                                options.FullProfileOptions.HasValues;
            if (addFields)
            {
                fields.Append(":(");
            }

            if (options.BasicProfileOptions.HasValues)
            {
                if (options.BasicProfileOptions[LinkedInBasicProfileFields.Id])
                    fields.Append("id,");
                if (options.BasicProfileOptions[LinkedInBasicProfileFields.FirstName])
                    fields.Append("first-name,");
                if (options.BasicProfileOptions[LinkedInBasicProfileFields.LastName])
                    fields.Append("last-name,");
                if (options.BasicProfileOptions[LinkedInBasicProfileFields.MaidenName])
                    fields.Append("maiden-name,");
                if (options.BasicProfileOptions[LinkedInBasicProfileFields.FormattedName])
                    fields.Append("formatted-name,");
                if (options.BasicProfileOptions[LinkedInBasicProfileFields.PhoneticFirstName])
                    fields.Append("phonetic-first-name,");
                if (options.BasicProfileOptions[LinkedInBasicProfileFields.PhoneticLastName])
                    fields.Append("phonetic-last-name,");
                if (options.BasicProfileOptions[LinkedInBasicProfileFields.FormattedPhoneticName])
                    fields.Append("formatted-phonetic-name,");
                if (options.BasicProfileOptions[LinkedInBasicProfileFields.Headline])
                    fields.Append("headline,");
                if (options.BasicProfileOptions[LinkedInBasicProfileFields.Location])
                    fields.Append("location,");
                if (options.BasicProfileOptions[LinkedInBasicProfileFields.Industry])
                    fields.Append("industry,");
                if (options.BasicProfileOptions[LinkedInBasicProfileFields.Distance])
                    fields.Append("distance,");
                if (options.BasicProfileOptions[LinkedInBasicProfileFields.RelationToViewer])
                    fields.Append("relation-to-viewer,");
                if (options.BasicProfileOptions[LinkedInBasicProfileFields.CurrentShare])
                    fields.Append("current-share,");
                if (options.BasicProfileOptions[LinkedInBasicProfileFields.NumConnections])
                    fields.Append("num-connections,");
                if (options.BasicProfileOptions[LinkedInBasicProfileFields.NumConnectionsCapped])
                    fields.Append("num-connections-capped,");
                if (options.BasicProfileOptions[LinkedInBasicProfileFields.Summary])
                    fields.Append("summary,");
                if (options.BasicProfileOptions[LinkedInBasicProfileFields.Specialities])
                    fields.Append("specialties,");
                if (options.BasicProfileOptions[LinkedInBasicProfileFields.Positions])
                    fields.Append("positions:(id,title,summary,start-date,end-date,is-current,company:(id,name,type,size,industry,ticker)),");
                if (options.BasicProfileOptions[LinkedInBasicProfileFields.PictureUrl])
                    fields.Append("picture-url,");
                if (options.BasicProfileOptions[LinkedInBasicProfileFields.SiteStandardProfileRequest])
                    fields.Append("site-standard-profile-request,");
                if (options.BasicProfileOptions[LinkedInBasicProfileFields.ApiStandardProfileRequest])
                    fields.Append("api-standard-profile-request,");
                if (options.BasicProfileOptions[LinkedInBasicProfileFields.PublicProfileUrl])
                    fields.Append("public-profile-url,");
            }
            if (options.EmailProfileOptions.HasValues)
            {
                if (options.EmailProfileOptions[LinkedInEmailProfileFields.EmailAddress])
                    fields.Append("email-address,");
            }
            if (options.FullProfileOptions.HasValues)
            {
                if (options.FullProfileOptions[LinkedInFullProfileFields.LastModifiedTimestamp])
                    fields.Append("last-modified-timestamp,");
                if (options.FullProfileOptions[LinkedInFullProfileFields.ProposalComments])
                    fields.Append("proposal-comments,");
                if (options.FullProfileOptions[LinkedInFullProfileFields.Associations])
                    fields.Append("associations,");
                if (options.FullProfileOptions[LinkedInFullProfileFields.Interests])
                    fields.Append("interests,");
                if (options.FullProfileOptions[LinkedInFullProfileFields.Publications])
                    fields.Append("publications:(id,title,publisher,authors,date,url,summary),");
                if (options.FullProfileOptions[LinkedInFullProfileFields.Patents])
                    fields.Append("patents:(id,title,summary,number,status,office,inventors,date,url),");
                if (options.FullProfileOptions[LinkedInFullProfileFields.Languages])
                    fields.Append("languages:(id,language,proficiency),");
                if (options.FullProfileOptions[LinkedInFullProfileFields.Skills])
                    fields.Append("skills,");
                if (options.FullProfileOptions[LinkedInFullProfileFields.Certifications])
                    fields.Append("certifications:(id,name,authority,number,start-date,end-date),");
                if (options.FullProfileOptions[LinkedInFullProfileFields.Educations])
                    fields.Append("educations,");
                if (options.FullProfileOptions[LinkedInFullProfileFields.Courses])
                    fields.Append("courses,");
                if (options.FullProfileOptions[LinkedInFullProfileFields.Volunteer])
                    fields.Append("volunteer:(volunteer-experiences:(id,role,organization,cause)),");
                if (options.FullProfileOptions[LinkedInFullProfileFields.ThreeCurrentPositions])
                    fields.Append("three-current-positions,");
                if (options.FullProfileOptions[LinkedInFullProfileFields.ThreeLastPositions])
                    fields.Append("three-past-positions,");
                if (options.FullProfileOptions[LinkedInFullProfileFields.NumRecommenders])
                    fields.Append("num-recommenders,");
                if (options.FullProfileOptions[LinkedInFullProfileFields.RecommendationsReceived])
                    fields.Append("recommendations-received,");
                if (options.FullProfileOptions[LinkedInFullProfileFields.MfeedRssUrl])
                    fields.Append("mfeed-rss-url,");
                if (options.FullProfileOptions[LinkedInFullProfileFields.Following])
                    fields.Append("following,");
                if (options.FullProfileOptions[LinkedInFullProfileFields.JobBookmarks])
                    fields.Append("job-bookmarks,");
                if (options.FullProfileOptions[LinkedInFullProfileFields.Suggestions])
                    fields.Append("suggestions,");
                if (options.FullProfileOptions[LinkedInFullProfileFields.DateOfBirth])
                    fields.Append("date-of-birth,");
                if (options.FullProfileOptions[LinkedInFullProfileFields.MemberUrlResources])
                    fields.Append("member-url-resources,");
                if (options.FullProfileOptions[LinkedInFullProfileFields.RelatedProfileViews])
                    fields.Append("related-profile-views,");
            }

            if (addFields)
            {
                if (fields.Length > 2) fields.Length -= 1;
                fields.Append(")");
            }

            return fields.ToString();
        }

        internal static string PrepareGetUpdatesFields(LinkedInGetUpdatesOptions options)
        {
            var unixDate = new DateTime(1970, 1, 1);
            var sb = new StringBuilder();

            //updates scope
            if (options.Parameters.GetBy == LinkedInGetMemberBy.Self)
            {
                if (options.UpdateScope == LinkedInUpdateScope.Self || options.UpdateScope == LinkedInUpdateScope.All)
                    sb.Append("scope=self&");
            }
            else
            {
                //always add scope=self when getting updates by memeber id or member URL
                sb.Append("scope=self&");
            }
            //updates type
            if ((options.UpdateType & LinkedInUpdateType.ApplicationUpdate) == LinkedInUpdateType.ApplicationUpdate)
                sb.Append("type=APPS&");
            if ((options.UpdateType & LinkedInUpdateType.CompanyFollowUpdate) == LinkedInUpdateType.CompanyFollowUpdate)
                sb.Append("type=CMPY&");
            if ((options.UpdateType & LinkedInUpdateType.ConnectionUpdate) == LinkedInUpdateType.ConnectionUpdate)
                sb.Append("type=CONN&");
            if ((options.UpdateType & LinkedInUpdateType.PostedJobUpdate) == LinkedInUpdateType.PostedJobUpdate)
                sb.Append("type=JOBS&");
            if ((options.UpdateType & LinkedInUpdateType.JoinedGroupUpdate) == LinkedInUpdateType.JoinedGroupUpdate)
                sb.Append("type=JGRP&");
            if ((options.UpdateType & LinkedInUpdateType.ChangedPictureUpdate) == LinkedInUpdateType.ChangedPictureUpdate)
                sb.Append("type=PICT&");
            if ((options.UpdateType & LinkedInUpdateType.ExtendedProfileUpdate) == LinkedInUpdateType.ExtendedProfileUpdate)
                sb.Append("type=PRFX&");
            if ((options.UpdateType & LinkedInUpdateType.RecommendationUpdate) == LinkedInUpdateType.RecommendationUpdate)
                sb.Append("type=RECU&");
            if ((options.UpdateType & LinkedInUpdateType.ChangedProfileUpdate) == LinkedInUpdateType.ChangedProfileUpdate)
                sb.Append("type=PRFU&");
            if ((options.UpdateType & LinkedInUpdateType.SharedItemUpdate) == LinkedInUpdateType.SharedItemUpdate)
                sb.Append("type=SHAR&");
            if ((options.UpdateType & LinkedInUpdateType.StatusUpdate) == LinkedInUpdateType.StatusUpdate)
                sb.Append("type=STAT&");
            if ((options.UpdateType & LinkedInUpdateType.ViralUpdate) == LinkedInUpdateType.ViralUpdate)
                sb.Append("type=VIRL&");
            //updates start
            if (options.UpdateStart.HasValue)
            {
                sb.Append("start=");
                sb.Append(options.UpdateStart.Value);
                sb.Append("&");
            }
            //updates count
            if (options.UpdateCount.HasValue)
            {
                sb.Append("count=");
                sb.Append(options.UpdateCount.Value);
                sb.Append("&");
            }
            //updates after
            if (options.After.HasValue)
            {
                var mseconds = (options.After.Value - unixDate).Milliseconds;
                sb.Append("after=");
                sb.Append(mseconds);
                sb.Append("&");
            }
            //updates before
            if (options.Before.HasValue)
            {
                var mseconds = (options.Before.Value - unixDate).Milliseconds;
                sb.Append("before=");
                sb.Append(mseconds);
                sb.Append("&");
            }
            //updates show hidden members
            if (options.ShowHiddenMembers.HasValue)
            {
                sb.Append("show-hidden-members=true&");
            }

            return sb.ToString();
        }

        internal static string PrepareGroupFields(LinkedInGetGroupOptions options)
        {
            var sb = new StringBuilder();
            var go = options.GroupOptions;

            if (go.HasValues)
            {
                sb.Append(":(");
                if (go[LinkedInGroupFields.Id])
                    sb.Append("id,");
                if (go[LinkedInGroupFields.Name])
                    sb.Append("name,");
                if (go[LinkedInGroupFields.ShortDescription])
                    sb.Append("short-description,");
                if (go[LinkedInGroupFields.Description])
                    sb.Append("description,");
                if (go[LinkedInGroupFields.RelationToViewer])
                    sb.Append("relation-to-viewer,");
                if (go[LinkedInGroupFields.CountsByCategory])
                    sb.Append("counts-by-category,");
                if (go[LinkedInGroupFields.IsOpenToNonMembers])
                    sb.Append("is-open-to-non-members,");
                if (go[LinkedInGroupFields.Category])
                    sb.Append("category,");
                if (go[LinkedInGroupFields.WebSiteUrl])
                    sb.Append("website-url,");
                if (go[LinkedInGroupFields.SiteGroupUrl])
                    sb.Append("site-group-url,");
                if (go[LinkedInGroupFields.Locale])
                    sb.Append("locale,");
                if (go[LinkedInGroupFields.Location])
                    sb.Append("location,");
                if (go[LinkedInGroupFields.AllowMembersInvite])
                    sb.Append("allow-member-invites,");
                if (go[LinkedInGroupFields.SmallLogoUrl])
                    sb.Append("small-logo-url,");
                if (go[LinkedInGroupFields.LargeLogoUrl])
                    sb.Append("large-logo-url,");
                if (go[LinkedInGroupFields.NumberOfMembers])
                    sb.Append("num-members,");
                sb.Length -= 1;
                sb.Append(")");
            }

            return sb.ToString();
        }

        internal static string PrepareGroupPostFields(LinkedInGetGroupPostsOptions options)
        {
            var sb = new StringBuilder();
            var gp = options.PostOptions;
            if (!gp.HasValues) return sb.ToString();
            sb.Append(":(");
            if (gp[LinkedInGroupPostFields.Id])
                sb.Append("id,");
            if (gp[LinkedInGroupPostFields.PostType])
                sb.Append("type,");
            if (gp[LinkedInGroupPostFields.Category])
                sb.Append("category,");
            if (gp[LinkedInGroupPostFields.Creator])
                sb.Append("creator,");
            if (gp[LinkedInGroupPostFields.Title])
                sb.Append("title,");
            if (gp[LinkedInGroupPostFields.Summary])
                sb.Append("summary,");
            if (gp[LinkedInGroupPostFields.CreationTime])
                sb.Append("creation-timestamp,");
            if (gp[LinkedInGroupPostFields.RelationToViewer])
                sb.Append("relation-to-viewer,");
            if (gp[LinkedInGroupPostFields.Likes])
                sb.Append("likes,");
            if (gp[LinkedInGroupPostFields.Attachment])
                sb.Append("attachment,");
            if (gp[LinkedInGroupPostFields.SiteGroupPostUrl])
                sb.Append("site-group-post-url,");
            sb.Length -= 1;
            sb.Append(")");
            return sb.ToString();
        }

        internal static string PrepareGroupPostCommentFields(LinkedInGetGroupPostCommentsOptions options)
        {
            var sb = new StringBuilder();
            var gc = options.CommentOptions;
            if (!gc.HasValues) return sb.ToString();
            sb.Append(":(");
            if (gc[LinkedInGroupPostCommentFields.Id])
                sb.Append("id,");
            if (gc[LinkedInGroupPostCommentFields.Text])
                sb.Append("text,");
            if (gc[LinkedInGroupPostCommentFields.Creator])
                sb.Append("creator,");
            if (gc[LinkedInGroupPostCommentFields.CreationTime])
                sb.Append("creation-timestamp,");
            if (gc[LinkedInGroupPostCommentFields.RelationToViewer])
                sb.Append("relation-to-viewer,");
            sb.Length -= 1;
            sb.Append(")");
            return sb.ToString();
        }
    }
}
