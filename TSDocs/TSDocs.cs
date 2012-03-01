using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSDocs
{
    public class TSCommand
    {
        public string name = "";
        public string command = "";
        public string file = "";
        public Dictionary<string, string> groups = new Dictionary<string, string>();

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
        public string file = "";
        public Dictionary<string, string> groups = new Dictionary<string, string>();

        public TSMotds(string file, Dictionary<string, string> groups)
        {
            this.file = file;
            this.groups = groups;
        }
    }
}
