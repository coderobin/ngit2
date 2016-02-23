/*
This code is derived from jgit (http://eclipse.org/jgit).
Copyright owners are documented in jgit's IP log.

This program and the accompanying materials are made available
under the terms of the Eclipse Distribution License v1.0 which
accompanies this distribution, is reproduced below, and is
available at http://www.eclipse.org/org/documents/edl-v10.php

All rights reserved.

Redistribution and use in source and binary forms, with or
without modification, are permitted provided that the following
conditions are met:

- Redistributions of source code must retain the above copyright
  notice, this list of conditions and the following disclaimer.

- Redistributions in binary form must reproduce the above
  copyright notice, this list of conditions and the following
  disclaimer in the documentation and/or other materials provided
  with the distribution.

- Neither the name of the Eclipse Foundation, Inc. nor the
  names of its contributors may be used to endorse or promote
  products derived from this software without specific prior
  written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Linq;
using System.Text;
using NGit;
// using NGit.Errors;
using NGit.Internal;
using NGit.Util;

namespace NGit
{
	/// <summary>
	/// A prefix abbreviation of an
	/// <see cref="ObjectId">ObjectId</see>
	/// .
	/// <p>
	/// Sometimes Git produces abbreviated SHA-1 strings, using sufficient leading
	/// digits from the ObjectId name to still be unique within the repository the
	/// string was generated from. These ids are likely to be unique for a useful
	/// period of time, especially if they contain at least 6-10 hex digits.
	/// <p>
	/// This class converts the hex string into a binary form, to make it more
	/// efficient for matching against an object.
	/// </summary>
	[System.Runtime.Serialization.DataContract]
	public sealed class AbbreviatedObjectId
	{
		private const long serialVersionUID = 1L;

        internal byte[] sha1 = new byte[Constants.OBJECT_ID_LENGTH];

        /// <summary>
        /// Length of the hexstring
        /// </summary>
        internal int length = 0;

        /// <summary>Test a string of characters to verify it is a hex format.</summary>
        /// <remarks>
        /// Test a string of characters to verify it is a hex format.
        /// <p>
        /// If true the string can be parsed with
        /// <see cref="FromString(string)">FromString(string)</see>
        /// .
        /// </remarks>
        /// <param name="id">the string to test.</param>
        /// <returns>true if the string can converted into an AbbreviatedObjectId.</returns>
        public static bool IsId(string id)
		{
			if (id.Length < 2 || Constants.OBJECT_ID_STRING_LENGTH < id.Length)
			{
				return false;
			}
			try
			{
				for (int i = 0; i < id.Length; i++)
				{
                    Convert.ToInt32(id.Substring(i, 1), 16);
				}
				return true;
			}
			catch (Exception) // could be multiple types of exception
			{
				return false;
			}
		}

        /// <summary>Convert an AbbreviatedObjectId from hex characters (US-ASCII).</summary>
        /// <remarks>Convert an AbbreviatedObjectId from hex characters (US-ASCII).</remarks>
        /// <param name="buf">the US-ASCII buffer to read from.</param>
        /// <param name="offset">position to read the first character from.</param>
        /// <param name="end">
        /// one past the last position to read (<code>end-offset</code> is
        /// the length of the string).
        /// </param>
        /// <returns>the converted object id.</returns>
        public static NGit.AbbreviatedObjectId FromString(byte[] buf, int offset, int end
            )
        {
            if (end - offset > Constants.OBJECT_ID_STRING_LENGTH)
            {
                throw new ArgumentException(MessageFormat.Format(JGitText.Get().invalidIdLength, ""));
            }
            var range = buf.Skip(offset).Take(end - offset);

            byte[] b = Enumerable.Range(0, end - offset)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(Encoding.ASCII.GetString(range.Skip(x).Take(2).ToArray()), 16))
                .ToArray();
            return new AbbreviatedObjectId(b, end - offset);
		}

		/// <summary>
		/// Convert an AbbreviatedObjectId from an
		/// <see cref="AnyObjectId">AnyObjectId</see>
		/// .
		/// <p>
		/// This method copies over all bits of the Id, and is therefore complete
		/// (see
		/// <see cref="IsComplete()">IsComplete()</see>
		/// ).
		/// </summary>
		/// <param name="id">
		/// the
		/// <see cref="ObjectId">ObjectId</see>
		/// to convert from.
		/// </param>
		/// <returns>the converted object id.</returns>
		public static NGit.AbbreviatedObjectId FromObjectId(AnyObjectId id)
		{
			return new NGit.AbbreviatedObjectId(id.sha1, Constants.OBJECT_ID_STRING_LENGTH);
		}

		/// <summary>Convert an AbbreviatedObjectId from hex characters.</summary>
		/// <remarks>Convert an AbbreviatedObjectId from hex characters.</remarks>
		/// <param name="str">the string to read from. Must be &lt;= 40 characters.</param>
		/// <returns>the converted object id.</returns>
		public static NGit.AbbreviatedObjectId FromString(string str)
		{
			if (str.Length > Constants.OBJECT_ID_STRING_LENGTH)
			{
				throw new ArgumentException(MessageFormat.Format(JGitText.Get().invalidId, str));
			}
            byte[] b = Enumerable.Range(0, str.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(String.Concat<char>(str.Skip(x).Take(2).ToArray()), 16))
                .ToArray();
            return new AbbreviatedObjectId(b, str.Length);
		}

		internal AbbreviatedObjectId(byte[] bytes, int length)
		{
            Buffer.BlockCopy(bytes, 0, this.sha1, 0, bytes.Length);
            this.length = length;
		}

		/// <returns>number of hex digits appearing in this id</returns>
		public int Length
		{
			get
			{
				return this.length;
			}
		}

		/// <returns>true if this ObjectId is actually a complete id.</returns>
		public bool IsComplete
		{
			get
			{
				return this.length == Constants.OBJECT_ID_STRING_LENGTH;
			}
		}

		/// <returns>
		/// a complete ObjectId; null if
		/// <see cref="IsComplete()">IsComplete()</see>
		/// is false
		/// </returns>
		public ObjectId ToObjectId()
		{
			return IsComplete ? new ObjectId(this.sha1) : null;
		}

		/// <summary>Compares this abbreviation to a full object id.</summary>
		/// <remarks>Compares this abbreviation to a full object id.</remarks>
		/// <param name="other">the other object id.</param>
		/// <returns>
		/// &lt;0 if this abbreviation names an object that is less than
		/// <code>other</code>; 0 if this abbreviation exactly matches the
		/// first
		/// <see cref="Length()">Length()</see>
		/// digits of <code>other.name()</code>;
		/// &gt;0 if this abbreviation names an object that is after
		/// <code>other</code>.
		/// </returns>
		public int PrefixCompare(AnyObjectId other)
		{
            for (int i = 0; i < this.length / 2; i++)
            {
                int cmp = sha1[i] - other.sha1[i];
                if (cmp != 0) return cmp;
            }
            return this.length % 2 == 0 ?
                0 : sha1[this.length / 2] - ((other.sha1[this.length / 2] & 0xf0) >> 4);
        }

		/// <summary>Compare this abbreviation to a network-byte-order ObjectId.</summary>
		/// <remarks>Compare this abbreviation to a network-byte-order ObjectId.</remarks>
		/// <param name="bs">array containing the other ObjectId in network byte order.</param>
		/// <param name="p">
		/// position within
		/// <code>bs</code>
		/// to start the compare at. At least
		/// 20 bytes, starting at this position are required.
		/// </param>
		/// <returns>
		/// &lt;0 if this abbreviation names an object that is less than
		/// <code>other</code>; 0 if this abbreviation exactly matches the
		/// first
		/// <see cref="Length()">Length()</see>
		/// digits of <code>other.name()</code>;
		/// &gt;0 if this abbreviation names an object that is after
		/// <code>other</code>.
		/// </returns>
		public int PrefixCompare(byte[] bs, int p)
		{
            throw new NotImplementedException("Not clear about the intention");
        }

		/// <summary>Compare this abbreviation to a network-byte-order ObjectId.</summary>
		/// <remarks>Compare this abbreviation to a network-byte-order ObjectId.</remarks>
		/// <param name="bs">array containing the other ObjectId in network byte order.</param>
		/// <param name="p">
		/// position within
		/// <code>bs</code>
		/// to start the compare at. At least 5
		/// ints, starting at this position are required.
		/// </param>
		/// <returns>
		/// &lt;0 if this abbreviation names an object that is less than
		/// <code>other</code>; 0 if this abbreviation exactly matches the
		/// first
		/// <see cref="Length()">Length()</see>
		/// digits of <code>other.name()</code>;
		/// &gt;0 if this abbreviation names an object that is after
		/// <code>other</code>.
		/// </returns>
		public int PrefixCompare(int[] bs, int p)
		{
            throw new NotImplementedException("Not clear about the intention");
        }

		/// <returns>value for a fan-out style map, only valid of length &gt;= 2.</returns>
		public int FirstByte
		{
			get
			{
                return sha1[0];
			}
		}

		public override int GetHashCode()
		{
            return BitConverter.ToInt32(sha1, 4);
        }

		public override bool Equals(object o)
		{
			if (o is NGit.AbbreviatedObjectId)
			{
				NGit.AbbreviatedObjectId b = (NGit.AbbreviatedObjectId)o;
                return b.length == this.length && b.sha1.SequenceEqual(this.sha1);
			}
			return false;
		}

		/// <returns>string form of the abbreviation, in lower case hexadecimal.</returns>
		public string Name
		{
			get
			{
                string hex = BitConverter.ToString(sha1).Replace("-", "").ToLowerInvariant();
                string front = hex.Substring(0, this.length / 2 * 2);

                return this.length % 2 == 0 ?
                    front : front + hex[this.length];
            }
		}

		public override string ToString()
		{
			return "AbbreviatedObjectId[" + Name + "]";
		}
	}
}
