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
	/// <summary>A mutable SHA-1 abstraction.</summary>
	/// <remarks>A mutable SHA-1 abstraction.</remarks>
	public class MutableObjectId : AnyObjectId
	{
		/// <summary>Empty constructor.</summary>
		/// <remarks>Empty constructor. Initialize object with default (zeros) value.</remarks>
		public MutableObjectId() : base()
		{
		}

		/// <summary>Copying constructor.</summary>
		/// <remarks>Copying constructor.</remarks>
		/// <param name="src">original entry, to copy id from</param>
		internal MutableObjectId(NGit.MutableObjectId src)
		{
			FromObjectId(src);
		}

		/// <summary>Set any byte in the id.</summary>
		/// <remarks>Set any byte in the id.</remarks>
		/// <param name="index">
		/// index of the byte to set in the raw form of the ObjectId. Must
		/// be in range [0,
		/// <see cref="Constants.OBJECT_ID_LENGTH">Constants.OBJECT_ID_LENGTH</see>
		/// ).
		/// </param>
		/// <param name="value">
		/// the value of the specified byte at
		/// <code>index</code>
		/// .
		/// </param>
		/// <exception cref="System.IndexOutOfRangeException">
		/// <code>index</code>
		/// is less than 0, equal to or greater than
		/// <see cref="Constants.OBJECT_ID_LENGTH">Constants.OBJECT_ID_LENGTH</see>
		/// .
		/// </exception>
		public virtual void SetByte(int index, byte value)
		{
            if (index < 0 || index >= Constants.OBJECT_ID_LENGTH)
            {
                throw new IndexOutOfRangeException();
            }

            sha1[index] = value;
		}

		/// <summary>
		/// Make this id match
		/// <see cref="ObjectId.ZeroId()">ObjectId.ZeroId()</see>
		/// .
		/// </summary>
		public virtual void Clear()
		{
            Array.Clear(sha1, 0, Constants.OBJECT_ID_LENGTH);
		}

		/// <summary>Copy an ObjectId into this mutable buffer.</summary>
		/// <remarks>Copy an ObjectId into this mutable buffer.</remarks>
		/// <param name="src">the source id to copy from.</param>
		public virtual void FromObjectId(AnyObjectId src)
		{
            Buffer.BlockCopy(src.sha1, 0, sha1, 0, Constants.OBJECT_ID_LENGTH);
		}

		/// <summary>Convert an ObjectId from raw binary representation.</summary>
		/// <remarks>Convert an ObjectId from raw binary representation.</remarks>
		/// <param name="bs">
		/// the raw byte buffer to read from. At least 20 bytes must be
		/// available within this byte array.
		/// </param>
		public virtual void FromRaw(byte[] bs)
		{
			FromRaw(bs, 0);
		}

		/// <summary>Convert an ObjectId from raw binary representation.</summary>
		/// <remarks>Convert an ObjectId from raw binary representation.</remarks>
		/// <param name="bs">
		/// the raw byte buffer to read from. At least 20 bytes after p
		/// must be available within this byte array.
		/// </param>
		/// <param name="p">position to read the first byte of data from.</param>
		public virtual void FromRaw(byte[] bs, int p)
		{
            if (bs.Length < p + Constants.OBJECT_ID_LENGTH)
            {
                throw new ArgumentException("byte buf not long enough");
            }

            Buffer.BlockCopy(bs, p, sha1, 0, Constants.OBJECT_ID_LENGTH);
        }

		/// <summary>Convert an ObjectId from hex characters (US-ASCII).</summary>
		/// <remarks>Convert an ObjectId from hex characters (US-ASCII).</remarks>
		/// <param name="buf">
		/// the US-ASCII buffer to read from. At least 40 bytes after
		/// offset must be available within this byte array.
		/// </param>
		/// <param name="offset">position to read the first character from.</param>
		public virtual void FromString(byte[] buf, int offset)
		{
            if (buf.Length - offset < Constants.OBJECT_ID_STRING_LENGTH)
            {
                throw new ArgumentException("Byte buf not long enough");
            }
            byte[] b = Enumerable.Range(offset, offset + Constants.OBJECT_ID_STRING_LENGTH)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(Encoding.ASCII.GetString(buf.Skip(x).Take(2).ToArray()), 16))
                .ToArray();

            Buffer.BlockCopy(b, 0, sha1, 0, Constants.OBJECT_ID_LENGTH);
        }

		/// <summary>Convert an ObjectId from hex characters.</summary>
		/// <remarks>Convert an ObjectId from hex characters.</remarks>
		/// <param name="str">the string to read from. Must be 40 characters long.</param>
		public virtual void FromString(string str)
		{
			if (str.Length != Constants.OBJECT_ID_STRING_LENGTH)
			{
				throw new ArgumentException(MessageFormat.Format(JGitText.Get().invalidId, str));
			}
            byte[] b = Enumerable.Range(0, str.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(str.Substring(x, 2), 16))
                .ToArray();

            Buffer.BlockCopy(b, 0, sha1, 0, Constants.OBJECT_ID_LENGTH);
        }

		public override ObjectId ToObjectId()
		{
			return new ObjectId(this);
		}
	}
}
