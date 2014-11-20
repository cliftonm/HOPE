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

namespace LinkedIn.NET
{
    /// <summary>
    /// Represents generic class intended to store various settings flags by switching on/off the bits of 32-bit integer
    /// </summary>
    /// <typeparam name="TE">The type of object to use</typeparam>
    public class BitField<TE> where TE : struct
    {
        private int _bits;

        internal BitField()
        {
        }
        /// <summary>
        /// Gets or sets value for specified bit
        /// </summary>
        /// <param name="index">Bit index</param>
        /// <returns>Value indicated whether specified bit is switched on or off</returns>
        public bool this[TE index]
        {
            get { return (_bits & (1 << Convert.ToInt32(index))) != 0; }
            set
            {
                if (value)
                {
                    _bits |= (1 << Convert.ToInt32(index));
                }
                else
                {
                    _bits &= ~(1 << Convert.ToInt32(index));
                }
            }
        }
        /// <summary>
        /// Gets value indicating whether at least one bit is switched on
        /// </summary>
        public bool HasValues
        {
            get { return _bits != 0; }
        }
        /// <summary>
        /// Switches off all bits
        /// </summary>
        public void Clear()
        {
            _bits = 0;
        }
        /// <summary>
        /// Switches on all bits
        /// </summary>
        public void SelectAll()
        {
            _bits = ~0;
        }
    }
}
