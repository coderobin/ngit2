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

using System.Text;
using NGit;
using Xunit;

namespace NGit
{
	public class AbbreviatedObjectIdTest
	{
		[Fact]
		public virtual void TestEmpty_FromByteArray()
		{
			AbbreviatedObjectId i;
			i = AbbreviatedObjectId.FromString(new byte[] {  }, 0, 0);
			Assert.NotNull(i);
			Assert.Equal<int>(0, i.Length);
			Assert.False(i.IsComplete);
			Assert.Equal<string>(string.Empty, i.Name);
		}

		[Fact]
		public virtual void TestEmpty_FromString()
		{
			AbbreviatedObjectId i = AbbreviatedObjectId.FromString(string.Empty);
			Assert.NotNull(i);
			Assert.Equal<int>(0, i.Length);
			Assert.False(i.IsComplete);
			Assert.Equal<string>(string.Empty, i.Name);
		}

		[Fact]
		public virtual void TestFull_FromByteArray()
		{
			string s = "7b6e8067ec96acef9a4184b43210d583b6d2f99a";
			byte[] b = Encoding.ASCII.GetBytes(s);
			AbbreviatedObjectId i = AbbreviatedObjectId.FromString(b, 0, b.Length);
			Assert.NotNull(i);
			Assert.Equal<int>(s.Length, i.Length);
			Assert.True(i.IsComplete);
			Assert.Equal<string>(s, i.Name);
			ObjectId f = i.ToObjectId();
			Assert.NotNull(f);
			Assert.Equal<ObjectId>(ObjectId.FromString(s), f);
			Assert.Equal<int>(f.GetHashCode(), i.GetHashCode());
		}

		[Fact]
		public virtual void TestFull_FromString()
		{
			string s = "7b6e8067ec96acef9a4184b43210d583b6d2f99a";
			AbbreviatedObjectId i = AbbreviatedObjectId.FromString(s);
			Assert.NotNull(i);
			Assert.Equal<int>(s.Length, i.Length);
			Assert.True(i.IsComplete);
			Assert.Equal<string>(s, i.Name);
			ObjectId f = i.ToObjectId();
			Assert.NotNull(f);
			Assert.Equal<ObjectId>(ObjectId.FromString(s), f);
			Assert.Equal<int>(f.GetHashCode(), i.GetHashCode());
		}

		[Theory]
        [InlineData("7")]
        [InlineData("7b")]
        [InlineData("7b6")]
        [InlineData("7b6e")]
        [InlineData("7b6e8")]
        [InlineData("7b6e80")]
        [InlineData("7b6e806")]
        [InlineData("7b6e8067")]
        [InlineData("7b6e8067e")]
        [InlineData("7b6e8067ec96acef9")]
        public virtual void TestPartial_FromString(string s)
		{
            System.Diagnostics.Debug.WriteLine(s);
			AbbreviatedObjectId i = AbbreviatedObjectId.FromString(s);
			Assert.NotNull(i);
			Assert.Equal<int>(s.Length, i.Length);
			Assert.False(i.IsComplete);
			Assert.Equal<string>(s, i.Name);
			Assert.Null(i.ToObjectId());
		}

		[Fact]
		public virtual void TestEquals_Short()
		{
			string s = "7b6e8067";
			AbbreviatedObjectId a = AbbreviatedObjectId.FromString(s);
			AbbreviatedObjectId b = AbbreviatedObjectId.FromString(s);
			Assert.NotSame(a, b);
			Assert.True(a.GetHashCode() == b.GetHashCode());
			Assert.Equal<AbbreviatedObjectId>(b, a);
			Assert.Equal<AbbreviatedObjectId>(a, b);
		}

		[Fact]
		public virtual void TestEquals_Full()
		{
			string s = "7b6e8067ec96acef9a4184b43210d583b6d2f99a";
			AbbreviatedObjectId a = AbbreviatedObjectId.FromString(s);
			AbbreviatedObjectId b = AbbreviatedObjectId.FromString(s);
			Assert.NotSame(a, b);
			Assert.True(a.GetHashCode() == b.GetHashCode());
			Assert.Equal<AbbreviatedObjectId>(b, a);
			Assert.Equal<AbbreviatedObjectId>(a, b);
		}

