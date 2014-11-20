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
using LinkedIn.NET.Options;

namespace LinkedIn.NET.Groups
{
    /// <summary>
    /// Represents LinkedIn group object.
    /// </summary>
    public class LinkedInGroup
    {
        internal LinkedInGroup()
        {
            AvailableAction = new LinkedInBits<LinkedInGroupAction>();
        }

        private List<LinkedInGroupPost> _Posts;
        private List<LinkedInGroupCategoryCount> _CountsByCategory;

        /// <summary>
        /// Gets groups's ID.
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// Gets groups's name.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets groups's short description.
        /// </summary>
        public string ShortDescription { get; internal set; }

        /// <summary>
        /// Gets groups's full description.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Gets group's relationships to user. Can be one of <see cref="LinkedInGroupRelationship"/> enumeration values
        /// </summary>
        public LinkedInGroupRelationship MembershipState { get; internal set; }

        /// <summary>
        /// Gets the group's available actions.
        /// </summary>
        public LinkedInBits<LinkedInGroupAction> AvailableAction { get; private set; }

        /// <summary>
        /// Gets collection of <see cref="LinkedInGroupPost"/> objects representing group's posts.
        /// </summary>
        public IEnumerable<LinkedInGroupPost> Posts
        {
            get { return _Posts == null ? null : _Posts.AsEnumerable(); }
        }

        /// <summary>
        /// Gets collection of <see cref="LinkedInGroupCategoryCount"/> objects representing group's count-for-category objects.
        /// </summary>
        public IEnumerable<LinkedInGroupCategoryCount> CountsByCategory
        {
            get
            {
                return _CountsByCategory == null ? null : _CountsByCategory.AsEnumerable();
            }
        }

        /// <summary>
        /// Gets value indicating whether group is open for non-members.
        /// </summary>
        public bool? IsOpenToNonMembers { get; internal set; }

        /// <summary>
        /// Gets value indicating group's category. Can be one of <see cref="LinkedInGroupCategory"/> enumeration values.
        /// </summary>
        public LinkedInGroupCategory Category { get; internal set; }

        /// <summary>
        /// Gets the external website for the group.
        /// </summary>
        public string WebSiteUrl { get; internal set; }

        /// <summary>
        /// Gets the LinkedIn site URL for the group.
        /// </summary>
        public string SiteGroupUrl { get; internal set; }

        /// <summary>
        /// Gets the language locale of the group.
        /// </summary>
        public string Locale { get; internal set; }

        /// <summary>
        /// Gets group's country.
        /// </summary>
        public string LocationCountry { get; internal set; }

        /// <summary>
        /// Gets group's postal code.
        /// </summary>
        public string LocationPostalCode { get; internal set; }

        /// <summary>
        /// Gets value indicating whether members are allowed to invite other members to join.
        /// </summary>
        public bool? AllowMembersInvite { get; internal set; }

        /// <summary>
        /// Gets small logo for the group, to be used when representing the group on other sites.
        /// </summary>
        public string SmallLogoUrl { get; internal set; }

        /// <summary>
        /// Gets large logo for the group, to be used when representing the group on other sites.
        /// </summary>
        public string LargeLogoUrl { get; internal set; }

        /// <summary>
        /// Gets number of members of the group.
        /// </summary>
        public int? NumberOfMembers { get; internal set; }

        /// <summary>
        /// Gets <see cref="LinkedInGroupSettings"/> object representing group's settings
        /// </summary>
        public LinkedInGroupSettings Settings { get; internal set; }

