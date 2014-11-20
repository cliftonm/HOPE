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
using LinkedIn.NET.Members;
using LinkedIn.NET.Options;

namespace LinkedIn.NET.Groups
{
    /// <summary>
    /// Represents LinkedIn group's post object.
    /// </summary>
    public class LinkedInGroupPost
    {
        internal LinkedInGroupPost()
        {
            AvailableAction = new LinkedInBits<LinkedInGroupPostAction>();
        }

        private List<LinkedInLike> _Likes;
        private List<LinkedInGroupComment> _Comments;

        /// <summary>
        /// Gets post's ID.
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// Gets post's type. Can be one of <see cref="LinkedInGroupPostType"/> enumeration values.
        /// </summary>
        public LinkedInGroupPostType PostType { get; internal set; }

        /// <summary>
        /// Gets post's category. Can be one of <see cref="LinkedInGroupPostCategory"/> enumeration values.
        /// </summary>
        public LinkedInGroupPostCategory Category { get; internal set; }

        /// <summary>
        /// Gets the person who created the post.
        /// </summary>
        public LinkedInPerson Creator { get; internal set; }

        /// <summary>
        /// Gets post's title.
        /// </summary>
        public string Title { get; internal set; }

        /// <summary>
        /// GetsGets post's summary.
        /// </summary>
        public string Summary { get; internal set; }

        /// <summary>
        /// Gets the timestamp for when the post was created.
        /// </summary>
        public DateTime CreationTime { get; internal set; }

        /// <summary>
        /// Gets the value indicating whether the poster is following by currently logged in user.
        /// </summary>
        public bool? IsFollowingByUser { get; internal set; }

        /// <summary>
        /// Gets the value indicating whether the post liked by currently logged in user.
        /// </summary>
        public bool? IsLikedByUser { get; internal set; }

        /// <summary>
        /// Gets the post's available actions.
        /// </summary>
        public LinkedInBits<LinkedInGroupPostAction> AvailableAction { get; private set; }

        /// <summary>
        /// Gets collection of <see cref="LinkedInLike"/> objects representing likes for the post.
        /// </summary>
        public IEnumerable<LinkedInLike> Likes
        {
            get { return _Likes == null ? null : _Likes.AsEnumerable(); }
        }

        /// <summary>
        /// Gets collection of <see cref="LinkedInGroupComment"/> objects representing comments on the post.
        /// </summary>
        public IEnumerable<LinkedInGroupComment> Comments
        {
            get
            {
                return _Comments == null ? null : _Comments.AsEnumerable();
            }
        }

        /// <summary>
        /// Get's <see cref="LinkedInGroupPostAttachment"/> object representing post's attachment.
        /// </summary>
        public LinkedInGroupPostAttachment Attachment { get; internal set; }

        /// <summary>
        /// Gets LinkedIn site URL to the post.
        /// </summary>
        public string SiteGroupPostUrl { get; internal set; }

