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
using System.Collections.Generic;
using NGit;
using Xunit;

namespace NGit
{
	public class ObjectIdOwnerMapTest
	{
		private MutableObjectId idBuf;

		private ObjectIdOwnerMapTest.SubId id_1;

		private ObjectIdOwnerMapTest.SubId id_2;

		private ObjectIdOwnerMapTest.SubId id_3;

		private ObjectIdOwnerMapTest.SubId id_a31;

		private ObjectIdOwnerMapTest.SubId id_b31;

        public ObjectIdOwnerMapTest()
        {
			idBuf = new MutableObjectId();
			id_1 = new ObjectIdOwnerMapTest.SubId(Id(1));
			id_2 = new ObjectIdOwnerMapTest.SubId(Id(2));
			id_3 = new ObjectIdOwnerMapTest.SubId(Id(3));
			id_a31 = new ObjectIdOwnerMapTest.SubId(Id(31));
			id_b31 = new ObjectIdOwnerMapTest.SubId(Id((1 << 8) + 31));
        }

		[Fact]
		public virtual void TestEmptyMap()
		{
			ObjectIdOwnerMap<ObjectIdOwnerMapTest.SubId> m = new ObjectIdOwnerMap<ObjectIdOwnerMapTest.SubId
				>();
			Assert.True(m.IsEmpty());
			Assert.Equal<int>(0, m.Size());
            IEnumerator<ObjectIdOwnerMapTest.SubId> i = m.GetEnumerator();
            Assert.NotNull(i);
            Assert.False(i.MoveNext());
            Assert.False(m.Contains(Id(1)));
        }

		[Fact]
		public virtual void TestAddGetAndContains()
		{
			ObjectIdOwnerMap<ObjectIdOwnerMapTest.SubId> m = new ObjectIdOwnerMap<ObjectIdOwnerMapTest.SubId
				>();
			m.Add(id_1);
			m.Add(id_2);
			m.Add(id_3);
			m.Add(id_a31);
			m.Add(id_b31);
			Assert.False(m.IsEmpty());
			Assert.Equal<int>(5, m.Size());
			Assert.Same(id_1, m.Get(id_1));
			Assert.Same(id_1, m.Get(Id(1)));
			Assert.Same(id_1, m.Get(Id(1).Copy()));
			Assert.Same(id_2, m.Get(Id(2).Copy()));
			Assert.Same(id_3, m.Get(Id(3).Copy()));
			Assert.Same(id_a31, m.Get(Id(31).Copy()));
			Assert.Same(id_b31, m.Get(id_b31.Copy()));
			Assert.True(m.Contains(id_1));
		}

		[Fact]
		public virtual void TestClear()
		{
			ObjectIdOwnerMap<ObjectIdOwnerMapTest.SubId> m = new ObjectIdOwnerMap<ObjectIdOwnerMapTest.SubId
				>();
			m.Add(id_1);
			Assert.Same(id_1, m.Get(id_1));
			m.Clear();
			Assert.True(m.IsEmpty());
			Assert.Equal<int>(0, m.Size());
            IEnumerator<ObjectIdOwnerMapTest.SubId> i = m.GetEnumerator();
            Assert.NotNull(i);
            Assert.False(i.MoveNext());
            Assert.False(m.Contains(Id(1)));
        }

		[Fact]
		public virtual void TestAddIfAbsent()
		{
			ObjectIdOwnerMap<ObjectIdOwnerMapTest.SubId> m = new ObjectIdOwnerMap<ObjectIdOwnerMapTest.SubId
				>();
			m.Add(id_1);
			Assert.Same(id_1, m.AddIfAbsent(new ObjectIdOwnerMapTest.SubId
				(id_1)));
			Assert.Equal<int>(1, m.Size());
			Assert.Same(id_2, m.AddIfAbsent(id_2));
			Assert.Equal<int>(2, m.Size());
			Assert.Same(id_a31, m.AddIfAbsent(id_a31));
			Assert.Same(id_b31, m.AddIfAbsent(id_b31));
			Assert.Same(id_a31, m.AddIfAbsent(new ObjectIdOwnerMapTest.SubId
				(id_a31)));
			Assert.Same(id_b31, m.AddIfAbsent(new ObjectIdOwnerMapTest.SubId
				(id_b31)));
			Assert.Equal<int>(4, m.Size());
		}

		[Fact]
		public virtual void TestAddGrowsWithObjects()
		{
			int n = 16384;
			ObjectIdOwnerMap<ObjectIdOwnerMapTest.SubId> m = new ObjectIdOwnerMap<ObjectIdOwnerMapTest.SubId
				>();
			m.Add(id_1);
			for (int i = 32; i < n; i++)
			{
				m.Add(new ObjectIdOwnerMapTest.SubId(Id(i)));
			}
			Assert.Equal<int>(n - 32 + 1, m.Size());
			Assert.Same(id_1, m.Get(id_1.Copy()));
			for (int i_1 = 32; i_1 < n; i_1++)
			{
				Assert.True(m.Contains(Id(i_1)));
			}
		}

		[Fact]
		public virtual void TestAddIfAbsentGrowsWithObjects()
		{
			int n = 16384;
			ObjectIdOwnerMap<ObjectIdOwnerMapTest.SubId> m = new ObjectIdOwnerMap<ObjectIdOwnerMapTest.SubId
				>();
			m.Add(id_1);
			for (int i = 32; i < n; i++)
			{
				m.AddIfAbsent(new ObjectIdOwnerMapTest.SubId(Id(i)));
			}
			Assert.Equal<int>(n - 32 + 1, m.Size());
			Assert.Same(id_1, m.Get(id_1.Copy()));
			for (int i_1 = 32; i_1 < n; i_1++)
			{
				Assert.True(m.Contains(Id(i_1)));
			}
		}

		[Fact]
		public virtual void TestIterator()
		{
			ObjectIdOwnerMap<ObjectIdOwnerMapTest.SubId> m = new ObjectIdOwnerMap<ObjectIdOwnerMapTest.SubId
				>();
			m.Add(id_1);
			m.Add(id_2);
			m.Add(id_3);
			IEnumerator<ObjectIdOwnerMapTest.SubId> i = m.GetEnumerator();
			Assert.True(i.MoveNext());
			Assert.Same(id_1, i.Current);
			Assert.True(i.MoveNext());
			Assert.Same(id_2, i.Current);
			Assert.True(i.MoveNext());
			Assert.Same(id_3, i.Current);
			Assert.False(i.MoveNext());
			// OK
			i = m.GetEnumerator();
			Assert.Same(id_1, i.Current);
            Assert.Throws<NotImplementedException>(() => i.Reset());
		}

        // TODO: add test to suit C# IEnumerator Current

		// OK
		private AnyObjectId Id(int val)
		{
			idBuf.SetByte(0, (byte)(val & 0xff));
			idBuf.SetByte(3, (byte)(((int)(((uint)val) >> 8)) & 0xff));
			return idBuf;
		}

		[System.Serializable]
		internal class SubId : ObjectIdOwnerMap.Entry
		{
			public SubId(AnyObjectId id) : base(id)
			{
			}
		}
	}
}