        /// <summary>
        /// Loads group's posts
        /// </summary>
        /// <param name="options"><see cref="LinkedInGetGroupPostsOptions"/> object representing posts retrieval options</param>
        /// <returns>Request result</returns>
        /// <remarks>This is synchronous operation, i.e. the current thread will be suspended until it finishes to load all posts. If you want to load group's posts asynchronously, consider to use <see cref="LinkedInClient.GetGroupPosts"/> function instead</remarks>
        public LinkedInResponse<bool> LoadPosts(LinkedInGetGroupPostsOptions options)
        {
            try
            {
                if (_Posts == null)
                    _Posts = new List<LinkedInGroupPost>();
                else
                    _Posts.Clear();
                options.GroupId = Id;
                _Posts.AddRange(RequestRunner.GetGroupPosts(options));
                return new LinkedInResponse<bool>(true, LinkedInResponseStatus.OK, null);
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
        /// Allows to replace group's posts with new ones, e.g by those received by <see cref="LinkedInClient.GetGroupPosts"/> function
        /// </summary>
        /// <param name="posts">Collection of <see cref="LinkedInGroupPost"/> objects representing new group's posts that will replace old ones</param>
        public void ReplacePosts(IEnumerable<LinkedInGroupPost> posts)
        {
            if (_Posts == null)
                _Posts = new List<LinkedInGroupPost>();
            else
                _Posts.Clear();
            _Posts.AddRange(posts);
        }

        /// <summary>
        /// Sends request to join current group
        /// </summary>
        /// <returns>Request result</returns>
        /// <exception cref="LinkedInAlreadyMemberException">Thrown if user is already member of the group</exception>
        public LinkedInResponse<bool> Join()
        {
            if (MembershipState == LinkedInGroupRelationship.NonMember)
                return RequestRunner.RequestJoinGroup(Id);
            throw new LinkedInAlreadyMemberException("The user is already member of group " + Name);
        }

        /// <summary>
        /// Sends request to leave current group
        /// </summary>
        /// <returns>Request result</returns>
        /// <exception cref="LinkedInAlreadyMemberException">Thrown if user is not a member of the group</exception>
        public LinkedInResponse<bool> Leave()
        {
            if (MembershipState != LinkedInGroupRelationship.NonMember)
                return RequestRunner.RequestLeaveGroup(Id);
            throw new LinkedInIsNotMemberException("The user is not a member of group " + Name);
        }

        /// <summary>
        /// Saves group's settings
        /// </summary>
        /// <returns>Request result</returns>
        /// <exception cref="LinkedInAlreadyMemberException">Thrown if user is not a member of the group</exception>
        public LinkedInResponse<bool> SaveSettings()
        {
            if (MembershipState != LinkedInGroupRelationship.NonMember)
                return RequestRunner.ChangeGroupSettings(Id, Settings);
            throw new LinkedInIsNotMemberException("The user is not a member of group " + Name);
        }

        /// <summary>
        /// Adds new post to group
        /// </summary>
        /// <param name="options">Value containing post's details</param>
        /// <returns>Request result</returns>
        /// <exception cref="LinkedInMissingParameterException">Thrown when any of post's required parameters is null or empty string</exception>
        public LinkedInResponse<bool> AddPost(LinkedInGroupPostOptions options)
        {
            options.GroupId = Id;
            if (string.IsNullOrEmpty(options.Title))
                throw new LinkedInMissingParameterException("Post title cannot be empty", "Title");
            if (string.IsNullOrEmpty(options.Summary))
                throw new LinkedInMissingParameterException("Post summary cannot be empty", "Summary");
            var contentPresented = Utils.IsAnyString(options.ContentTitle, options.ContentText, options.SubmittedUrl,
                options.SubmittedImageUrl);
            if (contentPresented && string.IsNullOrEmpty(options.ContentTitle))
                throw new LinkedInMissingParameterException("Post content title cannot be empty", "ContentTitle");
            if (contentPresented && string.IsNullOrEmpty(options.ContentText))
                throw new LinkedInMissingParameterException("Post content text cannot be empty", "ContentText");
            if (contentPresented && string.IsNullOrEmpty(options.SubmittedUrl))
                throw new LinkedInMissingParameterException("Submitted URL cannot be empty", "SubmittedUrl");
            if (contentPresented && string.IsNullOrEmpty(options.SubmittedImageUrl))
                throw new LinkedInMissingParameterException("Submitted image URL cannot be empty", "SubmittedImageUrl");

            return RequestRunner.AddGroupPost(options);
        }

        internal void AddCountsByCategory(IEnumerable<LinkedInGroupCategoryCount> counts)
        {
            if (_CountsByCategory == null) _CountsByCategory = new List<LinkedInGroupCategoryCount>();
            _CountsByCategory.AddRange(counts);
        }
    }
}
