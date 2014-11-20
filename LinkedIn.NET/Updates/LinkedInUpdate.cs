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
using System.Text;
using System.Xml.Linq;
using LinkedIn.NET.Options;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents base class for LinkedIn update object
    /// </summary>
    public abstract class LinkedInUpdate
    {
        internal LinkedInUpdate()
        {
        }

        private List<LinkedInComment> _Comments = new List<LinkedInComment>();
        private List<LinkedInLike> _Likes = new List<LinkedInLike>();

        /// <summary>
        /// Gets update's date
        /// </summary>
        public DateTime UpdateDate { get; internal set; }

        /// <summary>
        /// Gets update's key
        /// </summary>
        public string UpdateKey { get; internal set; }

        /// <summary>
        /// Gets update's type
        /// </summary>
        public string UpdateType { get; internal set; }

        /// <summary>
        /// Gets value indicating whether update is commentable
        /// </summary>
        public bool IsCommentable { get; internal set; }

        /// <summary>
        /// Gets value indicating whether update is likable
        /// </summary>
        public bool IsLikable { get; internal set; }

        /// <summary>
        /// Gets value indicating whether update is liked
        /// </summary>
        public bool IsLiked { get; internal set; }

        /// <summary>
        /// Gets number of update's comments
        /// </summary>
        public int? NumberOfComments { get; internal set; }

        /// <summary>
        /// Gets number of update's likes
        /// </summary>
        public int? NumberOfLikes { get; internal set; }

        /// <summary>
        /// Gets collection of <see cref="LinkedInLike"/> objects representing the update's likes
        /// </summary>
        public IEnumerable<LinkedInLike> Likes
        {
            get { return IsLikable ? _Likes.AsEnumerable() : null; }
        }

        /// <summary>
        /// Gets collection of <see cref="LinkedInComment"/> objects representing the update's comments
        /// </summary>
        public IEnumerable<LinkedInComment> Comments
        {
            get { return IsCommentable ? _Comments.AsEnumerable() : null; }
        }

        /// <summary>
        /// Adds comment to current update
        /// </summary>
        /// <param name="comment">Comment to add</param>
        /// <returns>Value containing true or false, depending on operation success, and response status</returns>
        /// <exception cref="LinkedInInvalidOperationException">Thrown when current update is not commentable</exception>
        public LinkedInResponse<bool> Comment(string comment)
        {
            try
            {
                if (!IsCommentable)
                    throw new LinkedInInvalidOperationException("Comments are not allowed for this update");
                if (comment.Length > LinkedInShareOptions.MAX_COMMENT_LENGTH)
                    comment = comment.Substring(0, LinkedInShareOptions.MAX_COMMENT_LENGTH);
                var response = RequestRunner.CommentUpdate(UpdateKey, comment);
                var liComment = getLastComment();
                if (liComment != null)
                    _Comments.Add(liComment);
                NumberOfComments++;
                return response;
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
        /// Likes current update
        /// </summary>
        ///  <returns>Value containing true or false, depending on operation success, and response status</returns>
        /// <exception cref="LinkedInInvalidOperationException">Thrown when current update is not likable</exception>
        public LinkedInResponse<bool> Like()
        {
            try
            {
                if (!IsLikable)
                    throw new LinkedInInvalidOperationException("Likes are not allowed for this update");
                var response = RequestRunner.LikeUnlikeUpdate(UpdateKey, true);
                _Likes.Add(new LinkedInLike { Person = Singleton.Instance.CurrentUser });
                NumberOfLikes++;
                return response;
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
        /// Unlikes current update
        /// </summary>
        /// <returns>Value containing true or false, depending on operation success, and response status</returns>
        /// <exception cref="LinkedInInvalidOperationException">Thrown when current update is not likable</exception>
        public LinkedInResponse<bool> Unlike()
        {
            try
            {
                if (!IsLikable)
                    throw new LinkedInInvalidOperationException("Likes are not allowed for this update");
                var response = RequestRunner.LikeUnlikeUpdate(UpdateKey, false);
                var user = Singleton.Instance.CurrentUser;
                _Likes.RemoveAll(l => l.Person.Id == user.Id);
                NumberOfLikes--;
                return response;
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
        /// Loads all update's comments (max 250)
        /// </summary>
        /// <returns>Request result</returns>
        /// <remarks>This is synchronous operation, i.e. the current thread will be suspended until it finishes to load all comments. If you want to load update's comments asynchronously, consider to use <see cref="LinkedInClient.GetUpdateComments"/> function instead</remarks>
        public LinkedInResponse<bool> LoadComments()
        {
            try
            {
                if (_Comments == null)
                    _Comments = new List<LinkedInComment>();
                else
                    _Comments.Clear();
                var response = RequestRunner.GetAllUpdateComments(UpdateKey);
                if (response != null && response.Result != null && response.Status == LinkedInResponseStatus.OK)
                    _Comments.AddRange(response.Result);
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
        /// Loads all update's likes (max 250)
        /// </summary>
        /// <returns>Request result</returns>
        /// <remarks>This is synchronous operation, i.e. the current thread will be suspended until it finishes to load all likes. If you want to load update's likes asynchronously, consider to use <see cref="LinkedInClient.GetUpdateLikes"/> function instead</remarks>
        public LinkedInResponse<bool> LoadLikes()
        {
            try
            {
                if (_Likes == null)
                    _Likes = new List<LinkedInLike>();
                else
                    _Likes.Clear();
                var response = RequestRunner.GetAllUpdateLikes(UpdateKey);
                if (response != null && response.Result != null && response.Status == LinkedInResponseStatus.OK)
                    _Likes.AddRange(response.Result);
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
        /// Allows to replace update's likes with new ones, e.g by those received by <see cref="LinkedInClient.GetUpdateLikes"/> function
        /// </summary>
        /// <param name="likes">Collection of <see cref="LinkedInLike"/> objects representing new update's likes that will replace old ones</param>
        public void ReplaceLikes(IEnumerable<LinkedInLike> likes)
        {
            if (_Likes == null)
                _Likes = new List<LinkedInLike>();
            else
                _Likes.Clear();
            _Likes.AddRange(likes);
        }

        /// <summary>
        /// Allows to replace update's comments with new ones, e.g by those received by <see cref="LinkedInClient.GetUpdateComments"/> function
        /// </summary>
        /// <param name="comments">Collection of <see cref="LinkedInComment"/> objects representing new update's comments that will replace old ones</param>
        public void ReplaceComments(IEnumerable<LinkedInComment> comments)
        {
            if (_Comments == null)
                _Comments = new List<LinkedInComment>();
            else
                _Comments.Clear();
            _Comments.AddRange(comments);
        }

        internal void AddLikes(IEnumerable<LinkedInLike> likes)
        {
            _Likes.AddRange(likes);
        }

        internal void AddComments(IEnumerable<LinkedInComment> comments)
        {
            _Comments.AddRange(comments);
        }

        internal abstract void BuildUpdate(XElement xp);

        internal void SetBaseValues(XElement xp)
        {
            var xe = xp.Element("timestamp");
            if (xe != null)
            {
                UpdateDate = Utils.GetRealDateTime(Convert.ToDouble(xe.Value.Trim()));
            }
            xe = xp.Element("update-key");
            if (xe != null)
            {
                UpdateKey = xe.Value.Trim();
            }
            xe = xp.Element("update-type");
            if (xe != null)
            {
                UpdateType = xe.Value.Trim();
            }
            xe = xp.Element("is-commentable");
            if (xe != null)
            {
                IsCommentable = Convert.ToBoolean(xe.Value.Trim());
            }
            xe = xp.Element("is-likable");
            if (xe != null)
            {
                IsLikable = Convert.ToBoolean(xe.Value.Trim());
            }
            xe = xp.Element("is-liked");
            if (xe != null)
            {
                IsLiked = Convert.ToBoolean(xe.Value.Trim());
            }
            //update comments
            xe = xp.Element("update-comments");
            if (xe != null && xe.Attribute("total") != null &&
                xe.Attribute("total").Value.Trim().Length > 0)
            {
                NumberOfComments = Convert.ToInt32(xe.Attribute("total").Value.Trim());
                AddComments(xe.Elements("update-comment").Select(Utils.BuildComment));
            }
            //update likes
            xe = xp.Element("num-likes");
            if (xe != null && xe.Value.Trim().Length > 0)
            {
                NumberOfLikes = Convert.ToInt32(xe.Value.Trim());
                var xlikes = xp.Element("likes");
                if (xlikes != null)
                {
                    AddLikes(xlikes.Elements("like").Select(Utils.BuildLike));
                }
            }
        }

        private LinkedInComment getLastComment()
        {
            var sb = new StringBuilder(Utils.UPDATE_COMMENTS_URL.Replace("{NETWORK UPDATE KEY}", UpdateKey));
            sb.Append("?oauth2_access_token=");
            sb.Append(Singleton.Instance.AccessToken);
            var requestString = Utils.MakeRequest(sb.ToString(), "GET");
            if (string.IsNullOrEmpty(requestString)) return null;
            var xdoc = XDocument.Parse(requestString);
            if (xdoc.Root == null) return null;
            var xcomments = xdoc.Root.Elements("update-comment");
            xcomments = xcomments.OrderByDescending(x =>
            {
                var xElement = x.Element("sequence-number");
                return xElement != null ? Convert.ToInt32(xElement.Value) : 0;
            });
            return (from xc in xcomments
                    let xp = xc.Element("person")
                    where xp != null
                    let xe = xp.Element("id")
                    where xe != null && xe.Value == Singleton.Instance.CurrentUser.Id
                    select xc).Select(Utils.BuildComment).FirstOrDefault();
        }
    }
}
