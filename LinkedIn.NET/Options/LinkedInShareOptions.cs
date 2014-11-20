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
    /// Represents LinkedIn share options
    /// </summary>
    public class LinkedInShareOptions
    {
        /// <summary>
        /// Max length of LinkedIn share comment
        /// </summary>
        public const int MAX_COMMENT_LENGTH = 700;
        private const int MAX_TITLE_LENGTH = 200;
        private const int MAX_DESCRIPTION_LENGHT = 256;

        private string _Comment = "";
        private string _Title = "";
        private string _Description = "";

        /// <summary>
        /// Gets or set share's description
        /// <remarks>Max lenght is 256</remarks>
        /// </summary>
        public string Description
        {
            get { return _Description; }
            set
            {
                _Description = value;
                if (_Description.Length > MAX_DESCRIPTION_LENGHT)
                    _Description = _Description.Substring(0, MAX_DESCRIPTION_LENGHT);
            }
        }
        /// <summary>
        /// Gets or set share's title
        /// <remarks>Max lenght is 200</remarks>
        /// </summary>
        public string Title
        {
            get { return _Title; }
            set
            {
                _Title = value;
                if (_Title.Length > MAX_TITLE_LENGTH) _Title = _Title.Substring(0, MAX_TITLE_LENGTH);
            }
        }
        /// <summary>
        /// Gets or set share's comment
        /// <remarks>Max lenght is 700</remarks>
        /// </summary>
        public string Comment
        {
            get { return _Comment; }
            set
            {
                _Comment = value;
                if (_Comment.Length > MAX_COMMENT_LENGTH) _Comment = _Comment.Substring(0, MAX_COMMENT_LENGTH);
            }
        }
        /// <summary>
        /// Gets or set share's submitted URL
        /// </summary>
        public string SubmittedUrl { get; set; }
        /// <summary>
        /// Gets or set share's submitted image URL
        /// </summary>
        public string SubmittedImageUrl { get; set; }
        /// <summary>
        /// Gets or set <see cref="LinkedInShareVisibilityCode"/> object representing share's cisibility
        /// </summary>
        public LinkedInShareVisibilityCode VisibilityCode { get; set; }
    }
}