		[Fact]
		public virtual void TestNotEquals_SameLength()
		{
			string sa = "7b6e8067";
			string sb = "7b6e806e";
			AbbreviatedObjectId a = AbbreviatedObjectId.FromString(sa);
			AbbreviatedObjectId b = AbbreviatedObjectId.FromString(sb);
			Assert.False(a.Equals(b));
			Assert.False(b.Equals(a));
		}

		[Fact]
		public virtual void TestNotEquals_DiffLength()
		{
			string sa = "7b6e8067abcd";
			string sb = "7b6e8067";
			AbbreviatedObjectId a = AbbreviatedObjectId.FromString(sa);
			AbbreviatedObjectId b = AbbreviatedObjectId.FromString(sb);
			Assert.False(a.Equals(b));
			Assert.False(b.Equals(a));
		}

		[Fact]
		public virtual void TestPrefixCompare_Full()
		{
			string s1 = "7b6e8067ec96acef9a4184b43210d583b6d2f99a";
			AbbreviatedObjectId a = AbbreviatedObjectId.FromString(s1);
			ObjectId i1 = ObjectId.FromString(s1);
			Assert.Equal<int>(0, a.PrefixCompare(i1));
			Assert.True(i1.StartsWith(a));
			string s2 = "7b6e8067ec96acef9a4184b43210d583b6d2f99b";
			ObjectId i2 = ObjectId.FromString(s2);
			Assert.True(a.PrefixCompare(i2) < 0);
			Assert.False(i2.StartsWith(a));
			string s3 = "7b6e8067ec96acef9a4184b43210d583b6d2f999";
			ObjectId i3 = ObjectId.FromString(s3);
			Assert.True(a.PrefixCompare(i3) > 0);
			Assert.False(i3.StartsWith(a));
		}

		[Fact]
		public virtual void TestPrefixCompare_1()
		{
			string sa = "7";
			AbbreviatedObjectId a = AbbreviatedObjectId.FromString(sa);
			string s1 = "7b6e8067ec96acef9a4184b43210d583b6d2f99a";
			ObjectId i1 = ObjectId.FromString(s1);
			Assert.Equal<int>(0, a.PrefixCompare(i1));
			Assert.True(i1.StartsWith(a));
			string s2 = "8b6e8067ec96acef9a4184b43210d583b6d2f99a";
			ObjectId i2 = ObjectId.FromString(s2);
			Assert.True(a.PrefixCompare(i2) < 0);
			Assert.False(i2.StartsWith(a));
			string s3 = "6b6e8067ec96acef9a4184b43210d583b6d2f99a";
			ObjectId i3 = ObjectId.FromString(s3);
			Assert.True(a.PrefixCompare(i3) > 0);
			Assert.False(i3.StartsWith(a));
		}

		[Fact]
		public virtual void TestPrefixCompare_7()
		{
			string sa = "7b6e806";
			AbbreviatedObjectId a = AbbreviatedObjectId.FromString(sa);
			string s1 = "7b6e8067ec96acef9a4184b43210d583b6d2f99a";
			ObjectId i1 = ObjectId.FromString(s1);
			Assert.Equal<int>(0, a.PrefixCompare(i1));
			Assert.True(i1.StartsWith(a));
			string s2 = "7b6e8167ec86acef9a4184b43210d583b6d2f99a";
			ObjectId i2 = ObjectId.FromString(s2);
			Assert.True(a.PrefixCompare(i2) < 0);
			Assert.False(i2.StartsWith(a));
			string s3 = "7b6e8057eca6acef9a4184b43210d583b6d2f99a";
			ObjectId i3 = ObjectId.FromString(s3);
			Assert.True(a.PrefixCompare(i3) > 0);
			Assert.False(i3.StartsWith(a));
		}