        /// <summary>
        /// Loads post's comments
        /// </summary>
        /// <param name="options"><see cref="LinkedInGetGroupPostCommentsOptions"/> object representing comments retrieval options</param>
        /// <returns>Request result</returns>
        /// <remarks>This is synchronous operation, i.e. the current thread will be suspended until it finishes to load all comments. If you want to load post's comments asynchronously, consider to use <see cref="LinkedInClient.GetPostComments"/> function instead</remarks>
        public LinkedInResponse<bool> LoadComments(LinkedInGetGroupPostCommentsOptions options)
        {
            try
            {
                if (_Comments == null)
                    _Comments = new List<LinkedInGroupComment>();
                else
                    _Comments.Clear();
                options.PostId = Id;
                _Comments.AddRange(RequestRunner.GetPostComments(options));
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
        /// Allows to replace post's comments with new ones, e.g by those received by <see cref="LinkedInClient.GetPostComments"/> function
        /// </summary>
        /// <param name="comments">Collection of <see cref="LinkedInGroupComment"/> objects representing new post's comments that will replace old ones</param>
        public void ReplaceComments(IEnumerable<LinkedInGroupComment> comments)
        {
            if (_Comments == null)
                _Comments = new List<LinkedInGroupComment>();
            else
                _Comments.Clear();
            _Comments.AddRange(comments);
        }

        /// <summary>
        /// Likes current post
        /// </summary>
        /// <returns>Request result</returns>
        /// <exception cref="LinkedInInvalidOperationException">Thrown when user attempts to like the post which is not marked as available for this action</exception>
        public LinkedInResponse<bool> Like()
        {
            if (!AvailableAction[LinkedInGroupPostAction.Like])
                throw new LinkedInInvalidOperationException("Like action is not available for current post");
            return RequestRunner.LikeUnlikePost(Id, true);
        }

        /// <summary>
        /// Unlikes current post
        /// </summary>
        /// <returns>Request result</returns>
        /// <exception cref="LinkedInInvalidOperationException">Thrown when user attempts to unlike the post which is not marked as available for this action</exception>
        public LinkedInResponse<bool> Unlike()
        {
            if (!AvailableAction[LinkedInGroupPostAction.Like])
                throw new LinkedInInvalidOperationException("Unlike action is not available for current post");
            return RequestRunner.LikeUnlikePost(Id, false);
        }

        /// <summary>
        /// Follows current post
        /// </summary>
        /// <returns>Request result</returns>
        /// <exception cref="LinkedInInvalidOperationException">Thrown when user attempts to follow the post which is not marked as available for this action</exception>
        public LinkedInResponse<bool> Follow()
        {
            if (!AvailableAction[LinkedInGroupPostAction.Follow])
                throw new LinkedInInvalidOperationException("Follow action is not available for current post");
            return RequestRunner.FollowUnfollowPost(Id, true);
        }

        /// <summary>
        /// Follows current post
        /// </summary>
        /// <returns>Request result</returns>
        /// <exception cref="LinkedInInvalidOperationException">Thrown when user attempts to unfollow the post which is not marked as available for this action</exception>
        public LinkedInResponse<bool> Unfollow()
        {
            if (!AvailableAction[LinkedInGroupPostAction.Follow])
                throw new LinkedInInvalidOperationException("Unfollow action is not available for current post");
            return RequestRunner.FollowUnfollowPost(Id, false);
        }

        /// <summary>
        /// Categorizes current post
        /// </summary>
        /// <param name="flag">Value indicating how the current post should be categorized</param>
        /// <returns>Request result</returns>
        /// <exception cref="LinkedInInvalidOperationException">Thrown when user attempts to categorize the post which is not marked as available for this action</exception>
        public LinkedInResponse<bool> Categorize(LinkedInGroupPostFlag flag)
        {
            if (!AvailableAction[LinkedInGroupPostAction.CategorizeAsPromotion] && flag == LinkedInGroupPostFlag.Promotion)
                throw new LinkedInInvalidOperationException("Specifying promotion flag is not available for current post");
            if (!AvailableAction[LinkedInGroupPostAction.CategorizeAsJob] && flag == LinkedInGroupPostFlag.Job)
                throw new LinkedInInvalidOperationException("Specifying job flag is not available for current post");

            return RequestRunner.CategorizePost(Id, flag);
        }

        /// <summary>
        /// Adds comment to current post
        /// </summary>
        /// <param name="comment">Comment's text</param>
        /// <returns>Request result</returns>
        /// <exception cref="LinkedInInvalidOperationException">Thrown when user attempts to add comment to the post which is not marked as available for this action</exception>
        public LinkedInResponse<bool> Comment(string comment)
        {
            if (!AvailableAction[LinkedInGroupPostAction.AddComment])
                throw new LinkedInInvalidOperationException("Adding comments is not available for current post");
            return RequestRunner.CommentPost(Id, comment);
        }

        /// <summary>
        /// Deletes current post if user is the post's creator or marks the post as inappropriate
        /// </summary>
        /// <returns>Request result</returns>
        /// <exception cref="LinkedInInvalidOperationException">Thrown when user attempts to delete the post which is not marked as available for this action</exception>
        public LinkedInResponse<bool> Delete()
        {
            if (!AvailableAction[LinkedInGroupPostAction.FlagAsInappropriate])
                throw new LinkedInInvalidOperationException("Deletion of current post is not available");
            return RequestRunner.DeletePost(Id);
        }

        internal void AddLikes(IEnumerable<LinkedInLike> likes)
        {
            if (_Likes == null) _Likes = new List<LinkedInLike>();
            _Likes.AddRange(likes);
        }
    }
}
