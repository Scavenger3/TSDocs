using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using TShockAPI;

namespace TSDocs
{
    public class TSConfig
    {
		public TSCommand[] commands;
        public bool motd_enabled;
        public TSMotds motd;
        public string pagination_header_format;
        public string news_file;
        public int news_lines;
        public string[] disclude_from_playerswg;

		public TSConfig()
		{
			commands = new TSCommand[] { new TSCommand("Rules", "/rules", "Rules\\rules.txt", new Dictionary<string, string> { { "vip", "Rules\\vip-rules.txt" } }),
										 new TSCommand("Help", "/help", "Help\\help.txt", new Dictionary<string, string> { { "admin", "Help\\admin-help.txt" }, { "trustedadmin", "Help\\tadmin-help.txt" } }) };
			motd_enabled = false;
			motd = new TSMotds("Motd\\motd.txt", new Dictionary<string, string> { { "guest", "Motd\\guest-motd.txt" }, { "vip", "Motd\\vip-motd.txt" } });
			pagination_header_format = "%150,255,150%%commandname - Page %current of %count | %command <page>";
			news_file = "news.txt";
			news_lines = 1;
			disclude_from_playerswg = new string[] { "superadmin" };
		}

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

		#region (Re)Load Config
		private static void Re_LoadConfig()
		{
			//config
			if (!Directory.Exists(TSDocs.SavePath))
				NewConfig();

			TSDocs.getConfig = TSConfig.Read(TSDocs.ConfigPath);
			TSDocs.getConfig.Write(TSDocs.ConfigPath);

			//motd
			if (TSDocs.getConfig.motd != null)
			{
				if (TSDocs.getConfig.motd.file != null)
					TSUtils.CheckFile(Path.Combine(TSDocs.SavePath, TSDocs.getConfig.motd.file));

				if (TSDocs.getConfig.motd_enabled && File.ReadAllText(Path.Combine(TShock.SavePath, "motd.txt")) != "")
					File.WriteAllText(Path.Combine(TShock.SavePath, "motd.txt"), "");

				if (TSDocs.getConfig.motd.groups != null)
					foreach (var motd in TSDocs.getConfig.motd.groups)
						TSUtils.CheckFile(Path.Combine(TSDocs.SavePath, motd.Value));
			}
			//news
			TSUtils.CheckFile(Path.Combine(TSDocs.SavePath, TSDocs.getConfig.news_file));

			//commands
			int i = 0;
			foreach (var command in TSDocs.getConfig.commands)
			{
				if (command == null) continue;
				if (command.command != null && !command.command.StartsWith("/"))
					TSDocs.getConfig.commands[i].command = "/{0}".SFormat(command.command);
				i++;
				foreach (var group in command.groups)
				{
					if (!string.IsNullOrEmpty(group.Value))
						TSUtils.CheckFile(Path.Combine(TSDocs.SavePath, group.Value));
				}

				if (!string.IsNullOrEmpty(command.file))
					TSUtils.CheckFile(Path.Combine(TSDocs.SavePath, command.file));
			}
		}
		#endregion

		#region Load Config
		public static bool LoadConfig()
		{
			try
			{
				Re_LoadConfig();
				return true;
			}
			catch (Exception ex)
			{
				Log.ConsoleError("[TSDocs] Error loading Config/Files, Check logs for more details!");
				Log.Error(ex.ToString());
				return false;
			}
		}
		#endregion

		#region Reload Config
		public static void CMDreload(CommandArgs args)
		{
			TSDocs.getConfig = new TSConfig();
			try
			{
				Re_LoadConfig();
				args.Player.SendMessage("Config reloaded successfully!", Color.MediumSeaGreen);
			}
			catch (Exception ex)
			{
				args.Player.SendMessage("Error: Config failed to reload, Check logs!", Color.IndianRed);
				Log.Error("[TSDocs] Config Exception:");
				Log.Error(ex.ToString());
			}
		}
		#endregion

