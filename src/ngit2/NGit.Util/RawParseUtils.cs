/*
Dong Xie 2016
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NGit;
using NGit.Util;

namespace NGit.Util
{
	/// <summary>Handy utility functions to parse raw object contents.</summary>
	/// <remarks>Handy utility functions to parse raw object contents.</remarks>
	public static class RawParseUtils
	{
        /// <summary>Locate the "committer " header line data.</summary>
        /// <remarks>Locate the "committer " header line data.</remarks>
        /// <param name="b">buffer to scan.</param>
        /// <param name="ptr">
        /// position in buffer to start the scan at. Most callers should
        /// pass 0 to ensure the scan starts from the beginning of the
        /// commit buffer and does not accidentally look at message body.
        /// </param>
        /// <returns>
        /// position just after the space in "committer ", so the first
        /// character of the committer's name. If no committer header can be
        /// located -1 is returned.
        /// </returns>
        public static int Committer(byte[] b, int ptr)
        {
            int sz = b.Length;
            if (ptr == 0)
            {
                ptr += 46;
            }
            // skip the "tree ..." line.
            while (ptr < sz && b[ptr] == 'p')
            {
                ptr += 48;
            }
            // skip this parent.
            if (ptr < sz && b[ptr] == 'a')
            {
                ptr = NextLF(b, ptr);
            }
            return Match(b, ptr, ObjectChecker.committer);
        }

        /// <summary>Locate the first position after the next LF.</summary>
		/// <remarks>
		/// Locate the first position after the next LF.
		/// <p>
		/// This method stops on the first '\n' it finds.
		/// </remarks>
		/// <param name="b">buffer to scan.</param>
		/// <param name="ptr">position within buffer to start looking for LF at.</param>
		/// <returns>new position just after the first LF found.</returns>
		public static int NextLF(byte[] b, int ptr)
        {
            return Next(b, ptr, '\n');
        }

        /// <summary>Locate the first position after either the given character or LF.</summary>
		/// <remarks>
		/// Locate the first position after either the given character or LF.
		/// <p>
		/// This method stops on the first match it finds from either chrA or '\n'.
		/// </remarks>
		/// <param name="b">buffer to scan.</param>
		/// <param name="ptr">position within buffer to start looking for chrA or LF at.</param>
		/// <param name="chrA">character to find.</param>
		/// <returns>new position just after the first chrA or LF to be found.</returns>
		public static int NextLF(byte[] b, int ptr, char chrA)
        {
            int sz = b.Length;
            while (ptr < sz)
            {
                byte c = b[ptr++];
                if (c == chrA || c == '\n')
                {
                    return ptr;
                }
            }
            return ptr;
        }

        /// <summary>Locate the first position after a given character.</summary>
		/// <remarks>Locate the first position after a given character.</remarks>
		/// <param name="b">buffer to scan.</param>
		/// <param name="ptr">position within buffer to start looking for chrA at.</param>
		/// <param name="chrA">character to find.</param>
		/// <returns>new position just after chrA.</returns>
		public static int Next(byte[] b, int ptr, char chrA)
        {
            int sz = b.Length;
            while (ptr < sz)
            {
                if (b[ptr++] == chrA)
                {
                    return ptr;
                }
            }
            return ptr;
        }

        /// <summary>Determine if b[ptr] matches src.</summary>
		/// <remarks>Determine if b[ptr] matches src.</remarks>
		/// <param name="b">the buffer to scan.</param>
		/// <param name="ptr">first position within b, this should match src[0].</param>
		/// <param name="src">the buffer to test for equality with b.</param>
		/// <returns>ptr + src.length if b[ptr..src.length] == src; else -1.</returns>
		public static int Match(byte[] b, int ptr, byte[] src)
        {
            if (ptr + src.Length > b.Length)
            {
                return -1;
            }
            for (int i = 0; i < src.Length; i++, ptr++)
            {
                if (b[ptr] != src[i])
                {
                    return -1;
                }
            }
            return ptr;
        }

        /// <summary>Parse a base 10 numeric from a sequence of ASCII digits into an int.</summary>
        /// <remarks>
        /// Parse a base 10 numeric from a sequence of ASCII digits into an int.
        /// <p>
        /// Digit sequences can begin with an optional run of spaces before the
        /// sequence, and may start with a '+' or a '-' to indicate sign position.
        /// Any other characters will cause the method to stop and return the current
        /// result to the caller.
        /// </remarks>
        /// <param name="b">buffer to scan.</param>
        /// <param name="ptr">position within buffer to start parsing digits at.</param>
        /// <param name="ptrResult">
        /// optional location to return the new ptr value through. If null
        /// the ptr value will be discarded.
        /// </param>
        /// <returns>
        /// the value at this location; 0 if the location is not a valid
        /// numeric.
        /// </returns>
        public static int ParseBase10(byte[] b, int ptr, ref int ptrResult)
        {
            return (int)ParseLongBase10(b, ptr, ref ptrResult);
        }

		public static long ParseLongBase10(byte[] b, int ptr, ref int ptrResult)
        {
            long r = 0;
            int sign = 0;
            int sz = b.Length;
            while (ptr < sz && b[ptr] == ' ') // skip space
            {
                ptr++;
            }
            if (ptr >= sz)
            {
                return 0;
            }
            switch (b[ptr])
            {
                case (byte)('-'):
                    {
                        sign = -1;
                        ptr++;
                        break;
                    }

                case (byte)('+'):
                    {
                        ptr++;
                        break;
                    }
            }
            while (ptr < sz)
            {
                byte v = b[ptr];
                if (v < '0' || v > '9')
                {
                    break;
                }
                r = (r * 10) + v - 48;
                ptr++;
            }
            ptrResult = ptr;

            return sign < 0 ? -r : r;
        }

        /// <summary>Locate the "author " header line data.</summary>
        /// <remarks>Locate the "author " header line data.</remarks>
        /// <param name="b">buffer to scan.</param>
        /// <param name="ptr">
        /// position in buffer to start the scan at. Most callers should
        /// pass 0 to ensure the scan starts from the beginning of the
        /// commit buffer and does not accidentally look at message body.
        /// </param>
        /// <returns>
        /// position just after the space in "author ", so the first
        /// character of the author's name. If no author header can be
        /// located -1 is returned.
        /// </returns>
        public static int Author(byte[] b, int ptr)
        {
            int sz = b.Length;
            if (ptr == 0)
            {
                ptr += 46;
            }
            // skip the "tree ..." line.
            while (ptr < sz && b[ptr] == 'p')
            {
                ptr += 48;
            }
            // skip this parent.
            return Match(b, ptr, ObjectChecker.author);
        }

        /// <summary>Parse a name line (e.g.</summary>
        /// <remarks>
        /// Parse a name line (e.g. author, committer, tagger) into a PersonIdent.
        /// <p>
        /// When passing in a value for <code>nameB</code> callers should use the
        /// return value of
        /// <see cref="Author(byte[], int)">Author(byte[], int)</see>
        /// or
        /// <see cref="Committer(byte[], int)">Committer(byte[], int)</see>
        /// , as these methods provide the proper
        /// position within the buffer.
        /// </remarks>
        /// <param name="raw">the buffer to parse character data from.</param>
        /// <param name="nameB">
        /// first position of the identity information. This should be the
        /// first position after the space which delimits the header field
        /// name (e.g. "author" or "committer") from the rest of the
        /// identity line.
        /// </param>
        /// <returns>
        /// the parsed identity or null in case the identity could not be
        /// parsed.
        /// </returns>
        public static PersonIdent ParsePersonIdent(byte[] raw, int nameB)
        {
            System.Text.Encoding cs = ParseEncoding(raw);
            int emailB = NextLF(raw, nameB, '<');
            int emailE = NextLF(raw, emailB, '>');
            if (emailB >= raw.Length || raw[emailB] == '\n' || (emailE >= raw.Length - 1 && raw
                [emailE - 1] != '>'))
            {
                return null;
            }
            int nameEnd = emailB - 2 >= nameB && raw[emailB - 2] == ' ' ? emailB - 2 : emailB
                 - 1;
            string name = Decode(cs, raw, nameB, nameEnd);
            string email = Decode(cs, raw, emailB, emailE - 1);
            // Start searching from end of line, as after first name-email pair,
            // another name-email pair may occur. We will ignore all kinds of
            // "junk" following the first email.
            //
            // We've to use (emailE - 1) for the case that raw[email] is LF,
            // otherwise we would run too far. "-2" is necessary to position
            // before the LF in case of LF termination resp. the penultimate
            // character if there is no trailing LF.
            int tzBegin = LastIndexOfTrim(raw, ' ', NextLF(raw, emailE - 1) - 2) + 1;
            if (tzBegin <= emailE)
            {
                // No time/zone, still valid
                return new PersonIdent(name, email, 0, 0);
            }
            int whenBegin = Math.Max(emailE, LastIndexOfTrim(raw, ' ', tzBegin - 1) + 1);
            if (whenBegin >= tzBegin - 1)
            {
                // No time/zone, still valid
                return new PersonIdent(name, email, 0, 0);
            }
            int ptrOut = 0;
            long when = ParseLongBase10(raw, whenBegin, ref ptrOut);
            int tz = ParseTimeZoneOffset(raw, tzBegin);
            return new PersonIdent(name, email, when * 1000L, tz);
        }

        /// <summary>Locate the "encoding " header line.</summary>
        /// <remarks>Locate the "encoding " header line.</remarks>
        /// <param name="b">buffer to scan.</param>
        /// <param name="ptr">
        /// position in buffer to start the scan at. Most callers should
        /// pass 0 to ensure the scan starts from the beginning of the
        /// buffer and does not accidentally look at the message body.
        /// </param>
        /// <returns>
        /// position just after the space in "encoding ", so the first
        /// character of the encoding's name. If no encoding header can be
        /// located -1 is returned (and UTF-8 should be assumed).
        /// </returns>
        public static int Encoding(byte[] b, int ptr)
        {
            int sz = b.Length;
            while (ptr < sz)
            {
                if (b[ptr] == '\n')
                {
                    return -1;
                }
                if (b[ptr] == 'e')
                {
                    break;
                }
                ptr = NextLF(b, ptr);
            }
            return Match(b, ptr, ObjectChecker.encoding);
        }

        /// <summary>Parse the "encoding " header into a character set reference.</summary>
        /// <remarks>
        /// Parse the "encoding " header into a character set reference.
        /// <p>
        /// Locates the "encoding " header (if present) by first calling
        /// <see cref="Encoding(byte[], int)">Encoding(byte[], int)</see>
        /// and then returns the proper character set
        /// to apply to this buffer to evaluate its contents as character data.
        /// <p>
        /// If no encoding header is present,
        /// <see cref="NGit.Constants.CHARSET">NGit.Constants.CHARSET</see>
        /// is assumed.
        /// </remarks>
        /// <param name="b">buffer to scan.</param>
        /// <returns>the Java character set representation. Never null.</returns>
        public static System.Text.Encoding ParseEncoding(byte[] b)
        {
            int enc = Encoding(b, 0);
            if (enc < 0)
            {
                return Constants.CHARSET;
            }
            int lf = NextLF(b, enc);
            string decoded = Decode(Constants.CHARSET, b, enc, lf - 1);
            try
            {
                return System.Text.Encoding.GetEncoding(decoded);
            }
            catch (ArgumentException)
            {
                if (decoded.ToLower() == "latin-1")
                    return System.Text.Encoding.GetEncoding("ISO-8859-1");
                throw;
            }
        }

        /// <summary>Decode a region of the buffer under the specified character set if possible.
		/// 	</summary>
		/// <remarks>
		/// Decode a region of the buffer under the specified character set if possible.
		/// If the byte stream cannot be decoded that way, the platform default is tried
		/// and if that too fails, the fail-safe ISO-8859-1 encoding is tried.
		/// </remarks>
		/// <param name="cs">character set to use when decoding the buffer.</param>
		/// <param name="buffer">buffer to pull raw bytes from.</param>
		/// <param name="start">first position within the buffer to take data from.</param>
		/// <param name="end">
		/// one position past the last location within the buffer to take
		/// data from.
		/// </param>
		/// <returns>
		/// a string representation of the range <code>[start,end)</code>,
		/// after decoding the region through the specified character set.
		/// </returns>
		public static string Decode(System.Text.Encoding cs, byte[] buffer, int start, int
             end)
        {
            try
            {
                return DecodeNoFallback(cs, buffer, start, end);
            }
            catch (DecoderFallbackException)
            {
                // Fall back to an ISO-8859-1 style encoding. At least all of
                // the bytes will be present in the output.
                //
                return ExtractBinaryString(buffer, start, end);
            }
        }

        /// <summary>
        /// Decode a region of the buffer under the specified character set if
        /// possible.
        /// </summary>
        /// <remarks>
        /// Decode a region of the buffer under the specified character set if
        /// possible.
        /// If the byte stream cannot be decoded that way, the platform default is
        /// tried and if that too fails, an exception is thrown.
        /// </remarks>
        /// <param name="cs">character set to use when decoding the buffer.</param>
        /// <param name="buffer">buffer to pull raw bytes from.</param>
        /// <param name="start">first position within the buffer to take data from.</param>
        /// <param name="end">
        /// one position past the last location within the buffer to take
        /// data from.
        /// </param>
        /// <returns>
        /// a string representation of the range <code>[start,end)</code>,
        /// after decoding the region through the specified character set.
        /// </returns>
        /// <exception cref="Sharpen.CharacterCodingException">the input is not in any of the tested character sets.
        /// 	</exception>
        public static string DecodeNoFallback(System.Text.Encoding cs, byte[] buffer, int
             start, int end)
        {
            // Try our built-in favorite. The assumption here is that
            // decoding will fail if the data is not actually encoded
            // using that encoder.
            //
            try
            {
                return Decode(buffer, Constants.CHARSET, start, end);
            }
            catch (DecoderFallbackException)
            {
            }
            if (!cs.Equals(Constants.CHARSET))
            {
                // Try the suggested encoding, it might be right since it was
                // provided by the caller.
                //
                try
                {
                    return Decode(buffer, cs, start, end);
                }
                catch (DecoderFallbackException)
                {
                }
            }
            // Try the default character set. A small group of people
            // might actually use the same (or very similar) locale.
            //
            System.Text.Encoding defcs = System.Text.Encoding.GetEncoding(0);
            if (!defcs.Equals(cs) && !defcs.Equals(Constants.CHARSET))
            {
                try
                {
                    return Decode(buffer, defcs, start, end);
                }
                catch (DecoderFallbackException)
                {
                }
            }
            throw new DecoderFallbackException();
        }

        /// <summary>Decode a buffer under UTF-8, if possible.</summary>
		/// <remarks>
		/// Decode a buffer under UTF-8, if possible.
		/// If the byte stream cannot be decoded that way, the platform default is
		/// tried and if that too fails, the fail-safe ISO-8859-1 encoding is tried.
		/// </remarks>
		/// <param name="buffer">buffer to pull raw bytes from.</param>
		/// <param name="start">start position in buffer</param>
		/// <param name="end">
		/// one position past the last location within the buffer to take
		/// data from.
		/// </param>
		/// <returns>
		/// a string representation of the range <code>[start,end)</code>,
		/// after decoding the region through the specified character set.
		/// </returns>
		public static string Decode(byte[] buffer, int start, int end)
        {
            return Decode(Constants.CHARSET, buffer, start, end);
        }

        private static string Decode(byte[] b, System.Text.Encoding charset, int start, int end)
        {
            var decoder = charset.GetDecoder();
            int count = decoder.GetCharCount(b, start, end - start);
            char[] chars = new char[count];
            decoder.GetChars(b, start, end - start, chars, 0);
            return new string(chars);
        }

        /// <summary>Decode a region of the buffer under the ISO-8859-1 encoding.</summary>
		/// <remarks>
		/// Decode a region of the buffer under the ISO-8859-1 encoding.
		/// Each byte is treated as a single character in the 8859-1 character
		/// encoding, performing a raw binary-&gt;char conversion.
		/// </remarks>
		/// <param name="buffer">buffer to pull raw bytes from.</param>
		/// <param name="start">first position within the buffer to take data from.</param>
		/// <param name="end">
		/// one position past the last location within the buffer to take
		/// data from.
		/// </param>
		/// <returns>a string representation of the range <code>[start,end)</code>.</returns>
		public static string ExtractBinaryString(byte[] buffer, int start, int end)
        {
            StringBuilder r = new StringBuilder(end - start);
            for (int i = start; i < end; i++)
            {
                r.Append((char)(buffer[i] & 0xff));
            }
            return r.ToString();
        }

        private static int LastIndexOfTrim(byte[] raw, char ch, int pos)
        {
            while (pos >= 0 && raw[pos] == ' ')
            {
                pos--;
            }
            while (pos >= 0 && raw[pos] != ch)
            {
                pos--;
            }
            return pos;
        }

        /// <summary>Parse a Git style timezone string.</summary>
		/// <remarks>
		/// Parse a Git style timezone string.
		/// <p>
		/// The sequence "-0315" will be parsed as the numeric value -195, as the
		/// lower two positions count minutes, not 100ths of an hour.
		/// </remarks>
		/// <param name="b">buffer to scan.</param>
		/// <param name="ptr">position within buffer to start parsing digits at.</param>
		/// <returns>the timezone at this location, expressed in minutes.</returns>
		public static int ParseTimeZoneOffset(byte[] b, int ptr)
        {
            int ptrout = 0;
            int v = ParseBase10(b, ptr, ref ptrout);
            int tzMins = v % 100;
            int tzHours = v / 100;
            return tzHours * 60 + tzMins;
        }

        /// <summary>Locate the position of the tag message body.</summary>
        /// <remarks>Locate the position of the tag message body.</remarks>
        /// <param name="b">buffer to scan.</param>
        /// <param name="ptr">
        /// position in buffer to start the scan at. Most callers should
        /// pass 0 to ensure the scan starts from the beginning of the tag
        /// buffer.
        /// </param>
        /// <returns>position of the user's message buffer.</returns>
        public static int TagMessage(byte[] b, int ptr)
        {
            int sz = b.Length;
            if (ptr == 0)
            {
                ptr += 48;
            }
            // skip the "object ..." line.
            while (ptr < sz && b[ptr] != '\n')
            {
                ptr = NextLF(b, ptr);
            }
            if (ptr < sz && b[ptr] == '\n')
            {
                return ptr + 1;
            }
            return -1;
        }

        /// <summary>Locate the position of the commit message body.</summary>
		/// <remarks>Locate the position of the commit message body.</remarks>
		/// <param name="b">buffer to scan.</param>
		/// <param name="ptr">
		/// position in buffer to start the scan at. Most callers should
		/// pass 0 to ensure the scan starts from the beginning of the
		/// commit buffer.
		/// </param>
		/// <returns>position of the user's message buffer.</returns>
		public static int CommitMessage(byte[] b, int ptr)
        {
            int sz = b.Length;
            if (ptr == 0)
            {
                ptr += 46;
            }
            // skip the "tree ..." line.
            while (ptr < sz && b[ptr] == 'p')
            {
                ptr += 48;
            }
            // skip this parent.
            // Skip any remaining header lines, ignoring what their actual
            // header line type is. This is identical to the logic for a tag.
            //
            return TagMessage(b, ptr);
        }

        /// <summary>Locate the end of a paragraph.</summary>
		/// <remarks>
		/// Locate the end of a paragraph.
		/// <p>
		/// A paragraph is ended by two consecutive LF bytes.
		/// </remarks>
		/// <param name="b">buffer to scan.</param>
		/// <param name="start">
		/// position in buffer to start the scan at. Most callers will
		/// want to pass the first position of the commit message (as
		/// found by
		/// <see cref="CommitMessage(byte[], int)">CommitMessage(byte[], int)</see>
		/// .
		/// </param>
		/// <returns>
		/// position of the LF at the end of the paragraph;
		/// <code>b.length</code> if no paragraph end could be located.
		/// </returns>
		public static int EndOfParagraph(byte[] b, int start)
        {
            int ptr = start;
            int sz = b.Length;
            while (ptr < sz && b[ptr] != '\n')
            {
                ptr = NextLF(b, ptr);
            }
            while (0 < ptr && start < ptr && b[ptr - 1] == '\n')
            {
                ptr--;
            }
            return ptr;
        }

        /// <summary>Locate the first position before a given character.</summary>
		/// <remarks>Locate the first position before a given character.</remarks>
		/// <param name="b">buffer to scan.</param>
		/// <param name="ptr">position within buffer to start looking for chrA at.</param>
		/// <param name="chrA">character to find.</param>
		/// <returns>new position just before chrA, -1 for not found</returns>
		public static int Prev(byte[] b, int ptr, char chrA)
        {
            if (ptr == b.Length)
            {
                --ptr;
            }
            while (ptr >= 0)
            {
                if (b[ptr--] == chrA)
                {
                    return ptr;
                }
            }
            return ptr;
        }

        /// <summary>Locate the first position before the previous LF.</summary>
		/// <remarks>
		/// Locate the first position before the previous LF.
		/// <p>
		/// This method stops on the first '\n' it finds.
		/// </remarks>
		/// <param name="b">buffer to scan.</param>
		/// <param name="ptr">position within buffer to start looking for LF at.</param>
		/// <returns>new position just before the first LF found, -1 for not found</returns>
		public static int PrevLF(byte[] b, int ptr)
        {
            return Prev(b, ptr, '\n');
        }

        /// <summary>Locate the previous position before either the given character or LF.</summary>
        /// <remarks>
        /// Locate the previous position before either the given character or LF.
        /// <p>
        /// This method stops on the first match it finds from either chrA or '\n'.
        /// </remarks>
        /// <param name="b">buffer to scan.</param>
        /// <param name="ptr">position within buffer to start looking for chrA or LF at.</param>
        /// <param name="chrA">character to find.</param>
        /// <returns>
        /// new position just before the first chrA or LF to be found, -1 for
        /// not found
        /// </returns>
        public static int PrevLF(byte[] b, int ptr, char chrA)
        {
            if (ptr == b.Length)
            {
                --ptr;
            }
            while (ptr >= 0)
            {
                byte c = b[ptr--];
                if (c == chrA || c == '\n')
                {
                    return ptr;
                }
            }
            return ptr;
        }

        /// <summary>Locate the end of a footer line key string.</summary>
		/// <remarks>
		/// Locate the end of a footer line key string.
		/// <p>
		/// If the region at
		/// <code>raw[ptr]</code>
		/// matches
		/// <code>^[A-Za-z0-9-]+:</code>
		/// (e.g.
		/// "Signed-off-by: A. U. Thor\n") then this method returns the position of
		/// the first ':'.
		/// <p>
		/// If the region at
		/// <code>raw[ptr]</code>
		/// does not match
		/// <code>^[A-Za-z0-9-]+:</code>
		/// then this method returns -1.
		/// </remarks>
		/// <param name="raw">buffer to scan.</param>
		/// <param name="ptr">first position within raw to consider as a footer line key.</param>
		/// <returns>
		/// position of the ':' which terminates the footer line key if this
		/// is otherwise a valid footer line key; otherwise -1.
		/// </returns>
		public static int EndOfFooterLineKey(byte[] raw, int ptr)
        {
            for(int i = 0; i < ptr; i++)
            {
                byte c = raw[ptr];
                if (!char.IsLetterOrDigit((char)c) && c != '-') //TODO: REVIEW, might not be fast than lookup table
                {
                    if (c == ':')
                    {
                        return ptr;
                    }
                    return -1;
                }
                ptr++;
            }
            return -1;
        }

        /// <summary>Locate the "tagger " header line data.</summary>
        /// <remarks>Locate the "tagger " header line data.</remarks>
        /// <param name="b">buffer to scan.</param>
        /// <param name="ptr">
        /// position in buffer to start the scan at. Most callers should
        /// pass 0 to ensure the scan starts from the beginning of the tag
        /// buffer and does not accidentally look at message body.
        /// </param>
        /// <returns>
        /// position just after the space in "tagger ", so the first
        /// character of the tagger's name. If no tagger header can be
        /// located -1 is returned.
        /// </returns>
        public static int Tagger(byte[] b, int ptr)
        {
            int sz = b.Length;
            if (ptr == 0)
            {
                ptr += 48;
            }
            // skip the "object ..." line.
            while (ptr < sz)
            {
                if (b[ptr] == '\n')
                {
                    return -1;
                }
                int m = Match(b, ptr, ObjectChecker.tagger);
                if (m >= 0)
                {
                    return m;
                }
                ptr = NextLF(b, ptr);
            }
            return -1;
        }
    }
}
