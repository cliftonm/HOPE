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
    /// Represents LinkedIn patent object
    /// </summary>
    public class LinkedInPatent
    {
        private readonly List<LinkedInInventor> _Inventors = new List<LinkedInInventor>();

        /// <summary>
        /// Gets unique identifier for patent
        /// </summary>
        public string Id { get; internal set; }
        /// <summary>
        /// Gets patent title
        /// </summary>
        public string Title { get; internal set; }
        /// <summary>
        /// Gets a short summary of the patent
        /// </summary>
        public string Summary { get; internal set; }
        /// <summary>
        /// Gets a string with the patent or application number
        /// </summary>
        public string Number { get; internal set; }
        /// <summary>
        /// Gets <see cref="LinkedInPatentStatus"/> object representing the patent status
        /// </summary>
        public LinkedInPatentStatus Status { get; private set; }
        /// <summary>
        /// Gets <see cref="LinkedInPatentOffice"/> object describing the patent issuing body
        /// </summary>
        public LinkedInPatentOffice Office { get; private set; }
        /// <summary>
        /// Gets collection of <see cref="LinkedInInventor"/> objects representing the patent inventors
        /// </summary>
        public IEnumerable<LinkedInInventor> Inventors
        {
            get { return _Inventors.AsEnumerable(); }
        }
        /// <summary>
        /// Gets <see cref="LinkedInDate"/> object indicating when the application was filed or when the patent was granted
        /// </summary>
        public LinkedInDate Date { get; internal set; }
        /// <summary>
        /// Gets patent URL
        /// </summary>
        public string Url { get; internal set; }

        internal LinkedInPatent()
        {
            Status = new LinkedInPatentStatus();
            Office = new LinkedInPatentOffice();
        }

        internal void AddInventors(IEnumerable<LinkedInInventor> inventors)
        {
            _Inventors.AddRange(inventors);
        }
    }
}
