using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Hooks;
using TShockAPI;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Net;

namespace TSDocs
{
    [APIVersion(1, 11)]
    public class TSDocs : TerrariaPlugin
    {
        Dictionary<string, string[]> CachedFiles = new Dictionary<string, string[]>();
        public static TSConfig getConfig { get; set; }
        internal static string ConfigPath { get { return Path.Combine(TShockAPI.TShock.SavePath, "TSDocs/Config.json"); } }
        public static String savepath = "";

        public override string Name
        {
            get { return "TSDocs"; }
        }

        public override string Author
        {
            get { return "by Scavenger"; }
        }

        public override string Description //online files coming soon, I Hope ;D
        {
            get { return "Powerful Documentation and MOTD Plugin. Show information from a file or online using a command that you define."; }
        }

        public override Version Version
        {
            get { return new Version("1.0.2"); }
        }

        public override void Initialize()
        {
            GameHooks.Initialize += OnInitialize;
            ServerHooks.Chat += OnChat;
            NetHooks.GreetPlayer += OnGreetPlayer;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GameHooks.Initialize -= OnInitialize;
                ServerHooks.Chat -= OnChat;
                NetHooks.GreetPlayer -= OnGreetPlayer;
            }
            base.Dispose(disposing);
        }

        public TSDocs(Main game)
            : base(game)
        {
            Order = -15;

            getConfig = new TSConfig();
        }

        #region Initialize
        public void OnInitialize()
        {
            Commands.ChatCommands.Add(new Command("tsdocsreload", docommand, "tsdocs"));

            savepath = Path.Combine(TShockAPI.TShock.SavePath, "TSDocs/");

            if (!Directory.Exists(savepath))
                Directory.CreateDirectory(savepath);

            if (SetupConfig())
            {
                //Console.ForegroundColor = ConsoleColor.DarkGreen;
                //Console.WriteLine("[TSDocs] TSDocs sucessfuly loaded!");
                //Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        #endregion

        #region Load Config
        public static bool SetupConfig()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    NewConfig();
                }
                getConfig = TSConfig.Read(ConfigPath);
                getConfig.Write(ConfigPath);

                CheckFile(getConfig.motd.file);
                if (!File.Exists(savepath + getConfig.motd.file))
                {
                    File.Copy(TShock.SavePath + "/motd.txt", savepath + getConfig.motd.file);
                    File.WriteAllText(TShock.SavePath + "/motd.txt", "");
                }

                CheckFile(savepath + getConfig.news_file);

                if (getConfig.motd_enabled && File.ReadAllText(TShock.SavePath + "/motd.txt") != "")
                    File.WriteAllText(TShock.SavePath + "/motd.txt", "");

                foreach (var motd in getConfig.motd.groups)
                    CheckFile(savepath + motd.Value);

                int i = 0;
                foreach (var command in getConfig.commands)
                {
                    if (!command.command.StartsWith("/"))
                        getConfig.commands[i].command = "/" + command.command;
                    i++;
                    foreach (var group in command.groups)
                    {
                        if (group.Value == "")
                            continue;
                        CheckFile(savepath + group.Value);
                    }
                    if (command.file == "")
                        continue;
                    CheckFile(savepath + command.file);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[TSDocs] Error in config file, Check logs for more details!");
                Console.ForegroundColor = ConsoleColor.Gray;
                Log.Error("[TSDocs] Config Exception:");
                Log.Error(ex.ToString());
                return false;
            }
        }
        #endregion

        #region Reload Config
        public static void docommand(CommandArgs args)
        {
            getConfig = new TSConfig();
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    NewConfig();
                }
                getConfig = TSConfig.Read(ConfigPath);
                getConfig.Write(ConfigPath);

                //reload files:
                CheckFile(getConfig.motd.file);
                if (!File.Exists(savepath + getConfig.motd.file))
                {
                    File.Copy(TShock.SavePath + "/motd.txt", savepath + getConfig.motd.file);
                    File.WriteAllText(TShock.SavePath + "/motd.txt", "");
                }

                CheckFile(savepath + getConfig.news_file);

                if (getConfig.motd_enabled && File.ReadAllText(TShock.SavePath + "/motd.txt") != "")
                    File.WriteAllText(TShock.SavePath + "/motd.txt", "");

                foreach (var motd in getConfig.motd.groups)
                    CheckFile(savepath + motd.Value);