		[Fact]
		public virtual void TestPrefixCompare_8()
		{
			string sa = "7b6e8067";
			AbbreviatedObjectId a = AbbreviatedObjectId.FromString(sa);
			string s1 = "7b6e8067ec96acef9a4184b43210d583b6d2f99a";
			ObjectId i1 = ObjectId.FromString(s1);
			Assert.Equal<int>(0, a.PrefixCompare(i1));
			Assert.True(i1.StartsWith(a));
			string s2 = "7b6e8167ec86acef9a4184b43210d583b6d2f99a";
			ObjectId i2 = ObjectId.FromString(s2);
			Assert.True(a.PrefixCompare(i2) < 0);
			Assert.False(i2.StartsWith(a));
			string s3 = "7b6e8057eca6acef9a4184b43210d583b6d2f99a";
			ObjectId i3 = ObjectId.FromString(s3);
			Assert.True(a.PrefixCompare(i3) > 0);
			Assert.False(i3.StartsWith(a));
		}

		[Fact]
		public virtual void TestPrefixCompare_9()
		{
			string sa = "7b6e8067e";
			AbbreviatedObjectId a = AbbreviatedObjectId.FromString(sa);
			string s1 = "7b6e8067ec96acef9a4184b43210d583b6d2f99a";
			ObjectId i1 = ObjectId.FromString(s1);
			Assert.Equal<int>(0, a.PrefixCompare(i1));
			Assert.True(i1.StartsWith(a));
			string s2 = "7b6e8167ec86acef9a4184b43210d583b6d2f99a";
			ObjectId i2 = ObjectId.FromString(s2);
			Assert.True(a.PrefixCompare(i2) < 0);
			Assert.False(i2.StartsWith(a));
			string s3 = "7b6e8057eca6acef9a4184b43210d583b6d2f99a";
			ObjectId i3 = ObjectId.FromString(s3);
			Assert.True(a.PrefixCompare(i3) > 0);
			Assert.False(i3.StartsWith(a));
		}

		[Fact]
		public virtual void TestPrefixCompare_17()
		{
			string sa = "7b6e8067ec96acef9";
			AbbreviatedObjectId a = AbbreviatedObjectId.FromString(sa);
			string s1 = "7b6e8067ec96acef9a4184b43210d583b6d2f99a";
			ObjectId i1 = ObjectId.FromString(s1);
			Assert.Equal<int>(0, a.PrefixCompare(i1));
			Assert.True(i1.StartsWith(a));
			string s2 = "7b6e8067eca6acef9a4184b43210d583b6d2f99a";
			ObjectId i2 = ObjectId.FromString(s2);
			Assert.True(a.PrefixCompare(i2) < 0);
			Assert.False(i2.StartsWith(a));
			string s3 = "7b6e8067ec86acef9a4184b43210d583b6d2f99a";
			ObjectId i3 = ObjectId.FromString(s3);
			Assert.True(a.PrefixCompare(i3) > 0);
			Assert.False(i3.StartsWith(a));
		}

		[Fact]
		public virtual void TestIsId()
		{
			// These are all too short.
			Assert.False(AbbreviatedObjectId.IsId(string.Empty));
			Assert.False(AbbreviatedObjectId.IsId("a"));
			// These are too long.
			Assert.False(AbbreviatedObjectId.IsId(ObjectId.FromString("7b6e8067ec86acef9a4184b43210d583b6d2f99a"
				).Name + "0"));
			Assert.False(AbbreviatedObjectId.IsId(ObjectId.FromString("7b6e8067ec86acef9a4184b43210d583b6d2f99a"
				).Name + "c0ffee"));
			// These contain non-hex characters.
			Assert.False(AbbreviatedObjectId.IsId("01notahexstring"));
			// These should all work.
			Assert.True(AbbreviatedObjectId.IsId("ab"));
			Assert.True(AbbreviatedObjectId.IsId("abc"));
			Assert.True(AbbreviatedObjectId.IsId("abcd"));
			Assert.True(AbbreviatedObjectId.IsId("abcd0"));
			Assert.True(AbbreviatedObjectId.IsId("abcd09"));
			Assert.True(AbbreviatedObjectId.IsId(ObjectId.FromString("7b6e8067ec86acef9a4184b43210d583b6d2f99a"
				).Name));
		}
    }
}
