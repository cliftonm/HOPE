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
    /// Represents base class for groups posts retrieving options
    /// </summary>
    public class LinkedInGetGroupPostsOptions
    {
        /// <summary>
        /// Initializes new instance of LinkedInGetGroupPostsOptions
        /// </summary>
        public LinkedInGetGroupPostsOptions()
        {
            PostOptions = new BitField<LinkedInGroupPostFields>();
        }

        /// <summary>
        /// Gets or sets the fields to be retrieved for each group's post
        /// </summary>
        public BitField<LinkedInGroupPostFields> PostOptions { get; private set; }

        /// <summary>
        /// Gets or sets group id to retrieve posts for
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// Gets or sets the value indicating optional member's role for posts query. Can be one of <see cref="LinkedInGroupPostRole"/> enumeration values
        /// </summary>
        public LinkedInGroupPostRole Role { get; set; }
    }
}
