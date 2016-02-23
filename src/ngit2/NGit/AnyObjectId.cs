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
using System.IO;
using System.Linq;
using System.Text;
using NGit;
using NGit.Util;

namespace NGit
{
	/// <summary>A (possibly mutable) SHA-1 abstraction.</summary>
	/// <remarks>
	/// A (possibly mutable) SHA-1 abstraction.
	/// <p>
	/// If this is an instance of
	/// <see cref="MutableObjectId">MutableObjectId</see>
	/// the concept of equality
	/// with this instance can alter at any time, if this instance is modified to
	/// represent a different object name.
	/// </remarks>
	public abstract class AnyObjectId : System.IComparable<object>
	{
        internal byte[] sha1 = new byte[Constants.OBJECT_ID_LENGTH];

        /// <summary>Compare to object identifier byte sequences for equality.</summary>
        /// <remarks>Compare to object identifier byte sequences for equality.</remarks>
        /// <param name="firstObjectId">the first identifier to compare. Must not be null.</param>
        /// <param name="secondObjectId">the second identifier to compare. Must not be null.</param>
        /// <returns>true if the two identifiers are the same.</returns>
        public static bool Equals(AnyObjectId firstObjectId, AnyObjectId secondObjectId)
		{
			if (firstObjectId == secondObjectId)
			{
				return true;
			}
            // We test word 2 first as odds are someone already used our
            // word 1 as a hash code, and applying that came up with these
            // two instances we are comparing for equality. Therefore the
            // first two words are very likely to be identical. We want to
            // break away from collisions as quickly as possible.
            //
            return firstObjectId.sha1.SequenceEqual(secondObjectId.sha1);
		}

		/// <summary>Get the first 8 bits of the ObjectId.</summary>
		/// <remarks>
		/// Get the first 8 bits of the ObjectId.
		/// This is a faster version of
		/// <code>getByte(0)</code>
		/// .
		/// </remarks>
		/// <returns>
		/// a discriminator usable for a fan-out style map. Returned values
		/// are unsigned and thus are in the range [0,255] rather than the
		/// signed byte range of [-128, 127].
		/// </returns>
		public int FirstByte
		{
			get
			{
                return sha1[0];
			}
		}

		/// <summary>Get any byte from the ObjectId.</summary>
		/// <remarks>
		/// Get any byte from the ObjectId.
		/// Callers hard-coding
		/// <code>getByte(0)</code>
		/// should instead use the much faster
		/// special case variant
		/// <see cref="FirstByte()">FirstByte()</see>
		/// .
		/// </remarks>
		/// <param name="index">
		/// index of the byte to obtain from the raw form of the ObjectId.
		/// Must be in range [0,
		/// <see cref="Constants.OBJECT_ID_LENGTH">Constants.OBJECT_ID_LENGTH</see>
		/// ).
		/// </param>
		/// <returns>
		/// the value of the requested byte at
		/// <code>index</code>
		/// . Returned values
		/// are unsigned and thus are in the range [0,255] rather than the
		/// signed byte range of [-128, 127].
		/// </returns>
		/// <exception cref="System.IndexOutOfRangeException">
		/// <code>index</code>
		/// is less than 0, equal to
		/// <see cref="Constants.OBJECT_ID_LENGTH">Constants.OBJECT_ID_LENGTH</see>
		/// , or greater than
		/// <see cref="Constants.OBJECT_ID_LENGTH">Constants.OBJECT_ID_LENGTH</see>
		/// .
		/// </exception>
		public int GetByte(int index)
		{
            return sha1[index];
		}

		/// <summary>Compare this ObjectId to another and obtain a sort ordering.</summary>
		/// <remarks>Compare this ObjectId to another and obtain a sort ordering.</remarks>
		/// <param name="other">the other id to compare to. Must not be null.</param>
		/// <returns>
		/// &lt; 0 if this id comes before other; 0 if this id is equal to
		/// other; &gt; 0 if this id comes after other.
		/// </returns>
		public int CompareTo(AnyObjectId other)
		{
			if (this == other)
			{
				return 0;
			}
            for(int i = 0; i < Constants.OBJECT_ID_LENGTH; i++)
            {
                int cmp = sha1[i] - other.sha1[i];
                if (cmp != 0) return cmp;
            }
            return 0;
        }

		public int CompareTo(object other)
		{
			return CompareTo(((AnyObjectId)other));
		}

