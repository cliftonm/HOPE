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

namespace LinkedIn.NET.Groups
{
    /// <summary>
    /// Represents generic class intended to store various settings flags by switching on/off the bits of 32-bit integer. 
    /// The difference from the class it derives from is that current class has read only setters.
    /// </summary>
    /// <typeparam name="TE">The type of object to use</typeparam>
    public class LinkedInBits<TE> : BitField<TE> where TE : struct
    {
        internal LinkedInBits()
        {
        }

        private int _Bits;

        /// <summary>
        /// Gets value for specified bit
        /// </summary>
        /// <param name="index">Bit index</param>
        /// <returns>Value indicated whether specified bit is switched on or off</returns>
        public new bool this[TE index]
        {
            get { return (_Bits & (1 << Convert.ToInt32(index))) != 0; }
            internal set
            {
                if (value)
                {
                    _Bits |= (1 << Convert.ToInt32(index));
                }
                else
                {
                    _Bits &= ~(1 << Convert.ToInt32(index));
                }
            }
        }

        internal new void Clear()
        {
            base.Clear();
        }

        internal new void SelectAll()
        {
            base.SelectAll();
        }
    }
}