                int i = 0;
                foreach (var command in getConfig.commands)
                {
                    if (!command.command.StartsWith("/"))
                        getConfig.commands[i].command = "/" + command.command;
                    i++;
                    foreach (var group in command.groups)
                    {
                        if (group.Value == "")
                            continue;
                        CheckFile(savepath + group.Value);
                    }
                    if (command.file == "")
                        continue;
                    CheckFile(savepath + command.file);
                }

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
            File.WriteAllText(ConfigPath,
            "{" + Environment.NewLine +
            "  \"commands\": [" + Environment.NewLine +
            "    {" + Environment.NewLine +
            "      \"name\": \"Rules\"," + Environment.NewLine +
            "      \"command\": \"/rules\"," + Environment.NewLine +
            "      \"file\": \"rules.txt\"," + Environment.NewLine +
            "      \"groups\": {" + Environment.NewLine +
            "        \"vip\": \"vip-rules.txt\"" + Environment.NewLine +
            "      }" + Environment.NewLine +
            "    }," + Environment.NewLine +
            "    {" + Environment.NewLine +
            "      \"name\": \"Help\"," + Environment.NewLine +
            "      \"command\": \"/help\"," + Environment.NewLine +
            "      \"file\": \"help.txt\"," + Environment.NewLine +
            "      \"groups\": {" + Environment.NewLine +
            "        \"admin\": \"admin-help.txt\"," + Environment.NewLine +
            "        \"trustedadmin\": \"tadmin-help.txt\"," + Environment.NewLine +
            "      }" + Environment.NewLine +
            "    }" + Environment.NewLine +
            "  ]," + Environment.NewLine +
            "  \"motd_enabled\": true," + Environment.NewLine +
            "  \"motd\": {" + Environment.NewLine +
            "    \"file\": \"motd.txt\"," + Environment.NewLine +
            "    \"groups\": {" + Environment.NewLine +
            "      \"guest\": \"guest-motd.txt\"," + Environment.NewLine +
            "      \"admin\": \"admin-motd.txt\"" + Environment.NewLine +
            "    }" + Environment.NewLine +
            "  }," + Environment.NewLine +
            "  \"pagination_header_format\": \"%150,255,150%%commandname - Page %current of %count | %command <page>\"," + Environment.NewLine +
            "  \"news_file\": \"news.txt\"," + Environment.NewLine +
            "  \"news_lines\": 1," + Environment.NewLine +
            "   \"disclude_from_playerswg\": [" + Environment.NewLine +
            "    \"superadmin\"" + Environment.NewLine +
            "  ]" + Environment.NewLine +
            "}");
        }
        #endregion

        #region Check if file exists
        public static void CheckFile(String path)
        {
            String directory = (new DirectoryInfo(path)).Parent.FullName;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            if (!File.Exists(path))
                File.Create(path);
        }
        #endregion

        #region Handle MOTD
        public void OnGreetPlayer(int who, HandledEventArgs e)
        {
            if (getConfig.motd_enabled)
            {
                ShowMOTD(TShock.Players[who]);
            }
        }
        #endregion

        #region Show MOTD
        public static void ShowMOTD(TSPlayer player)
        {
            try
            {
                String filetoshow = savepath + getConfig.motd.file;
                foreach (var group in getConfig.motd.groups)
                {
                    if (group.Key == player.Group.Name)
                    {
                        filetoshow = savepath + group.Value;
                    }
                }

                CheckFile(filetoshow);
                var file = File.ReadAllLines(filetoshow);

                foreach (var line in file)
                {
                    string newLine = line;
                    if (newLine.StartsWith("%command%") && newLine.EndsWith("%"))
                    {
                        string docmd = newLine.Split('%')[2];
                        if (!docmd.StartsWith("/"))
                            docmd = "/" + docmd;
                        Commands.HandleCommand(player, docmd);
                        continue;
                    }

                    if (newLine.Contains("%playersingroup%"))
                    {
                        string replace = "";
                        string with = GetPlayersInGroup(newLine, out replace);

                        newLine = newLine.Replace(replace, with);
                    }

                    newLine = newLine.Replace("%name", player.Name);
                    newLine = newLine.Replace("%world", Main.worldName);
                    newLine = newLine.Replace("%ip", player.IP);
                    newLine = newLine.Replace("%timeG", GetWorldTime().ToString());
                    newLine = newLine.Replace("%timeR", DateTime.UtcNow.ToString());
                    newLine = newLine.Replace("%time", GetWorldTime().ToString());
                    newLine = newLine.Replace("%playercount", GetPlayerCount().ToString());
                    newLine = newLine.Replace("%playerswg", GetPlayersWithGroups());
                    newLine = newLine.Replace("%players", TShock.Utils.GetPlayers());
                    newLine = newLine.Replace("%group", player.Group.Name);
                    newLine = newLine.Replace("%prefix", player.Group.Prefix);
                    newLine = newLine.Replace("%suffix", player.Group.Suffix);
                    
                    string displayLine = newLine;
                    Color displayColour = new Color(0, 255, 0);
                    if (newLine.StartsWith("%"))
                    {
                        string colorString = "000,255,000";
                        try
                        {
                            colorString = newLine.Split('%')[1];
                            displayLine = newLine.Remove(0, (colorString.Length + 2));
                        }
                        catch { }
                        int R = 0; int G = 255; int B = 0;
                        string[] cData = new string[3] { "000", "255", "000" };
                        try
                        {
                            cData = colorString.Split(',');
                            R = Convert.ToInt32(cData[0]);
                            G = Convert.ToInt32(cData[1]);
                            B = Convert.ToInt32(cData[2]);
                        }
                        catch { displayLine = newLine; R = 0; G = 255; B = 0; }

                        displayColour = new Color(R, G, B);
                    }

                    #region Replace the news
                    if (displayLine.Contains("%news"))
                    {
                        bool firstline = true;
                        foreach (var lne in GetLatestNews())
                        {
                            if (firstline)
                            {
                                firstline = false;
                                player.SendMessage(displayLine.Replace("%news", lne), displayColour);
                            }
                            else
                                player.SendMessage(lne, displayColour);
                        }
                        continue;
                    }
                    #endregion

                    player.SendMessage(displayLine, displayColour);
                }
            }
            catch (Exception ex)
            {
                Log.ConsoleError("Something when wrong when showing {0} a motd. Check the logs.".SFormat(player.Name));
                Log.Error(ex.ToString());
            }
        }
        #endregion

        #region Handle Command
        public void OnChat(messageBuffer msgb, int who, string text, HandledEventArgs e)
        {
            if (e.Handled || !text.StartsWith("/"))
                return;

            foreach (var command in getConfig.commands)
            {
                if (text == command.command || text.StartsWith(command.command + " "))
                {
                    if (command.file != "" && (TShock.Players[who].Group.HasPermission("tsdocs-command-*") || TShock.Players[who].Group.HasPermission("tsdocs-command-" + command.command)))
                    {
                        e.Handled = true;
                        Log.Info("{0} executed: {1}".SFormat(TShock.Players[who].Name, command.command));
                        ShowFile(command, text, TShock.Players[who]);
                    }
                    break;
                }
            }
        }
        #endregion

        #region Show Command
        public static void ShowFile(TSCommand command, string chat, TSPlayer player)
        {
            try
            {
                String filetoshow = savepath + command.file;
                foreach (var group in command.groups)
                {
                    if (group.Key == player.Group.Name)
                    {
                       filetoshow = savepath + group.Value;
                    }
               }

                Dictionary<string, Color> displayLines = new Dictionary<string, Color>();

                CheckFile(filetoshow);

                var file = File.ReadAllLines(filetoshow);

                displayLines = GetMessages(file, player);

                if (displayLines.Count <= 7)
                {
                    foreach (var Pair in displayLines)
                    {
                        if (Pair.Key.StartsWith("&CoMManD&|"))
                        {
                            string docmd = Pair.Key.Split('|')[1];
                            Commands.HandleCommand(player, docmd);
                            continue;
                        }
                        player.SendMessage(Pair.Key, Pair.Value);
                    }
                }
                else
                {
                    //pagenation:
                    const int pagelimit = 6;
                    const int perline = 1;
                    int page = 0;

                    if (chat.Contains(" "))
                    {
                        var data = chat.Split(' ');
                        if (int.TryParse(data[1], out page))
                            page--;
                        else
                            player.SendMessage(string.Format("Invalid page number ({0})", data[1]), Color.Red);
                    }

                    int pagecount = displayLines.Count / pagelimit;
                    if (page > pagecount)
                    {
                        player.SendMessage(string.Format("Page number exceeds pages ({0}/{1})", page + 1, pagecount + 1), Color.Red);
                        return;
                    }
                    var header = GetPaginationHeader(command.name, command.command, page + 1, pagecount + 1);
                    player.SendMessage(header.Key, header.Value);
                    var flines = new List<string>();
                    var fcolors = new List<Color>();
                    foreach (var Pair in displayLines)
                    {
                        flines.Add(Pair.Key);
                        fcolors.Add(Pair.Value);
                    }
                    var nameslist = new List<string>();
                    for (int i = (page * pagelimit); (i < ((page * pagelimit) + pagelimit)) && i < displayLines.Count; i++)
                        nameslist.Add(flines[i]);
                    var colourslist = new List<Color>();
                    for (int i = (page * pagelimit); (i < ((page * pagelimit) + pagelimit)) && i < displayLines.Count; i++)
                        colourslist.Add(fcolors[i]);
                    var names = nameslist.ToArray();
                    var colors = colourslist.ToArray();
                    for (int i = 0; i < names.Length; i += perline)
                    {
                        if (names[i].StartsWith("&CoMManD&|"))
                        {
                            string docmd = names[i].Split('|')[1];
                            Commands.HandleCommand(player, docmd);
                            continue;
                        }
                        player.SendMessage(names[i], colors[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ConsoleError("Something when wrong when showing {0} \"{1}\". Check the Logs.".SFormat(player.Name, command.command));
                Log.Error(ex.ToString());
            }
        }
        #endregion

        #region Get messages from a file
        public static Dictionary<string, Color> GetMessages(string[] file, TSPlayer player)
        {
            Dictionary<string, Color> r = new Dictionary<string, Color>();

            foreach (var line in file)
            {
                string newLine = line;

                #region Check for Command / Include
                if (newLine.StartsWith("%command%") && newLine.EndsWith("%"))
                {
                    string writecmd = newLine.Split('%')[2];
                    if (!writecmd.StartsWith("/"))
                        writecmd = "/" + writecmd;
                    r.Add("&CoMManD&|" + writecmd, new Color(255, 255, 255));
                    continue;
                }
                else if (newLine.StartsWith("%include%") && newLine.EndsWith("%"))
                {
                    String sfile = savepath + newLine.Split('%')[2];
                    CheckFile(sfile);
                    var sfiledata = File.ReadAllLines(sfile);
                    foreach(var P in GetMessages(sfiledata, player))
                    {
                        r.Add(P.Key, P.Value);
                    }
                    continue;
                }
                #endregion

                #region Change Variables
                if (newLine.Contains("%playersingroup%"))
                {
                    string replace = "";
                    string with = GetPlayersInGroup(newLine, out replace);

                    newLine = newLine.Replace(replace, with);
                }
                newLine = newLine.Replace("%name", player.Name);
                newLine = newLine.Replace("%world", Main.worldName);
                newLine = newLine.Replace("%ip", player.IP);
                newLine = newLine.Replace("%timeG", GetWorldTime().ToString());
                newLine = newLine.Replace("%timeR", DateTime.UtcNow.ToString());
                newLine = newLine.Replace("%time", GetWorldTime().ToString());
                newLine = newLine.Replace("%playercount", GetPlayerCount().ToString());
                newLine = newLine.Replace("%playerswg", GetPlayersWithGroups());
                newLine = newLine.Replace("%players", TShock.Utils.GetPlayers());
                newLine = newLine.Replace("%group", player.Group.Name);
                newLine = newLine.Replace("%prefix", player.Group.Prefix);
                newLine = newLine.Replace("%suffix", player.Group.Suffix);
                #endregion

                #region Get Colour
                string displayLine = newLine;
                Color displayColour = new Color(0, 255, 0);
                if (newLine.StartsWith("%"))
                {
                    string colorString = "000,255,000";
                    try
                    {
                        colorString = newLine.Split('%')[1];
                        displayLine = newLine.Remove(0, (colorString.Length + 2));
                    }
                    catch { }
                    int R = 0; int G = 255; int B = 0;
                    string[] cData = new string[3] { "000", "255", "000" };
                    try
                    {
                        cData = colorString.Split(',');
                        R = Convert.ToInt32(cData[0]);
                        G = Convert.ToInt32(cData[1]);
                        B = Convert.ToInt32(cData[2]);
                    }
                    catch { displayLine = newLine; R = 0; G = 255; B = 0; }

                    displayColour = new Color(R, G, B);
                }
                #endregion

                #region Replace the news
                if (displayLine.Contains("%news"))
                {
                    bool firstline = true;
                    foreach (var lne in GetLatestNews())
                    {
                        if (firstline)
                        {
                            firstline = false;
                            r.Add(displayLine.Replace("%news", lne), displayColour);
                        }
                        else
                            r.Add(lne, displayColour);
                    }
                    continue;
                }
                #endregion

                r.Add(displayLine, displayColour);
            }
            return r;
        }
        #endregion

        #region Get Stuff for Variables
        #region Get Latest News
        public static List<string> GetLatestNews()
        {
            CheckFile(savepath + getConfig.news_file);
            List<string> r = new List<string>();
            var file = File.ReadAllLines(savepath + getConfig.news_file);
            if (file.Length < 1)
                return new List<string> { " " };
            for (int i = 1; i <= Math.Min(getConfig.news_lines, file.Length); i++)
            {
                int line = i - 1;
                r.Add(file[line]);
            }
            return r;
        }
        #endregion

        #region Get World Time
        public static string GetWorldTime()
        {
            string text25 = "AM";
            double num212 = Main.time;
            if (!Main.dayTime)
            {
                num212 += 54000.0;
            }
            num212 = num212 / 86400.0 * 24.0;
            double num213 = 7.5;
            num212 = num212 - num213 - 12.0;
            if (num212 < 0.0)
            {
                num212 += 24.0;
            }
            if (num212 >= 12.0)
            {
                text25 = "PM";
            }
            int num214 = (int)num212;
            double num215 = num212 - (double)num214;
            num215 = (double)((int)(num215 * 60.0));
            string text26 = string.Concat(num215);
            if (num215 < 10.0)
            {
                text26 = "0" + text26;
            }
            if (num214 > 12)
            {
                num214 -= 12;
            }
            if (num214 == 0)
            {
                num214 = 12;
            }
            if (Main.player[Main.myPlayer].accWatch == 1)
            {
                text26 = "00";
            }
            else
            {
                if (Main.player[Main.myPlayer].accWatch == 2)
                {
                    if (num215 < 30.0)
                    {
                        text26 = "00";
                    }
                    else
                    {
                        text26 = "30";
                    }
                }
            }
            string text24 = string.Concat(new object[]
			{
			    num214,
			    ":",
			    text26,
			    " ",
			    text25
			});
            return text24;
        }
        #endregion

        #region Get Player Lists
        public static string GetPlayersInGroup(string newLine, out string replace)
        {
            
            string group = "";
            var data = newLine.Split('%');
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == "playersingroup")
                {
                    group = data[i + 1];
                    break;
                }
            }

            var sb = new StringBuilder();
            foreach (TSPlayer splayer in TShock.Players)
            {
                if (splayer != null && splayer.Active && splayer.Group.Name == group)
                {
                    if (sb.Length != 0)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(splayer.Name);
                }
            }

            replace = "%playersingroup%" + group + "%";
            return sb.ToString();
        }

        public static string GetPlayersWithGroups()
        {
            var sb2 = new StringBuilder();
            foreach (TSPlayer splayer in TShock.Players)
            {
                if (splayer != null && splayer.Active && !getConfig.disclude_from_playerswg.Contains(splayer.Group.Name))
                {
                    if (sb2.Length != 0)
                    {
                        sb2.Append(", ");
                    }
                    sb2.Append(splayer.Name + " (" + splayer.Group.Name + ")");
                }
            }
            return sb2.ToString();
        }

        public static int GetPlayerCount()
        {
            int count = 0;
            foreach (TSPlayer splayer in TShock.Players)
            {
                if (splayer != null && splayer.Active)
                {
                    count++;
                }
            }
            return count;
        }
        #endregion
        #endregion

        #region Get Pagination Header
        public static KeyValuePair<string, Color> GetPaginationHeader(string commandName, string command, int currentPage, int pageCount)
        {
            string currentHeader = getConfig.pagination_header_format;
            string newHeader = currentHeader;
            newHeader = newHeader.Replace("%commandname", commandName);
            newHeader = newHeader.Replace("%command", command);
            newHeader = newHeader.Replace("%current", currentPage.ToString());
            newHeader = newHeader.Replace("%count", pageCount.ToString());

            string displayLine = newHeader;
            string colorString = "000,255,000";
            try
            {
                colorString = newHeader.Split('%')[1];
                displayLine = newHeader.Remove(0, (colorString.Length + 2));
            }
            catch
            {
            }
            int R = 0; int G = 255; int B = 0;
            string[] cData = new string[3] { "000", "255", "000" };
            try
            {
                cData = colorString.Split(',');
            }
            catch
            {
            }
            int.TryParse(cData[0], out R); int.TryParse(cData[1], out G); int.TryParse(cData[2], out B);
            KeyValuePair<string, Color> Pair = new KeyValuePair<string, Color>(displayLine, new Color(R, G, B));

            return Pair;
        }
        #endregion

    }
}
