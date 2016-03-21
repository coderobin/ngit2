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
using System.Collections;
using System.Collections.Generic;
using NGit;

namespace NGit
{
	/// <summary>
	/// Fast, efficient map for
	/// <see cref="ObjectId">ObjectId</see>
	/// subclasses in only one map.
	/// <p>
	/// To use this map type, applications must have their entry value type extend
	/// from
	/// <see cref="Entry">Entry</see>
	/// , which itself extends from ObjectId.
	/// <p>
	/// Object instances may only be stored in <b>ONE</b> ObjectIdOwnerMap. This
	/// restriction exists because the map stores internal map state within each
	/// object instance. If an instance is be placed in another ObjectIdOwnerMap it
	/// could corrupt one or both map's internal state.
	/// <p>
	/// If an object instance must be in more than one map, applications may use
	/// ObjectIdOwnerMap for one of the maps, and
	/// <see cref="ObjectIdSubclassMap{T}">ObjectIdSubclassMap&lt;T&gt;</see>
	/// for the
	/// other map(s). It is encouraged to use ObjectIdOwnerMap for the map that is
	/// accessed most often, as this implementation runs faster than the more general
	/// ObjectIdSubclassMap implementation.
	/// </summary>
	/// <?></?>
	public class ObjectIdOwnerMap<T> : IEnumerable<T> where T:ObjectIdOwnerMap.Entry
	{
		/// <summary>Size of the initial directory, will grow as necessary.</summary>
		/// <remarks>Size of the initial directory, will grow as necessary.</remarks>
		private const int INITIAL_DIRECTORY = 1024;

		/// <summary>Number of bits in a segment's index.</summary>
		/// <remarks>Number of bits in a segment's index. Segments are 2^11 in size.</remarks>
		private const int SEGMENT_BITS = 11;

		private const int SEGMENT_SHIFT = 32 - SEGMENT_BITS;

		/// <summary>Top level directory of the segments.</summary>
		/// <remarks>
		/// Top level directory of the segments.
		/// <p>
		/// The low
		/// <see cref="ObjectIdOwnerMap{T}.bits">ObjectIdOwnerMap&lt;T&gt;.bits</see>
		/// of the SHA-1 are used to select the segment from
		/// this directory. Each segment is constant sized at 2^SEGMENT_BITS.
		/// </remarks>
		private T[][] directory;

		/// <summary>Total number of objects in this map.</summary>
		/// <remarks>Total number of objects in this map.</remarks>
		private int size;

		/// <summary>
		/// The map doubles in capacity when
		/// <see cref="ObjectIdOwnerMap{T}.size">ObjectIdOwnerMap&lt;T&gt;.size</see>
		/// reaches this target.
		/// </summary>
		private int grow;

		/// <summary>
		/// Number of low bits used to form the index into
		/// <see cref="ObjectIdOwnerMap{T}.directory">ObjectIdOwnerMap&lt;T&gt;.directory</see>
		/// .
		/// </summary>
		private int bits;

		/// <summary>
		/// Low bit mask to index into
		/// <see cref="ObjectIdOwnerMap{T}.directory">ObjectIdOwnerMap&lt;T&gt;.directory</see>
		/// ,
		/// <code>2^bits-1</code>
		/// .
		/// </summary>
		private int mask;

		/// <summary>Create an empty map.</summary>
		/// <remarks>Create an empty map.</remarks>
		public ObjectIdOwnerMap()
		{
			bits = 0;
			mask = 0;
			grow = ComputeGrowAt(bits);
			directory = new T[INITIAL_DIRECTORY][];
			directory[0] = NewSegment();
		}

		/// <summary>Remove all entries from this map.</summary>
		/// <remarks>Remove all entries from this map.</remarks>
		public virtual void Clear()
		{
			size = 0;
			foreach (T[] tbl in directory)
			{
				if (tbl == null)
				{
					break;
				}
                Array.Clear(tbl, 0, tbl.Length);
			}
		}

