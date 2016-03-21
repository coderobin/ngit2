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
using NGit.Revwalk;

// Dong Xie comments:
// Java's AbstractSet doesn't really look like a Set as in math, it
// does not offer any set ops. Maybe this class should use another base
// class, e.g. simply a list.

namespace NGit.Revwalk
{
	/// <summary>
	/// Multiple application level mark bits for
	/// <see cref="RevObject">RevObject</see>
	/// s.
	/// </summary>
	/// <seealso cref="RevFlag">RevFlag</seealso>
	public class RevFlagSet : ISet<RevFlag>
	{
		internal int mask;

		private readonly IList<RevFlag> active;
        private object flag;

        /// <summary>Create an empty set of flags.</summary>
        /// <remarks>Create an empty set of flags.</remarks>
        public RevFlagSet()
		{
			active = new List<RevFlag>();
		}

		/// <summary>Create a set of flags, copied from an existing set.</summary>
		/// <remarks>Create a set of flags, copied from an existing set.</remarks>
		/// <param name="s">the set to copy flags from.</param>
		public RevFlagSet(NGit.Revwalk.RevFlagSet s)
		{
			mask = s.mask;
			active = new List<RevFlag>(s.active);
		}

		/// <summary>Create a set of flags, copied from an existing collection.</summary>
		/// <remarks>Create a set of flags, copied from an existing collection.</remarks>
		/// <param name="s">the collection to copy flags from.</param>
		public RevFlagSet(ICollection<RevFlag> s) : this()
		{
            this.UnionWith(s);
		}

        // Dong Xie: this is not used according to ILSpy
		//public override bool ContainsAll (ICollection<object> c)
		//{
		//	if (c is NGit.Revwalk.RevFlagSet)
		//	{
		//		int cMask = ((NGit.Revwalk.RevFlagSet)c).mask;
		//		return (mask & cMask) == cMask;
		//	}
		//	return base.ContainsAll(c);
		//}

        public bool Add(RevFlag item)
        {
            if ((mask & item.mask) != 0)
            {
                return false;
            }
            mask |= item.mask;
            int p = 0;
            while (p < active.Count && active[p].mask < item.mask)
            {
                p++;
            }
            active.Insert(p, item);
            return true;
        }

        public void ExceptWith(IEnumerable<RevFlag> other)
        {
            throw new NotImplementedException();
        }

        public void IntersectWith(IEnumerable<RevFlag> other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSubsetOf(IEnumerable<RevFlag> other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSupersetOf(IEnumerable<RevFlag> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSubsetOf(IEnumerable<RevFlag> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSupersetOf(IEnumerable<RevFlag> other)
        {
            throw new NotImplementedException();
        }

        public bool Overlaps(IEnumerable<RevFlag> other)
        {
            throw new NotImplementedException();
        }

        public bool SetEquals(IEnumerable<RevFlag> other)
        {
            throw new NotImplementedException();
        }

        public void SymmetricExceptWith(IEnumerable<RevFlag> other)
        {
            throw new NotImplementedException();
        }

        public void UnionWith(IEnumerable<RevFlag> other)
        {
            throw new NotImplementedException();
        }

        void ICollection<RevFlag>.Add(RevFlag item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(RevFlag item)
        {
            return (mask & item.mask) != 0;
        }

        public void CopyTo(RevFlag[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(RevFlag item)
        {
            if ((mask & item.mask) == 0)
            {
                return false;
            }
            mask &= ~item.mask;
            for (int i = 0; i < active.Count; i++)
            {
                if (active[i].mask == item.mask)
                {
                    active.RemoveAt(i);
                }
            }
            return true;
        }

        public IEnumerator<RevFlag> GetEnumerator()
        {
            return active.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return active.GetEnumerator();
        }

		public int Count
		{
			get
			{
				return active.Count;
			}
		}

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }
    }
}
