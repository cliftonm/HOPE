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
using LinkedIn.NET.Members;

namespace LinkedIn.NET.Groups
{
    /// <summary>
    /// Represents LinkedIn group's post's comment object.
    /// </summary>
    public class LinkedInGroupComment
    {
        internal LinkedInGroupComment()
        {
            AvailableAction = new LinkedInBits<LinkedInGroupCommentAction>();
        }

        /// <summary>
        /// Gets comment's ID.
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// Gets comment's text.
        /// </summary>
        public string Text { get; internal set; }

        /// <summary>
        /// Gets person who created the comment.
        /// </summary>
        public LinkedInPerson Creator { get; internal set; }

        /// <summary>
        /// Gets timestamp for when the comment was created.
        /// </summary>
        public DateTime CreationTime { get; internal set; }

        /// <summary>
        /// Gets comment's available actions.
        /// </summary>
        public LinkedInBits<LinkedInGroupCommentAction> AvailableAction { get; private set; }

        /// <summary>
        /// Deletes current comment if user is the comments's creator or marks the post as inappropriate
        /// </summary>
        /// <returns>Request result</returns>
        /// <exception cref="LinkedInInvalidOperationException">Thrown when user attempts to delete the comment which is not marked as available for this action</exception>
        public LinkedInResponse<bool> Delete()
        {
            if (!AvailableAction[LinkedInGroupCommentAction.Delete] && !AvailableAction[LinkedInGroupCommentAction.FlagAsInappropriate])
                throw new LinkedInInvalidOperationException("Deletion of current post is not available");
            return RequestRunner.DeletePostComment(Id);
        }
    }
}
