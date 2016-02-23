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

using System.IO;
using System.Linq;
using NGit.Util.IO;
using NSubstitute;
using Xunit;

namespace NGit.Util.IO
{
    public class UnionInputStreamTest
    {
        /// <exception cref="System.IO.IOException"></exception>
        [Fact]
        public virtual void TestEmptyStream()
        {
            UnionInputStream u = new UnionInputStream();
            Assert.True(u.IsEmpty());
            Assert.Equal<int>(-1, u.ReadByte());
            Assert.Equal<int>(-1, u.Read(new byte[1], 0, 1));
            Assert.Equal<int>(-1, u.Read(new byte[1], 0, 0));
            u.Dispose();
        }

        /// <exception cref="System.IO.IOException"></exception>
        [Fact]
        public virtual void TestReadSingleBytes()
        {
            UnionInputStream u = new UnionInputStream();
            Assert.True(u.IsEmpty());
            u.Add(new MemoryStream(new byte[] { 1, 0, 2 }));
            u.Add(new MemoryStream(new byte[] { 3 }));
            u.Add(new MemoryStream(new byte[] { 4, 5 }));
            Assert.False(u.IsEmpty());
            Assert.Equal<int>(1, u.ReadByte());
            Assert.Equal<int>(0, u.ReadByte());
            Assert.Equal<int>(2, u.ReadByte());
            Assert.Equal<int>(3, u.ReadByte());
            Assert.Equal<int>(4, u.ReadByte());
            Assert.Equal<int>(5, u.ReadByte());
            Assert.Equal<int>(-1, u.ReadByte());
            Assert.True(u.IsEmpty());
            u.Add(new MemoryStream(new byte[] { 255 }));
            Assert.Equal<int>(255, u.ReadByte());
            Assert.Equal<int>(-1, u.ReadByte());
            Assert.True(u.IsEmpty());
        }

        /// <exception cref="System.IO.IOException"></exception>
        [Fact]
        public virtual void TestReadByteBlocks()
        {
            UnionInputStream u = new UnionInputStream();
            u.Add(new MemoryStream(new byte[] { 1, 0, 2 }));
            u.Add(new MemoryStream(new byte[] { 3 }));
            u.Add(new MemoryStream(new byte[] { 4, 5 }));
            byte[] r = new byte[5];
            Assert.Equal<int>(3, u.Read(r, 0, 5));
            Assert.True((new byte[] { 1, 0, 2 }).SequenceEqual(r.Take(3)));
            Assert.Equal<int>(1, u.Read(r, 0, 5));
            Assert.Equal<int>(3, r[0]);
            Assert.Equal<int>(2, u.Read(r, 0, 5));
            Assert.True((new byte[] { 4, 5 }).SequenceEqual(r.Take(2)));
            Assert.Equal<int>(-1, u.Read(r, 0, 5));
        }

        /// <exception cref="System.IO.IOException"></exception>
        [Fact]
        public virtual void TestArrayConstructor()
        {
            UnionInputStream u = new UnionInputStream(new MemoryStream(new byte[] { 1
                , 0, 2 }), new MemoryStream(new byte[] { 3 }), new MemoryStream(
                new byte[] { 4, 5 }));
            byte[] r = new byte[5];
            Assert.Equal<int>(3, u.Read(r, 0, 5));
            Assert.True((new byte[] { 1, 0, 2 }).SequenceEqual(r.Take(3)));
            Assert.Equal<int>(1, u.Read(r, 0, 5));
            Assert.Equal<int>(3, r[0]);
            Assert.Equal<int>(2, u.Read(r, 0, 5));
            Assert.True((new byte[] { 4, 5 }).SequenceEqual(r.Take(2)));
            Assert.Equal<int>(-1, u.Read(r, 0, 5));
        }

        /// <exception cref="System.IO.IOException"></exception>
        [Fact]
        public virtual void TestAutoDisposeDuringRead()
        {
            UnionInputStream u = new UnionInputStream();
            bool[] closed = new bool[2];
            var ms1 = Substitute.ForPartsOf<MemoryStream>(new byte[] { 1 });
            var ms2 = Substitute.ForPartsOf<MemoryStream>(new byte[] { 2 });
            u.Add(ms1);
            u.Add(ms2);
            ms1.DidNotReceive().Dispose();
            ms2.DidNotReceive().Dispose();
            Assert.Equal<int>(1, u.ReadByte());
            ms1.DidNotReceive().Dispose();
            ms2.DidNotReceive().Dispose();
            Assert.Equal<int>(2, u.ReadByte());
            ms1.Received().Dispose();
            ms2.DidNotReceive().Dispose();
            Assert.Equal<int>(-1, u.ReadByte());
            ms1.Received().Dispose();
            ms2.Received().Dispose();
        }


        /// <exception cref="System.IO.IOException"></exception>
        [Fact]
        public virtual void TestCloseDuringClose()
        {
            UnionInputStream u = new UnionInputStream();
            var ms1 = Substitute.ForPartsOf<MemoryStream>(new byte[] { 1 });
            var ms2 = Substitute.ForPartsOf<MemoryStream>(new byte[] { 2 });
            u.Add(ms1);
            u.Add(ms2);
            ms1.DidNotReceive().Dispose();
            ms2.DidNotReceive().Dispose();
            u.Close();
            ms1.Received().Dispose();
            ms2.Received().Dispose();
        }

        /// <exception cref="System.IO.IOException"></exception>
        [Fact]
        public virtual void TestCloseDuringClose2()
        {
            UnionInputStream u = new UnionInputStream();
            var ms1 = Substitute.ForPartsOf<MemoryStream>(new byte[] { 1 });
            var ms2 = Substitute.ForPartsOf<MemoryStream>(new byte[] { 2 });
            u.Add(ms1);
            u.Add(ms2);
            ms1.DidNotReceive().Dispose();
            ms2.DidNotReceive().Dispose();
            Assert.Equal<int>(1, u.ReadByte()); // we consume one
            u.Close();
            ms1.Received().Dispose();
            ms2.Received().Dispose();
        }

        [Fact]
        public virtual void TestExceptionDuringClose()
        {
            UnionInputStream u = new UnionInputStream();
            var ms1 = Substitute.ForPartsOf<MemoryStream>(new byte[] { 1 });
            ms1.When(x => x.Dispose()).Throw(new IOException("I AM A TEST"));
            u.Add(ms1);

            try
            {
                u.Close();
                throw new System.Exception("close ignored inner stream exception");
            }
            catch (IOException e)
            {
                Assert.Equal<string>("I AM A TEST", e.Message);
            }
        }


        /// <exception cref="System.Exception"></exception>
        [Fact]
        public virtual void TestNonBlockingPartialRead()
        {
            byte[] buf = new byte[10];
            var ms1 = Substitute.ForPartsOf<MemoryStream>();
            ms1.When(x => x.Read(buf, 0, 1)).Throw(new IOException("Expected"));

            UnionInputStream u = new UnionInputStream(new MemoryStream(new byte[] { 1
                        , 2, 3 }), ms1);

            Assert.Equal<int>(3, u.Read(buf, 0, 10));
            Assert.True((new byte[] { 1, 2, 3 }).SequenceEqual(buf.Take(3)));
            try
            {
                u.Read(buf, 0, 1);
                throw new System.Exception("Expected exception from errorReadStream");
            }
            catch (IOException e)
            {
                Assert.Equal<string>("Expected", e.Message);
            }
        }
    }
}
