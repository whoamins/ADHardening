using System;
using System.Diagnostics;
using System.Net;

namespace ActiveDiretoryHardening
{
    class Program
    {
        static void Main(string[] args)
        {

        }
        
        private static void CallCopy()
        {
            NetworkCredential credentials = new("Administrator", "P@ssw0rd");

            NetworkConnection networkConnection = new(@"\\192.168.56.102\shara\", credentials);
            networkConnection.CopyFile(@"C:\\Users\000\Desktop\keys", @"keys");
        }
    }
}