		#region Generate New Config
		public static void NewConfig()
		{
			Directory.CreateDirectory(TSDocs.SavePath);
			Directory.CreateDirectory(Path.Combine(TSDocs.SavePath, "Rules"));
			Directory.CreateDirectory(Path.Combine(TSDocs.SavePath, "Help"));
			Directory.CreateDirectory(Path.Combine(TSDocs.SavePath, "Motd"));

			File.WriteAllText(TSDocs.ConfigPath,
			"{" + Environment.NewLine +
			"  \"commands\": [" + Environment.NewLine +
			"    {" + Environment.NewLine +
			"      \"name\": \"Rules\"," + Environment.NewLine +
			"      \"command\": \"/rules\"," + Environment.NewLine +
			"      \"file\": \"Rules\\\\rules.txt\"," + Environment.NewLine +
			"      \"groups\": {" + Environment.NewLine +
			"        \"vip\": \"Rules\\\\vip-rules.txt\"" + Environment.NewLine +
			"      }" + Environment.NewLine +
			"    }," + Environment.NewLine +
			"    {" + Environment.NewLine +
			"      \"name\": \"Help\"," + Environment.NewLine +
			"      \"command\": \"/help\"," + Environment.NewLine +
			"      \"file\": \"Help\\\\help.txt\"," + Environment.NewLine +
			"      \"groups\": {" + Environment.NewLine +
			"        \"admin\": \"Help\\\\admin-help.txt\"," + Environment.NewLine +
			"        \"trustedadmin\": \"Help\\\\tadmin-help.txt\"," + Environment.NewLine +
			"      }" + Environment.NewLine +
			"    }" + Environment.NewLine +
			"  ]," + Environment.NewLine +
			"  \"motd_enabled\": false," + Environment.NewLine +
			"  \"motd\": {" + Environment.NewLine +
			"    \"file\": \"Motd\\\\motd.txt\"," + Environment.NewLine +
			"    \"groups\": {" + Environment.NewLine +
			"      \"guest\": \"Motd\\\\guest-motd.txt\"," + Environment.NewLine +
			"      \"vip\": \"Motd\\\\vip-motd.txt\"" + Environment.NewLine +
			"    }" + Environment.NewLine +
			"  }," + Environment.NewLine +
			"  \"pagination_header_format\": \"%150,255,150%%commandname - Page %current of %count | %command <page>\"," + Environment.NewLine +
			"  \"news_file\": \"news.txt\"," + Environment.NewLine +
			"  \"news_lines\": 1," + Environment.NewLine +
			"   \"disclude_from_playerswg\": [" + Environment.NewLine +
			"    \"superadmin\"" + Environment.NewLine +
			"  ]" + Environment.NewLine +
			"}");

			File.WriteAllText(Path.Combine(TSDocs.SavePath, "news.txt"),
			"Congrats on 2 registered players! :D");

			File.WriteAllText(Path.Combine(TSDocs.SavePath, "Rules", "rules.txt"),
			"%255,250,205%1. No Hacks / Grief!" + Environment.NewLine +
			"%255,250,205%2. Do not spawn Wall of Flesh!" + Environment.NewLine +
			"%255,250,205%3. No inapropriate buildings!" + Environment.NewLine +
			"%255,250,205%4. No explosives!" + Environment.NewLine +
			"%255,250,205%5. No Timers!" + Environment.NewLine +
			"%255,250,205%6. No Invisibility potions!" + Environment.NewLine +
			"%255,250,205%7. Always keep pvp on!" + Environment.NewLine +
			"%255,250,205%8. Always keep in character!" + Environment.NewLine +
			"%255,250,205%9. Be nice to other players!" + Environment.NewLine +
			"%255,250,205%10. Respect the admins!");

			File.WriteAllText(Path.Combine(TSDocs.SavePath, "Rules", "vip-rules.txt"),
			"%185,250,225%Do Not abuse your VIP powers!" + Environment.NewLine +
			"%include%Rules" + Path.DirectorySeparatorChar + "rules.txt%");

			File.WriteAllText(Path.Combine(TSDocs.SavePath, "Help", "help.txt"),
			"%255,250,205%Guest: /register then /login" + Environment.NewLine +
			"%255,250,205%Member: /money, /warp" + Environment.NewLine +
			"%255,250,205%Donator 1: /myhome" + Environment.NewLine +
			"%255,250,205%Donator 2: /kit tools, /tp" + Environment.NewLine +
			"%255,250,205%Mod: /tphere, /kick");

			File.WriteAllText(Path.Combine(TSDocs.SavePath, "Help", "admin-help.txt"),
			"%include%Help" + Path.DirectorySeparatorChar + "help.txt%" + Environment.NewLine +
			"%255,250,205%Admin: /mute, /ban");

			File.WriteAllText(Path.Combine(TSDocs.SavePath, "Help", "tadmin-help.txt"),
			"%include%Help" + Path.DirectorySeparatorChar + "admin-help.txt%" + Environment.NewLine +
			"%255,250,205%Trusted Admin: /region, /convertbiome");

			File.WriteAllText(Path.Combine(TSDocs.SavePath, "Motd", "motd.txt"),
			"%173,255,047%Welcome to the server %name!" + Environment.NewLine +
			"%173,255,047%Current World: %world" + Environment.NewLine +
			"%173,255,047%Currently Online: %online" + Environment.NewLine +
			"%173,255,047%Recent News: %news");

			File.WriteAllText(Path.Combine(TSDocs.SavePath, "Motd", "guest-motd.txt"),
			"%include%Motd" + Path.DirectorySeparatorChar + "motd.txt%" + Environment.NewLine +
			"%173,255,047%Type: \"/register <password>\" to register!");

			File.WriteAllText(Path.Combine(TSDocs.SavePath, "Motd", "vip-motd.txt"),
			"%include%Motd" + Path.DirectorySeparatorChar + "motd.txt%" + Environment.NewLine +
			"%255,250,205%Thanks for donating!");
		}
		#endregion
    }
}
