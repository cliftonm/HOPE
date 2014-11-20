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
    /// Represents base class for groups post comments retrieving options
    /// </summary>
    public class LinkedInGetGroupPostCommentsOptions
    {
        /// <summary>
        /// Initializes new instance of LinkedInGetGroupPostCommentsOptions
        /// </summary>
        public LinkedInGetGroupPostCommentsOptions()
        {
            CommentOptions = new BitField<LinkedInGroupPostCommentFields>();
        }

        /// <summary>
        /// Gets or sets the fields to be retrieved for each group's post's comment.
        /// </summary>
        public BitField<LinkedInGroupPostCommentFields> CommentOptions { get; private set; }

        /// <summary>
        /// Gets or sets the post's id to retrieve comments for
        /// </summary>
        public string PostId { get; set; }
    }
}
