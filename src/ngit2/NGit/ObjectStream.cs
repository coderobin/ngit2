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
using NGit;

namespace NGit
{
	/// <summary>
	/// Stream of data coming from an object loaded by
	/// <see cref="ObjectLoader">ObjectLoader</see>
	/// .
	/// </summary>
	public abstract class ObjectStream : Stream
	{
		/// <returns>
		/// Git object type, see
		/// <see cref="Constants">Constants</see>
		/// .
		/// </returns>
		public abstract int GetGitType();

		/// <returns>total size of object in bytes</returns>
		public abstract long GetSize();

		/// <summary>Simple stream around the cached byte array created by a loader.</summary>
		/// <remarks>
		/// Simple stream around the cached byte array created by a loader.
		/// <p>
		/// ObjectLoader implementations can use this stream type when the object's
		/// content is small enough to be accessed as a single byte array, but the
		/// application has still requested it in stream format.
		/// </remarks>
		public class SmallStream : ObjectStream
		{
            private int ptr;

            private readonly int type;

			private readonly byte[] data;

            public override bool CanRead
            {
                get
                {
                    return true;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override bool CanWrite
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override long Length
            {
                get
                {
                    return data.Length;
                }
            }

            public override long Position
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            /// <summary>Create the stream from an existing loader's cached bytes.</summary>
            /// <remarks>Create the stream from an existing loader's cached bytes.</remarks>
            /// <param name="loader">the loader.</param>
            public SmallStream(ObjectLoader loader) : this(loader.GetGitType(), loader.GetCachedBytes
				())
			{
			}

			/// <summary>Create the stream from an existing byte array and type.</summary>
			/// <remarks>Create the stream from an existing byte array and type.</remarks>
			/// <param name="type">the type constant for the object.</param>
			/// <param name="data">the fully inflated content of the object.</param>
			public SmallStream(int type, byte[] data)
			{
				this.type = type;
				this.data = data;
			}

			public override int GetGitType()
			{
				return type;
			}

			public override long GetSize()
			{
                // TODO: review, this might not be needed as we have prop: Length on stream override
				return data.Length;
			}

			public override int ReadByte()
			{
				if (ptr == data.Length)
				{
					return -1;
				}
				return data[ptr++];
			}

			public override int Read(byte[] b, int off, int len)
			{
				if (ptr == data.Length)
				{
					return -1;
				}
				int n = Math.Min(data.Length - ptr, len);
				Buffer.BlockCopy(data, ptr, b, off, n);
				ptr += n;
				return n;
			}

            public override void Flush()
            {
                throw new NotImplementedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }
        }
	}
}
