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

namespace LinkedIn.NET.Groups
{
    /// <summary>
    /// Represents LinkedIn group post attachment.
    /// </summary>
    public class LinkedInGroupPostAttachment
    {
        internal LinkedInGroupPostAttachment()
        {
        }

        /// <summary>
        /// Gets attachment's content URL.
        /// </summary>
        public string ContentUrl { get; internal set; }

        /// <summary>
        /// Gets attachment's title.
        /// </summary>
        public string Title { get; internal set; }

        /// <summary>
        /// Gets attachment's summary.
        /// </summary>
        public string Summary { get; internal set; }

        /// <summary>
        /// Gets attachment's image URL.
        /// </summary>
        public string ImageUrl { get; internal set; }

        /// <summary>
        /// Gets attachment's content domain.
        /// </summary>
        public string ContentDomain { get; internal set; }
    }
}
