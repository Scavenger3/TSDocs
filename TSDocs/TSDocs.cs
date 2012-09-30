using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSDocs
{
    public class TSCommand
    {
		public string name { get; set; }
		public string command { get; set; }
		public string file { get; set; }
		public Dictionary<string, string> groups { get; set; }

        public TSCommand(string name, string command, string file, Dictionary<string, string> groups)
        {
            this.name = name;
            this.command = command;
            this.file = file;
            this.groups = groups;
        }
    }

    public class TSMotds
    {
		public string file { get; set; }
		public Dictionary<string, string> groups { get; set; }

        public TSMotds(string file, Dictionary<string, string> groups)
        {
            this.file = file;
            this.groups = groups;
        }
    }
}