		/// <summary>Lookup an existing mapping.</summary>
		/// <remarks>Lookup an existing mapping.</remarks>
		/// <param name="toFind">the object identifier to find.</param>
		/// <returns>the instance mapped to toFind, or null if no mapping exists.</returns>
		public virtual T Get(AnyObjectId toFind)
		{
			int h = toFind.GetHashCode();
			T obj = directory[h & mask][(int)(((uint)h) >> SEGMENT_SHIFT)];
			for (; obj != null; obj = (T)obj.next)
			{
				if (Equals(obj, toFind))
				{
					return obj;
				}
			}
			return null;
		}

		/// <summary>Returns true if this map contains the specified object.</summary>
		/// <remarks>Returns true if this map contains the specified object.</remarks>
		/// <param name="toFind">object to find.</param>
		/// <returns>true if the mapping exists for this object; false otherwise.</returns>
		public virtual bool Contains(AnyObjectId toFind)
		{
			return Get(toFind) != null;
		}

		/// <summary>Store an object for future lookup.</summary>
		/// <remarks>
		/// Store an object for future lookup.
		/// <p>
		/// An existing mapping for <b>must not</b> be in this map. Callers must
		/// first call
		/// <see cref="ObjectIdOwnerMap{T}.Get(AnyObjectId)">ObjectIdOwnerMap&lt;T&gt;.Get(AnyObjectId)
		/// 	</see>
		/// to verify there is no current
		/// mapping prior to adding a new mapping, or use
		/// <see cref="ObjectIdOwnerMap{T}.AddIfAbsent{Q}(Entry)">ObjectIdOwnerMap&lt;T&gt;.AddIfAbsent&lt;Q&gt;(Entry)
		/// 	</see>
		/// .
		/// </remarks>
		/// <param name="newValue">the object to store.</param>
		/// <?></?>
		public virtual void Add<Q>(Q newValue) where Q:T
		{
			if (++size == grow)
			{
				Grow();
			}
			int h = ((T)newValue).GetHashCode();
			T[] table = directory[h & mask];
			h = (int)(((uint)h) >> SEGMENT_SHIFT);
			newValue.next = table[h];
			table[h] = newValue;
		}

		/// <summary>Store an object for future lookup.</summary>
		/// <remarks>
		/// Store an object for future lookup.
		/// <p>
		/// Stores
		/// <code>newValue</code>
		/// , but only if there is not already an object for
		/// the same object name. Callers can tell if the value is new by checking
		/// the return value with reference equality:
		/// <pre>
		/// V obj = ...;
		/// boolean wasNew = map.addIfAbsent(obj) == obj;
		/// </pre>
		/// </remarks>
		/// <param name="newValue">the object to store.</param>
		/// <returns>
		/// 
		/// <code>newValue</code>
		/// if stored, or the prior value already stored and
		/// that would have been returned had the caller used
		/// <code>get(newValue)</code>
		/// first.
		/// </returns>
		/// <?></?>
		public virtual T AddIfAbsent<Q>(Q newValue) where Q:T
		{
			int h = ((T)newValue).GetHashCode();
			T[] table = directory[h & mask];
			h = (int)(((uint)h) >> SEGMENT_SHIFT);
			for (T obj = table[h]; obj != null; obj = (T)obj.next)
			{
				if (Equals(obj, newValue))
				{
					return obj;
				}
			}
			newValue.next = table[h];
			table[h] = newValue;
			if (++size == grow)
			{
				Grow();
			}
			return newValue;
		}

		/// <returns>number of objects in this map.</returns>
		public virtual int Size()
		{
			return size;
		}

		/// <returns>
		/// true if
		/// <see cref="ObjectIdOwnerMap{V}.Size()">ObjectIdOwnerMap&lt;V&gt;.Size()</see>
		/// is 0.
		/// </returns>
		public virtual bool IsEmpty()
		{
			return size == 0;
		}

