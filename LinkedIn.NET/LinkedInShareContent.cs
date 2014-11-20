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

namespace LinkedIn.NET
{
    /// <summary>
    /// Represents LinkedIn share content object
    /// </summary>
    public class LinkedInShareContent
    {
        /// <summary>
        /// Gets share content's submitted URL
        /// </summary>
        public string SubmittedUrl { get; internal set; }
        /// <summary>
        /// Gets share content's resolved URL
        /// </summary>
        public string ResolvedUrl { get; internal set; }
        /// <summary>
        /// Gets share content's shortened URL
        /// </summary>
        public string ShorteneddUrl { get; internal set; }
        /// <summary>
        /// Gets share content's title
        /// </summary>
        public string Title { get; internal set; }
        /// <summary>
        /// Gets share content's description
        /// </summary>
        public string Description { get; internal set; }
        /// <summary>
        /// Gets share content's submitted image URL
        /// </summary>
        public string SubmittedImageUrl { get; internal set; }
        /// <summary>
        /// Gets share content's thumbnail URL
        /// </summary>
        public string ThumbnailUrl { get; internal set; }
        /// <summary>
        /// Gets share content's eyebrow URL
        /// </summary>
        public string EyebrowUrl { get; internal set; }

        internal LinkedInShareContent()
        {
        }
    }
}
