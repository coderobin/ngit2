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
using System.Text;
using NGit.Util.IO;
using Xunit;

namespace NGit.Util.IO
{
	public class EolCanonicalizingInputStreamTest
	{
		/// <exception cref="System.IO.IOException"></exception>
		[Fact]
		public virtual void TestLF()
		{
			byte[] bytes = AsBytes("1\n2\n3");
			Test(bytes, bytes, false);
		}

		/// <exception cref="System.IO.IOException"></exception>
		[Fact]
		public virtual void TestCR()
		{
			byte[] bytes = AsBytes("1\r2\r3");
			Test(bytes, bytes, false);
		}

		/// <exception cref="System.IO.IOException"></exception>
		[Fact]
		public virtual void TestCRLF()
		{
			Test(AsBytes("1\r\n2\r\n3"), AsBytes("1\n2\n3"), false);
		}

		/// <exception cref="System.IO.IOException"></exception>
		[Fact]
		public virtual void TestLFCR()
		{
			byte[] bytes = AsBytes("1\n\r2\n\r3");
			Test(bytes, bytes, false);
		}

		/// <exception cref="System.IO.IOException"></exception>
		[Fact]
		public virtual void TestEmpty()
		{
			byte[] bytes = AsBytes(string.Empty);
			Test(bytes, bytes, false);
		}

		/// <exception cref="System.IO.IOException"></exception>
		[Fact]
		public virtual void TestBinaryDetect()
		{
			byte[] bytes = AsBytes("1\r\n2\r\n3\x0");
			Test(bytes, bytes, true);
		}

		/// <exception cref="System.IO.IOException"></exception>
		[Fact]
		public virtual void TestBinaryDontDetect()
		{
			Test(AsBytes("1\r\n2\r\n3\x0"), AsBytes("1\n2\n3\x0"), false);
		}

		/// <exception cref="System.IO.IOException"></exception>
		private void Test(byte[] input, byte[] expected, bool detectBinary)
		{
			Stream bis1 = new MemoryStream(input);
			Stream cis1 = new EolCanonicalizingInputStream(bis1, detectBinary);
			int index1 = 0;
			for (int b = cis1.ReadByte(); b != -1; b = cis1.ReadByte())
			{
				Assert.Equal<byte>(expected[index1], (byte)b);
				index1++;
			}
			Assert.Equal<int>(expected.Length, index1);
			for (int bufferSize = 1; bufferSize < 10; bufferSize++)
			{
				byte[] buffer = new byte[bufferSize];
				Stream bis2 = new MemoryStream(input);
				Stream cis2 = new EolCanonicalizingInputStream(bis2, detectBinary);
				int read = 0;
				for (int readNow = cis2.Read(buffer, 0, buffer.Length); readNow != 0 && read < expected
					.Length; readNow = cis2.Read(buffer, 0, buffer.Length))
				{
					for (int index2 = 0; index2 < readNow; index2++)
					{
						Assert.Equal<byte>(expected[read + index2], buffer[index2]);
					}
					read += readNow;
				}
				Assert.Equal<int>(expected.Length, read);
				cis2.Dispose();
			}
			cis1.Dispose();
		}

		private static byte[] AsBytes(string @in)
		{
            return Encoding.UTF8.GetBytes(@in);
		}
	}
}
