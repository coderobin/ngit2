/*
Dong Xie 2016

This is new test not in NGit or JGit
*/
using System;
using Xunit;

namespace NGit.Util.IO
{
    public class DisabledOutputStreamTest
    {
        [Fact]
        public void Test()
        {
            var s = DisabledOutputStream.INSTANCE;
            Assert.Throws<InvalidOperationException>(() => s.WriteByte(10));
        }
    }
}
