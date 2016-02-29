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
using NGit;
using Xunit;

namespace NGit
{
	public class ObjectLoaderTest
	{
        private Random rnd;

        public ObjectLoaderTest()
        {
            rnd = new Random();
        }

		/// <exception cref="NGit.Errors.MissingObjectException"></exception>
		/// <exception cref="System.IO.IOException"></exception>
		[Fact]
		public virtual void TestSmallObjectLoader()
		{
            byte[] act = new byte[512];
            rnd.NextBytes(act);
			ObjectLoader ldr = new ObjectLoader.SmallObject(Constants.OBJ_BLOB, act);
			Assert.Equal<int>(Constants.OBJ_BLOB, ldr.GetGitType());
			Assert.Equal<long>(act.Length, ldr.GetSize());
			Assert.False(ldr.IsLarge(), "not is large");
			Assert.Same(act, ldr.GetCachedBytes());
			Assert.Same(act, ldr.GetCachedBytes(1));
			Assert.Same(act, ldr.GetCachedBytes(int.MaxValue));
			byte[] copy = ldr.GetBytes();
			Assert.NotSame(act, copy);
			Assert.True(act.SequenceEqual(copy), "same content");
			copy = ldr.GetBytes(1);
			Assert.NotSame(act, copy);
			Assert.True(act.SequenceEqual(copy), "same content");
            copy = ldr.GetBytes(int.MaxValue);
			Assert.NotSame(act, copy);
			Assert.True(act.SequenceEqual(copy), "same content");
            ObjectStream @in = ldr.OpenStream();
			Assert.NotNull(@in);
			Assert.True(@in is ObjectStream.SmallStream, "is small stream");
			Assert.Equal<int>(Constants.OBJ_BLOB, @in.GetGitType());
			Assert.Equal<long>(act.Length, @in.GetSize());
			copy = new byte[act.Length];
			Assert.Equal<int>(act.Length, @in.Read(copy, 0, act.Length));
			Assert.Equal<int>(-1, @in.ReadByte());
			Assert.True(act.SequenceEqual(copy), "same content");
			MemoryStream tmp = new MemoryStream();
			ldr.CopyTo(tmp);
			Assert.True(act.SequenceEqual(tmp.ToArray()), "same content"
				);
		}
/*
		/// <exception cref="NGit.Errors.MissingObjectException"></exception>
		/// <exception cref="System.IO.IOException"></exception>
		[Fact]
		public virtual void TestLargeObjectLoader()
		{
			byte[] act = GetRng().NextBytes(512);
			ObjectLoader ldr = new _ObjectLoader_122(act);
			Assert.Equal<int>(Constants.OBJ_BLOB, ldr.GetType());
			Assert.Equal<int>(act.Length, ldr.GetSize());
			Assert.True(ldr.IsLarge(), "is large");
			try
			{
				ldr.GetCachedBytes();
				NUnit.Framework.Assert.Fail("did not throw on getCachedBytes()");
			}
			catch (LargeObjectException)
			{
			}
			// expected
			try
			{
				ldr.GetBytes();
				NUnit.Framework.Assert.Fail("did not throw on getBytes()");
			}
			catch (LargeObjectException)
			{
			}
			// expected
			try
			{
				ldr.GetCachedBytes(64);
				NUnit.Framework.Assert.Fail("did not throw on getCachedBytes(64)");
			}
			catch (LargeObjectException)
			{
			}
			// expected
			byte[] copy = ldr.GetCachedBytes(1024);
			Assert.NotSame(act, copy);
			Assert.True(Arrays.Equals(act, copy), "same content");
			ObjectStream @in = ldr.OpenStream();
			Assert.NotNull(@in, "has stream");
			Assert.Equal<int>(Constants.OBJ_BLOB, @in.GetType());
			Assert.Equal<int>(act.Length, @in.GetSize());
			Assert.Equal<int>(act.Length, @in.Available());
			Assert.True(@in.MarkSupported(), "mark supported");
			copy = new byte[act.Length];
			Assert.Equal<int>(act.Length, @in.Read(copy));
			Assert.Equal<int>(0, @in.Available());
			Assert.Equal<int>(-1, @in.Read());
			Assert.True(Arrays.Equals(act, copy), "same content");
			ByteArrayOutputStream tmp = new ByteArrayOutputStream();
			ldr.CopyTo(tmp);
			Assert.True(Arrays.Equals(act, tmp.ToByteArray()), "same content"
				);
		}

		private sealed class _ObjectLoader_122 : ObjectLoader
		{
			public _ObjectLoader_122(byte[] act)
			{
				this.act = act;
			}

			/// <exception cref="NGit.Errors.LargeObjectException"></exception>
			public override byte[] GetCachedBytes()
			{
				throw new LargeObjectException();
			}

			public override long GetSize()
			{
				return act.Length;
			}

			public override int GetType()
			{
				return Constants.OBJ_BLOB;
			}

			/// <exception cref="NGit.Errors.MissingObjectException"></exception>
			/// <exception cref="System.IO.IOException"></exception>
			public override ObjectStream OpenStream()
			{
				return new ObjectStream.Filter(this.GetType(), act.Length, new ByteArrayInputStream
					(act));
			}

			private readonly byte[] act;
		}

		/// <exception cref="NGit.Errors.LargeObjectException"></exception>
		/// <exception cref="NGit.Errors.MissingObjectException"></exception>
		/// <exception cref="System.IO.IOException"></exception>
		[Fact]
		public virtual void TestLimitedGetCachedBytes()
		{
			byte[] act = GetRng().NextBytes(512);
			ObjectLoader ldr = new _SmallObject_196(Constants.OBJ_BLOB, act);
			Assert.True(ldr.IsLarge(), "is large");
			try
			{
				ldr.GetCachedBytes(10);
				NUnit.Framework.Assert.Fail("Did not throw LargeObjectException");
			}
			catch (LargeObjectException)
			{
			}
			// Expected result.
			byte[] copy = ldr.GetCachedBytes(512);
			Assert.NotSame(act, copy);
			Assert.True(Arrays.Equals(act, copy), "same content");
			copy = ldr.GetCachedBytes(1024);
			Assert.NotSame(act, copy);
			Assert.True(Arrays.Equals(act, copy), "same content");
		}

		private sealed class _SmallObject_196 : ObjectLoader.SmallObject
		{
			public _SmallObject_196(int baseArg1, byte[] baseArg2) : base(baseArg1, baseArg2)
			{
			}

			public override bool IsLarge()
			{
				return true;
			}
		}

		/// <exception cref="NGit.Errors.LargeObjectException"></exception>
		/// <exception cref="NGit.Errors.MissingObjectException"></exception>
		/// <exception cref="System.IO.IOException"></exception>
		[Fact]
		public virtual void TestLimitedGetCachedBytesExceedsJavaLimits()
		{
			ObjectLoader ldr = new _ObjectLoader_223();
			Assert.True(ldr.IsLarge(), "is large");
			try
			{
				ldr.GetCachedBytes(10);
				NUnit.Framework.Assert.Fail("Did not throw LargeObjectException");
			}
			catch (LargeObjectException)
			{
			}
			// Expected result.
			try
			{
				ldr.GetCachedBytes(int.MaxValue);
				NUnit.Framework.Assert.Fail("Did not throw LargeObjectException");
			}
			catch (LargeObjectException)
			{
			}
		}

		private sealed class _ObjectLoader_223 : ObjectLoader
		{
			public _ObjectLoader_223()
			{
			}

			public override bool IsLarge()
			{
				return true;
			}

			/// <exception cref="NGit.Errors.LargeObjectException"></exception>
			public override byte[] GetCachedBytes()
			{
				throw new LargeObjectException();
			}

			public override long GetSize()
			{
				return long.MaxValue;
			}

			public override int GetType()
			{
				return Constants.OBJ_BLOB;
			}

			/// <exception cref="NGit.Errors.MissingObjectException"></exception>
			/// <exception cref="System.IO.IOException"></exception>
			public override ObjectStream OpenStream()
			{
				return new _ObjectStream_247();
			}

			private sealed class _ObjectStream_247 : ObjectStream
			{
				public _ObjectStream_247()
				{
				}

				public override long GetSize()
				{
					return long.MaxValue;
				}

				public override int GetType()
				{
					return Constants.OBJ_BLOB;
				}

				/// <exception cref="System.IO.IOException"></exception>
				public override int Read()
				{
					NUnit.Framework.Assert.Fail("never should have reached read");
					return -1;
				}
			}
		}*/
		// Expected result.
	}
}
