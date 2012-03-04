using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TSDocs
{
    public class TSConfig
    {
        public List<TSCommand> commands = new List<TSCommand>();// { new CCmd("Rules", "/rules", "rules.txt", "vip", "vip-rules.txt") };
        public bool motd_enabled = true;
        public TSMotds motd;// = new CMotd("motd.txt", "guest", "guest-motd.txt");
        public string pagination_header_format = "%150,255,150%%commandname - Page %current of %count | %command <page>";
        public string news_file = "news.txt";
        public int news_lines = 1;
        public string[] disclude_from_playerswg = { "superadmin" };


        public static TSConfig Read(string path)
        {
            if (!File.Exists(path))
                return new TSConfig();
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Read(fs);
            }
        }

        public static TSConfig Read(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                var cf = JsonConvert.DeserializeObject<TSConfig>(sr.ReadToEnd());
                if (ConfigRead != null)
                    ConfigRead(cf);
                return cf;
            }
        }

        public void Write(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                Write(fs);
            }
        }

        public void Write(Stream stream)
        {
            var str = JsonConvert.SerializeObject(this, Formatting.Indented);
            using (var sw = new StreamWriter(stream))
            {
                sw.Write(str);
            }
        }

        public static Action<TSConfig> ConfigRead;
    }
}
