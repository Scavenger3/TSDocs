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

namespace TSDocs
{
    [APIVersion(1, 11)]
    public class TSMain : TerrariaPlugin
    {
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
            get { return new Version("1.0"); }
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

        public TSMain(Main game)
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
                if (File.Exists(ConfigPath))
                {
                     getConfig = TSConfig.Read(ConfigPath);
                }
                getConfig.Write(ConfigPath);

                if (!File.Exists(savepath + getConfig.motd.file))
                {
                    File.Copy(TShock.SavePath + "/motd.txt", savepath + getConfig.motd.file);
                    File.WriteAllText(TShock.SavePath + "/motd.txt", "");
                }

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
                if (File.Exists(ConfigPath))
                {
                    getConfig = TSConfig.Read(ConfigPath);
                }
                getConfig.Write(ConfigPath);

                //reload files:
                if (!File.Exists(savepath + getConfig.motd.file))
                {
                    File.Copy(TShock.SavePath + "/motd.txt", savepath + getConfig.motd.file);
                    File.WriteAllText(TShock.SavePath + "/motd.txt", "");
                }

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
                    newLine = newLine.Replace("%name", player.Name);
                    newLine = newLine.Replace("%world", Main.worldName);
                    newLine = newLine.Replace("%ip", player.IP);
                    newLine = newLine.Replace("%timeG", Main.time.ToString());
                    newLine = newLine.Replace("%timeR", DateTime.UtcNow.ToString());
                    newLine = newLine.Replace("%time", DateTime.UtcNow.ToString());
                    newLine = newLine.Replace("%online", TShock.Utils.GetPlayers());
                    newLine = newLine.Replace("%players", TShock.Utils.GetPlayers());
                    newLine = newLine.Replace("%group", player.Group.Name);
                    newLine = newLine.Replace("%prefix", player.Group.Prefix);
                    newLine = newLine.Replace("%suffix", player.Group.Suffix);
                    //add more here if requested!

                    string displayLine = newLine;
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
                    }
                    catch { }
                    int.TryParse(cData[0], out R); int.TryParse(cData[1], out G); int.TryParse(cData[2], out B);

                    player.SendMessage(displayLine, (byte)R, (byte)G, (byte)B);
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
                    string file = command.file;
                    foreach (var group in command.groups)
                    {
                        if (group.Key == TShock.Players[who].Group.Name)
                        {
                            file = group.Value;
                        }
                    }

                    if (file != "")
                    {
                        ShowFile(command, text, TShock.Players[who]);
                        e.Handled = true;
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

                foreach (var line in file)
                {
                    string newLine = line;

                    if (newLine.StartsWith("%command%") && newLine.EndsWith("%"))
                    {
                        string Lcommand = newLine.Split('%')[2];
                        if (!Lcommand.StartsWith("/"))
                            Lcommand = "/" + Lcommand;
                        Commands.HandleCommand(player, Lcommand);
                        continue;
                    }

                    newLine = newLine.Replace("%name", player.Name);
                    newLine = newLine.Replace("%world", Main.worldName);
                    newLine = newLine.Replace("%ip", player.IP);
                    newLine = newLine.Replace("%timeG", Main.time.ToString());
                    newLine = newLine.Replace("%timeR", DateTime.UtcNow.ToString());
                    newLine = newLine.Replace("%time", DateTime.UtcNow.ToString());
                    newLine = newLine.Replace("%online", TShock.Utils.GetPlayers());
                    newLine = newLine.Replace("%players", TShock.Utils.GetPlayers());
                    newLine = newLine.Replace("%group", player.Group.Name);
                    newLine = newLine.Replace("%prefix", player.Group.Prefix);
                    newLine = newLine.Replace("%suffix", player.Group.Suffix);
                    //add more here if requested!

                    string displayLine = newLine;
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
                    }
                    catch { }
                    int.TryParse(cData[0], out R); int.TryParse(cData[1], out G); int.TryParse(cData[2], out B);

                    Color displayColour = new Color(R, G, B);
                    displayLines.Add(displayLine, displayColour);
                }

                if (displayLines.Count <= 7)
                {
                    foreach (var Pair in displayLines)
                    {
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
                        player.SendMessage(names[i], colors[i]);
                }
            }
            catch (Exception ex)
            {
                Log.ConsoleError("Something when wrong when showing {0} a file. Check the Logs.".SFormat(player.Name));
                Log.Error(ex.ToString());
            }
        }
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
