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
    /// Represents object that include information about LinkedIn.NET operations
    /// </summary>
    /// <typeparam name="T">Type of object that includes request results</typeparam>
    public class LinkedInResponse<T>
    {
        internal LinkedInResponse(T result, LinkedInResponseStatus status, object userState)
        {
            Result = result;
            Status = status;
            UserState = userState;
        }

        internal LinkedInResponse(T result, LinkedInResponseStatus status, object userState, string message)
        {
            Result = result;
            Status = status;
            Message = message;
            UserState = userState;
        }

        internal LinkedInResponse(T result, LinkedInResponseStatus status, object userState, Exception ex)
        {
            Result = result;
            Status = status;
            UserState = userState;
            Exception = ex;
        }

        /// <summary>
        /// Gets object that includes request results
        /// </summary>
        public T Result { get; private set; }

        /// <summary>
        /// Gets value indicating operation's success. Can be one of <see cref="LinkedInResponseStatus"/> enumeration values.
        /// </summary>
        public LinkedInResponseStatus Status { get; private set; }

        /// <summary>
        /// Gets exception occured
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Gets a user-defined object that qualifies or contains information about response
        /// </summary>
        public object UserState { get; private set; }

        /// <summary>
        /// Gets possible web response message 
        /// </summary>
        public string Message { get; private set; }
    }
}