		/// <summary>Compare this ObjectId to a network-byte-order ObjectId.</summary>
		/// <remarks>Compare this ObjectId to a network-byte-order ObjectId.</remarks>
		/// <param name="bs">array containing the other ObjectId in network byte order.</param>
		/// <param name="p">
		/// position within
		/// <code>bs</code>
		/// to start the compare at. At least
		/// 20 bytes, starting at this position are required.
		/// </param>
		/// <returns>
		/// a negative integer, zero, or a positive integer as this object is
		/// less than, equal to, or greater than the specified object.
		/// </returns>
		public int CompareTo(byte[] bs, int p)
		{
            for (int i = 0; i < Constants.OBJECT_ID_LENGTH; i++)
            {
                int cmp = sha1[i] - bs[i + p];
                if (cmp != 0) return cmp;
            }
            return 0;
        }

		/// <summary>Compare this ObjectId to a network-byte-order ObjectId.</summary>
		/// <remarks>Compare this ObjectId to a network-byte-order ObjectId.</remarks>
		/// <param name="bs">array containing the other ObjectId in network byte order.</param>
		/// <param name="p">
		/// position within
		/// <code>bs</code>
		/// to start the compare at. At least 5
		/// integers, starting at this position are required.
		/// </param>
		/// <returns>
		/// a negative integer, zero, or a positive integer as this object is
		/// less than, equal to, or greater than the specified object.
		/// </returns>
		public int CompareTo(int[] bs, int p)
		{
            for (int i = 0; i < Constants.OBJECT_ID_LENGTH; i++)
            {
                int cmp = sha1[i] - bs[i + p];
                if (cmp != 0) return cmp;
            }
            return 0;
        }

		/// <summary>Tests if this ObjectId starts with the given abbreviation.</summary>
		/// <remarks>Tests if this ObjectId starts with the given abbreviation.</remarks>
		/// <param name="abbr">the abbreviation.</param>
		/// <returns>true if this ObjectId begins with the abbreviation; else false.</returns>
		public virtual bool StartsWith(AbbreviatedObjectId abbr)
		{
			return abbr.PrefixCompare(this) == 0;
		}

		public sealed override int GetHashCode()
		{
            return BitConverter.ToInt32(sha1, 4);
		}

		/// <summary>Determine if this ObjectId has exactly the same value as another.</summary>
		/// <remarks>Determine if this ObjectId has exactly the same value as another.</remarks>
		/// <param name="other">the other id to compare to. May be null.</param>
		/// <returns>true only if both ObjectIds have identical bits.</returns>
		public bool Equals(AnyObjectId other)
		{
			return other != null ? Equals(this, other) : false;
		}

		public sealed override bool Equals(object o)
		{
			if (o is AnyObjectId)
			{
				return Equals((AnyObjectId)o);
			}
			else
			{
				return false;
			}
		}

		/// <summary>Copy this ObjectId to an output writer in raw binary.</summary>
		/// <remarks>Copy this ObjectId to an output writer in raw binary.</remarks>
		/// <param name="w">the buffer to copy to. Must be in big endian order.</param>
		public virtual void CopyRawTo(MemoryStream m)
		{
            m.Write(sha1, 0, Constants.OBJECT_ID_LENGTH);
		}

		/// <summary>Copy this ObjectId to a byte array.</summary>
		/// <remarks>Copy this ObjectId to a byte array.</remarks>
		/// <param name="b">the buffer to copy to.</param>
		/// <param name="o">the offset within b to write at.</param>
		public virtual void CopyRawTo(byte[] b, int o)
		{
            Buffer.BlockCopy(sha1, 0, b, o, Constants.OBJECT_ID_LENGTH);
		}

		/// <summary>Copy this ObjectId to an int array.</summary>
		/// <remarks>Copy this ObjectId to an int array.</remarks>
		/// <param name="b">the buffer to copy to.</param>
		/// <param name="o">the offset within b to write at.</param>
		public virtual void CopyRawTo(int[] b, int o)
		{
            Buffer.BlockCopy(sha1, 0, b, o, Constants.OBJECT_ID_LENGTH);
        }

		/// <summary>Copy this ObjectId to an output writer in raw binary.</summary>
		/// <remarks>Copy this ObjectId to an output writer in raw binary.</remarks>
		/// <param name="w">the stream to write to.</param>
		/// <exception cref="System.IO.IOException">the stream writing failed.</exception>
		public virtual void CopyRawTo(Stream s)
		{
            s.Write(sha1, 0, Constants.OBJECT_ID_LENGTH);
		}

		/// <summary>Copy this ObjectId to an output writer in hex format.</summary>
		/// <remarks>Copy this ObjectId to an output writer in hex format.</remarks>
		/// <param name="w">the stream to copy to.</param>
		/// <exception cref="System.IO.IOException">the stream writing failed.</exception>
		public virtual void CopyTo(Stream s)
		{
            s.Write(sha1, 0, Constants.OBJECT_ID_LENGTH);
        }

		/// <summary>Copy this ObjectId to a byte array in hex format.</summary>
		/// <remarks>Copy this ObjectId to a byte array in hex format.</remarks>
		/// <param name="b">the buffer to copy to.</param>
		/// <param name="o">the offset within b to write at.</param>
		public virtual void CopyTo(byte[] b, int o)
		{
            Buffer.BlockCopy(sha1, 0, b, o, Constants.OBJECT_ID_LENGTH);
        }