		private sealed class ObjectIdOwnerMapEnumerator : IEnumerator<T>
		{
			public ObjectIdOwnerMapEnumerator(ObjectIdOwnerMap<T> _enclosing)
			{
				this._enclosing = _enclosing;
			}

			private int dirIdx;

			private int tblIdx;

			private T current;

            public bool MoveNext()
            {
                if (this.current != null)
                {
                    if (current.next != null)
                    {
                        current = (T)current.next;
                        return true;
                    }
                    else
                    {
                        current = null;
                        return false;
                    }
                }
                for (;;)
                {
                    T[] table = this._enclosing.directory[this.dirIdx];
                    if (this.tblIdx == table.Length)
                    {
                        if (++this.dirIdx >= (1 << this._enclosing.bits))
                        {
                            current = null;
                            return false;
                        }
                        table = this._enclosing.directory[this.dirIdx];
                        this.tblIdx = 0;
                    }
                    while (this.tblIdx < table.Length)
                    {
                        T t = table[this.tblIdx++];
                        if (t != null)
                        {
                            current = t;
                            return true;
                        }
                    }
                }
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            private readonly ObjectIdOwnerMap<T> _enclosing;

            public T Current
            {
                get
                {
                    return this.current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.current;
                }
            }
        }

		private void Grow()
		{
			int oldDirLen = 1 << bits;
			int s = 1 << bits;
			bits++;
			mask = (1 << bits) - 1;
			grow = ComputeGrowAt(bits);
			// Quadruple the directory if it needs to expand. Expanding the
			// directory is expensive because it generates garbage, so try
			// to avoid doing it often.
			//
			int newDirLen = 1 << bits;
			if (directory.Length < newDirLen)
			{
				T[][] newDir = (T[][])new ObjectIdOwnerMap.Entry[newDirLen << 1][];
				System.Array.Copy(directory, 0, newDir, 0, oldDirLen);
				directory = newDir;
			}
			// For every bucket of every old segment, split the chain between
			// the old segment and the new segment's corresponding bucket. To
			// select between them use the lowest bit that was just added into
			// the mask above. This causes the table to double in capacity.
			//
			for (int dirIdx = 0; dirIdx < oldDirLen; dirIdx++)
			{
				T[] oldTable = directory[dirIdx];
				T[] newTable = NewSegment();
				for (int i = 0; i < oldTable.Length; i++)
				{
					T chain0 = null;
					T chain1 = null;
					T next;
					for (T obj = oldTable[i]; obj != null; obj = next)
					{
						next = (T)obj.next;
						if ((obj.GetHashCode() & s) == 0)
						{
							obj.next = chain0;
							chain0 = obj;
						}
						else
						{
							obj.next = chain1;
							chain1 = obj;
						}
					}
					oldTable[i] = chain0;
					newTable[i] = chain1;
				}
				directory[oldDirLen + dirIdx] = newTable;
			}
		}

		private T[] NewSegment()
		{
			return new T[1 << SEGMENT_BITS];
		}

		private static int ComputeGrowAt(int bits)
		{
			return 1 << (bits + SEGMENT_BITS);
		}

		private static bool Equals(AnyObjectId firstObjectId, AnyObjectId secondObjectId)
		{
            return firstObjectId.Equals(secondObjectId);
		}

        public IEnumerator<T> GetEnumerator()
        {
            return new ObjectIdOwnerMapEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
	
	public class ObjectIdOwnerMap
	{
		/// <summary>
		/// Type of entry stored in the
		/// <see cref="ObjectIdOwnerMap{V}">ObjectIdOwnerMap&lt;V&gt;</see>
		/// .
		/// </summary>
		[System.Runtime.Serialization.DataContract]
		public abstract class Entry : ObjectId
		{
			internal ObjectIdOwnerMap.Entry next;

			/// <summary>Initialize this entry with a specific ObjectId.</summary>
			/// <remarks>Initialize this entry with a specific ObjectId.</remarks>
			/// <param name="id">the id the entry represents.</param>
			protected internal Entry(AnyObjectId id) : base(id)
			{
			}
		}
	}
}
