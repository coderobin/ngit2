/* 
Dong Xie 2016
*/

using System;
using System.Linq;
using System.Text;
using Xunit;

namespace NGit
{
    public class FileModeTest
    {
        [Fact]
        public void TestVerifyFromBitsCodeChangeCorrect()
        {
            // Octal: 100666
            int mode = 33206;
            byte[] tmp = new byte[10];
            int p = tmp.Length;

            while (mode != 0)
            {
                tmp[--p] = (byte)((byte)'0' + (mode & 0x7));
                mode >>= 3;
            }

            byte[] octalBytes = new byte[tmp.Length - p];
            for (int k = 0; k < octalBytes.Length; k++)
            {
                octalBytes[k] = tmp[p + k];
            }

            int mode2 = 33206;
            byte[] octalBytes2 = Encoding.ASCII.GetBytes(Convert.ToString(mode2, 8));

            Assert.True(octalBytes.SequenceEqual(octalBytes2));
        }

        [Fact]
        public void TestFromBitsRegular()
        {
            // Octal: 100666
            FileMode fm = FileMode.FromBits(33206);

            Assert.Equal<FileMode>(fm, FileMode.REGULAR_FILE);

            // Octal: 100444
            FileMode fm2 = FileMode.FromBits(33060);

            Assert.Equal<FileMode>(fm2, FileMode.REGULAR_FILE);
        }

        public void TestFromBitsTree()
        {
            FileMode fm = FileMode.FromBits(18295);

            Assert.Equal<FileMode>(fm, FileMode.TREE);
        }
    }
}
