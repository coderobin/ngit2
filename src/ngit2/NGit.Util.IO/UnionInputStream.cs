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
using System.IO;

namespace NGit.Util.IO
{
    /// <summary>An InputStream which reads from one or more InputStreams.</summary>
    /// <remarks>
    /// An InputStream which reads from one or more InputStreams.
    /// <p>
    /// This stream may enter into an EOF state, returning -1 from any of the read
    /// methods, and then later successfully read additional bytes if a new
    /// InputStream is added after reaching EOF.
    /// <p>
    /// Currently this stream does not support the mark/reset APIs. If mark and later
    /// reset functionality is needed the caller should wrap this stream with a
    /// <see cref="Sharpen.BufferedInputStream">Sharpen.BufferedInputStream</see>
    /// .
    /// </remarks>
    public class UnionInputStream : Stream
    {
        private readonly Queue<Stream> streams = new Queue<Stream>();

        private Stream currentStream;

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
                return false;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
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

        /// <summary>Create an empty InputStream that is currently at EOF state.</summary>
        /// <remarks>Create an empty InputStream that is currently at EOF state.</remarks>
        public UnionInputStream()
        {
        }

        /// <summary>Create an InputStream that is a union of the individual streams.</summary>
        /// <remarks>
        /// Create an InputStream that is a union of the individual streams.
        /// <p>
        /// As each stream reaches EOF, it will be automatically closed before bytes
        /// from the next stream are read.
        /// </remarks>
        /// <param name="inputStreams">streams to be pushed onto this stream.</param>
        public UnionInputStream(params Stream[] inputStreams)
        {
            foreach (Stream s in inputStreams)
            {
                streams.Enqueue(s);
            }
        }

        /// <summary>Add the given InputStream onto the end of the stream queue.</summary>
        /// <remarks>
        /// Add the given InputStream onto the end of the stream queue.
        /// <p>
        /// When the stream reaches EOF it will be automatically closed.
        /// </remarks>
        /// <param name="in">the stream to add; must not be null.</param>
        public virtual void Add(Stream @in)
        {
            streams.Enqueue(@in);
        }

        /// <summary>Returns true if there are no more InputStreams in the stream queue.</summary>
        /// <remarks>
        /// Returns true if there are no more InputStreams in the stream queue.
        /// <p>
        /// If this method returns
        /// <code>true</code>
        /// then all read methods will signal EOF
        /// by returning -1, until another InputStream has been pushed into the queue
        /// with
        /// <see cref="Add(Sharpen.InputStream)">Add(Sharpen.InputStream)</see>
        /// .
        /// </remarks>
        /// <returns>true if there are no more streams to read from.</returns>
        public virtual bool IsEmpty()
        {
            return streams.Count == 0;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override int ReadByte()
        {
            if (currentStream == null)
            {
                if (streams.Count > 0)
                    currentStream = streams.Dequeue();
                else
                    return -1;
            }

            do
            {
                int r = currentStream.ReadByte();
                if (r >= 0)
                {
                    return r;
                }
                else
                {
                    currentStream.Dispose();
                    currentStream = null;

                    if (streams.Count == 0)
                        return -1;
                    else
                    {
                        currentStream = streams.Dequeue();
                        continue;
                    }
                }
            } while (true);
        }


        /// <exception cref="System.IO.IOException"></exception>
        public override int Read(byte[] b, int off, int len)
        {
            if (currentStream == null)
            {
                if (streams.Count > 0)
                    currentStream = streams.Dequeue();
                else
                    return -1;
            }

            do
            {
                int n = currentStream.Read(b, off, len);
                if (n > 0)
                {
                    return n;
                }
                else
                {
                    currentStream.Dispose();
                    currentStream = null;

                    if (streams.Count == 0)
                        return -1;
                    else
                    {
                        currentStream = streams.Dequeue();
                        continue;
                    }
                }
            } while (true);
        }

        /// <exception cref="System.IO.IOException"></exception>
        protected override void Dispose(bool disposing)
        {
            // if called before exhausted
            foreach(var s in streams)
            {
                if (s != null)
                    s.Dispose();
            }

            if (currentStream != null)
                currentStream.Dispose();
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
