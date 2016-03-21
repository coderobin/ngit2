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
using System.Globalization;
using System.Text;
using NGit;
using NGit.Util;

namespace NGit
{
    /// <summary>A combination of a person identity and time in Git.</summary>
    /// <remarks>
    /// A combination of a person identity and time in Git.
    /// Git combines Name + email + time + time zone to specify who wrote or
    /// committed something. e.g. From Pro Git
    /// $ git cat-file -p fdf4fc3
    /// tree d8329fc1cc938780ffdd9f94e0d364e0ea74f579
    /// author Scott Chacon<schacon@gmail.com> 1243040974 -0700
    /// committer Scott Chacon<schacon@gmail.com> 1243040974 -0700
    /// first commit
    /// </remarks>
    [System.Runtime.Serialization.DataContract]
	public class PersonIdent
	{
		private const long serialVersionUID = 1L;

		private readonly string name;

		private readonly string emailAddress;

		private readonly long when;

        /// <summary>
        /// Minutes, +/-, e.g. EST would be -300, i.e. -0500
        /// </summary>
		private readonly int tzOffset;

		/// <summary>
		/// Copy a
		/// <see cref="PersonIdent">PersonIdent</see>
		/// .
		/// </summary>
		/// <param name="pi">
		/// Original
		/// <see cref="PersonIdent">PersonIdent</see>
		/// </param>
		public PersonIdent(NGit.PersonIdent pi) : this(pi.GetName(), pi.GetEmailAddress()
			)
		{
		}

		/// <summary>
		/// Construct a new
		/// <see cref="PersonIdent">PersonIdent</see>
		/// with current time.
		/// </summary>
		/// <param name="aName"></param>
		/// <param name="aEmailAddress"></param>
		public PersonIdent(string aName, string aEmailAddress) : this(aName, aEmailAddress
			, DateTimeOffset.Now.ToUnixTimeSeconds())
		{
		}

		/// <summary>Copy a PersonIdent, but alter the clone's time stamp</summary>
		/// <param name="pi">
		/// original
		/// <see cref="PersonIdent">PersonIdent</see>
		/// </param>
		/// <param name="aWhen">local time stamp</param>
		/// <param name="aTZ">time zone</param>
		public PersonIdent(NGit.PersonIdent pi, long aWhen, int aTZ) : this(pi.GetName(), 
			pi.GetEmailAddress(), aWhen, aTZ)
		{
		}

		private PersonIdent(string aName, string aEmailAddress, long when) : this(aName, 
			aEmailAddress, when, (int)TimeZoneInfo.Local.GetUtcOffset(DateTimeOffset.FromUnixTimeSeconds(when)).TotalMinutes)
		{
		}

		/// <summary>
		/// Construct a
		/// <see cref="PersonIdent">PersonIdent</see>
		/// </summary>
		/// <param name="aName"></param>
		/// <param name="aEmailAddress"></param>
		/// <param name="aWhen">local time stamp</param>
		/// <param name="aTZ">time zone</param>
		public PersonIdent(string aName, string aEmailAddress, long aWhen, int aTZ)
		{
			if (aName == null)
			{
				throw new ArgumentException("Name of PersonIdent must not be null.");
			}
			if (aEmailAddress == null)
			{
				throw new ArgumentException("E-mail address of PersonIdent must not be null.");
			}
			name = aName;
			emailAddress = aEmailAddress;
			when = aWhen;
			tzOffset = aTZ;
		}

		/// <returns>Name of person</returns>
		public virtual string GetName()
		{
			return name;
		}

		/// <returns>email address of person</returns>
		public virtual string GetEmailAddress()
		{
			return emailAddress;
		}

		/// <returns>timestamp</returns>
		public virtual DateTime GetWhen()
		{
			return DateTimeOffset.FromUnixTimeSeconds(when).DateTime;
		}

		/// <returns>
		/// this person's declared time zone as minutes east of UTC. If the
		/// timezone is to the west of UTC it is negative.
		/// </returns>
		public virtual int GetTimeZoneOffset()
		{
			return tzOffset;
		}

		public override int GetHashCode()
		{
			int hc = GetEmailAddress().GetHashCode();
			hc *= 31;
			hc += (int)(when / 1000L);
			return hc;
		}

		public override bool Equals(object o)
		{
			if (o is NGit.PersonIdent)
			{
				NGit.PersonIdent p = (NGit.PersonIdent)o;
				return GetName().Equals(p.GetName()) && GetEmailAddress().Equals(p.GetEmailAddress
					()) && when / 1000L == p.when / 1000L;
			}
			return false;
		}

		/// <summary>Format for Git storage.</summary>
		/// <remarks>Format for Git storage.</remarks>
		/// <returns>a string in the git author format</returns>
		public virtual string ToExternalString()
		{
			StringBuilder r = new StringBuilder();
			r.Append(GetName());
			r.Append(" <");
			r.Append(GetEmailAddress());
			r.Append("> ");
			r.Append(when / 1000);
			r.Append(' ');
			AppendTimezone(r);
			return r.ToString();
		}

		private void AppendTimezone(StringBuilder r)
		{
			int offset = tzOffset;
			char sign;
			int offsetHours;
			int offsetMins;
			if (offset < 0)
			{
				sign = '-';
				offset = -offset;
			}
			else
			{
				sign = '+';
			}
			offsetHours = offset / 60;
			offsetMins = offset % 60;
			r.Append(sign);
			if (offsetHours < 10)
			{
				r.Append('0');
			}
			r.Append(offsetHours);
			if (offsetMins < 10)
			{
				r.Append('0');
			}
			r.Append(offsetMins);
		}

		public override string ToString()
		{
            throw new NotImplementedException();
		}
	}
}
