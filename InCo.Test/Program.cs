using InCo.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InCo.Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            upload();
        }

        public static void upload()
        {

        }
    }

    public class SaveResult
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public object data { get; set; }
        public string redirect { get; set; }
    }
}