		/// <summary>Copy this ObjectId to a ByteBuffer in hex format.</summary>
		/// <remarks>Copy this ObjectId to a ByteBuffer in hex format.</remarks>
		/// <param name="b">the buffer to copy to.</param>
		public virtual void CopyTo(MemoryStream m)
		{
            m.Write(sha1, 0, Constants.OBJECT_ID_LENGTH);
		}

		/// <summary>Copy this ObjectId to an output writer in hex format.</summary>
		/// <remarks>Copy this ObjectId to an output writer in hex format.</remarks>
		/// <param name="w">the stream to copy to.</param>
		/// <exception cref="System.IO.IOException">the stream writing failed.</exception>
		public virtual void CopyTo(TextWriter w)
		{
			w.Write(sha1);
		}

		/// <summary>Copy this ObjectId to an output writer in hex format.</summary>
		/// <remarks>Copy this ObjectId to an output writer in hex format.</remarks>
		/// <param name="tmp">
		/// temporary char array to buffer construct into before writing.
		/// Must be at least large enough to hold 2 digits for each byte
		/// of object id (40 characters or larger).
		/// </param>
		/// <param name="w">the stream to copy to.</param>
		/// <exception cref="System.IO.IOException">the stream writing failed.</exception>
		public virtual void CopyTo(char[] tmp, TextWriter w)
		{
            throw new NotImplementedException();
		}

		/// <summary>Copy this ObjectId to a StringBuilder in hex format.</summary>
		/// <remarks>Copy this ObjectId to a StringBuilder in hex format.</remarks>
		/// <param name="tmp">
		/// temporary char array to buffer construct into before writing.
		/// Must be at least large enough to hold 2 digits for each byte
		/// of object id (40 characters or larger).
		/// </param>
		/// <param name="w">the string to append onto.</param>
		public virtual void CopyTo(char[] tmp, StringBuilder w)
		{
            throw new NotImplementedException();
		}

		public override string ToString()
		{
			return "AnyObjectId[" + Name + "]";
		}

		/// <returns>string form of the SHA-1, in lower case hexadecimal.</returns>
		public string Name
		{
			get
			{
                return BitConverter.ToString(sha1).Replace("-", "").ToLowerInvariant();
			}
		}

		/// <returns>string form of the SHA-1, in lower case hexadecimal.</returns>
		internal string GetName()
		{
			return Name;
		}

        /// <summary>Return an abbreviation (prefix) of this object SHA-1.</summary>
        /// <remarks>
        /// Return an abbreviation (prefix) of this object SHA-1.
        /// <p>
        /// This implementation does not guarantee uniqueness. Callers should
        /// instead use
        /// <see cref="ObjectReader.Abbreviate(AnyObjectId, int)">ObjectReader.Abbreviate(AnyObjectId, int)
        /// 	</see>
        /// to obtain a
        /// unique abbreviation within the scope of a particular object database.
        /// </remarks>
        /// <param name="len">length of the abbreviated string.</param>
        /// <returns>SHA-1 abbreviation.</returns>
        public virtual AbbreviatedObjectId Abbreviate(int len)
        {
            return new AbbreviatedObjectId(sha1, len);
        }

        /// <summary>Obtain an immutable copy of this current object name value.</summary>
        /// <remarks>
        /// Obtain an immutable copy of this current object name value.
        /// <p>
        /// Only returns <code>this</code> if this instance is an unsubclassed
        /// instance of
        /// <see cref="ObjectId">ObjectId</see>
        /// ; otherwise a new instance is returned
        /// holding the same value.
        /// <p>
        /// This method is useful to shed any additional memory that may be tied to
        /// the subclass, yet retain the unique identity of the object id for future
        /// lookups within maps and repositories.
        /// </remarks>
        /// <returns>an immutable copy, using the smallest memory footprint possible.</returns>
        public ObjectId Copy()
		{
			if (GetType() == typeof(ObjectId))
			{
				return (ObjectId)this;
			}
			return new ObjectId(this);
		}

		/// <summary>Obtain an immutable copy of this current object name value.</summary>
		/// <remarks>
		/// Obtain an immutable copy of this current object name value.
		/// <p>
		/// See
		/// <see cref="Copy()">Copy()</see>
		/// if <code>this</code> is a possibly subclassed (but
		/// immutable) identity and the application needs a lightweight identity
		/// <i>only</i> reference.
		/// </remarks>
		/// <returns>
		/// an immutable copy. May be <code>this</code> if this is already
		/// an immutable instance.
		/// </returns>
		public abstract ObjectId ToObjectId();
	}
}
