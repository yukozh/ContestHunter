using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContestHunter.Models.Domain
{
    public class TestCase
    {
        public byte[] Input;
        public byte[] Data;
        public int TimeLimit;
        public int MemoryLimit;
    }
}