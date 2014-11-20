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

using System.Collections.Generic;
using System.Linq;

namespace LinkedIn.NET
{
    /// <summary>
    /// Represents LinkedIn publication object
    /// </summary>
    public class LinkedInPublication
    {
        private readonly List<LinkedInAuthor> _Authors = new List<LinkedInAuthor>();

        /// <summary>
        /// Gets unique identifier for publication
        /// </summary>
        public string Id { get; internal set; }
        /// <summary>
        /// Gets publication's title
        /// </summary>
        public string Title { get; internal set; }
        /// <summary>
        /// Gets <see cref="LinkedInDate"/> object representing publication's date
        /// </summary>
        public LinkedInDate Date { get; internal set; }
        /// <summary>
        /// Gets publication's summary
        /// </summary>
        public string Summary { get; internal set; }
        /// <summary>
        /// Gets publication's URL
        /// </summary>
        public string Url { get; internal set; }
        /// <summary>
        /// Gets the name of the publisher of this publication
        /// </summary>
        public string Publisher { get; internal set; }
        /// <summary>
        /// Gets collection of <see cref="LinkedInAuthor"/> objects representing authors of this publication
        /// </summary>
        public IEnumerable<LinkedInAuthor> Authors
        {
            get { return _Authors.AsEnumerable(); }
        }

        internal LinkedInPublication()
        {
        }

        internal void AddAuthors(IEnumerable<LinkedInAuthor> authors)
        {
            _Authors.AddRange(authors);
        }
    }
}
