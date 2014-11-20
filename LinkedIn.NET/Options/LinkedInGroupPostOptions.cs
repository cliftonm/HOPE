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
    /// Represents LinkedIn group's post options
    /// </summary>
    public class LinkedInGroupPostOptions
    {
        private const int MAX_TITLE_LENGTH = 200;

        private string _Title = "";
        private string _ContentTitle = "";
        private string _ContentText = "";

        /// <summary>
        /// Gets or sets post contents's text
        /// </summary>
        public string ContentText
        {
            get { return _ContentText; }
            set { _ContentText = value; }
        }

        /// <summary>
        /// Gets or sets post contents's title
        /// </summary>
        public string ContentTitle
        {
            get { return _ContentTitle; }
            set 
            { 
                _ContentTitle = value;
                if (_ContentTitle.Length > MAX_TITLE_LENGTH)
                    _ContentTitle = _ContentTitle.Substring(0, MAX_TITLE_LENGTH);
            }
        }

        /// <summary>
        /// Gets or sets post's summary
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or set post's title
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
        /// Gets or set post content's submitted URL
        /// </summary>
        public string SubmittedUrl { get; set; }

        /// <summary>
        /// Gets or set posts content's submitted image URL
        /// </summary>
        public string SubmittedImageUrl { get; set; }

        internal string GroupId { get; set; }
    }
}
