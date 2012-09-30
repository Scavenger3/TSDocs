using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Terraria;
using TShockAPI;

namespace TSDocs
{
	public static class TSUtils
	{
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

		#region Replace Variables
		public static Dictionary<string, Color> ReplaceVariables(string[] file, TSPlayer player)
		{
			Dictionary<string, Color> messages = new Dictionary<string, Color>();

			foreach (var line in file)
			{
				string newLine = line;

				#region Execute command
				if (line.StartsWith("%command%") && line.EndsWith("%"))
				{
					string[] data = line.Split('%');
					string docmd = data[2];
					if (data.Length > 4)
					{
						for (int i = 3; i < (data.Length - 1); i++)
							docmd += "%" + data[i];
					}

					if (!docmd.StartsWith("/"))
						docmd = "/" + docmd;
					Commands.HandleCommand(player, docmd);
					continue;
				}
				#endregion

				#region Check for Include
				if (newLine.StartsWith("%include%") && newLine.EndsWith("%"))
				{
					string path = Path.Combine(TSDocs.SavePath, newLine.Split('%')[2]);
					CheckFile(path);
					var sfiledata = File.ReadAllLines(path);
					foreach (var P in ReplaceVariables(sfiledata, player))
					{
						messages.Add(P.Key, P.Value);
					}
					continue;
				}
				#endregion

				#region Change Variables
				if (newLine.Contains("%playersingroup%"))
				{
					string replace = string.Empty;
					string with = GetPlayersInGroup(newLine, out replace);

					newLine = newLine.Replace(replace, with);
				}
				if (newLine.Contains("%playersingroupcount%"))
				{
					string replace = string.Empty;
					string with = GetPlayerCountInGroup(newLine, out replace).ToString();

					newLine = newLine.Replace(replace, with);
				}
				newLine = newLine.Replace("%name", player.Name);
				newLine = newLine.Replace("%world", Main.worldName);
				newLine = newLine.Replace("%ip", player.IP);
				newLine = newLine.Replace("%timeG", GetWorldTime());
				newLine = newLine.Replace("%timeR", DateTime.UtcNow.ToString());
				newLine = newLine.Replace("%time", GetWorldTime());
				newLine = newLine.Replace("%playercount", GetPlayerCount().ToString());
				newLine = newLine.Replace("%playerswg", GetPlayersWithGroups());
				newLine = newLine.Replace("%online", TShock.Utils.GetPlayers());
				newLine = newLine.Replace("%group", player.Group.Name);
				newLine = newLine.Replace("%prefix", player.Group.Prefix);
				newLine = newLine.Replace("%suffix", player.Group.Suffix);
				#endregion

				#region Get Colour
				Color displayColour = new Color(0, 255, 0);
				if (newLine.StartsWith("%"))
				{
					string colorString = "000,255,000";
					try
					{
						colorString = newLine.Split('%')[1];
						newLine = newLine.Remove(0, (colorString.Length + 2));
					}
					catch { }
					int R = 0; int G = 255; int B = 0;
					string[] cData = new string[] { "000", "255", "000" };
					try
					{
						cData = colorString.Split(',');
						R = Convert.ToInt32(cData[0]);
						G = Convert.ToInt32(cData[1]);
						B = Convert.ToInt32(cData[2]);
					}
					catch { R = 0; G = 255; B = 0; }

					displayColour = new Color(R, G, B);
				}
				#endregion

				#region Replace the news
				if (newLine.Contains("%news"))
				{
					var news = GetLatestNews();
					for (int i = 0; i < news.Count; i++)
					{
						if (i == 0)
							messages.Add(newLine.Replace("%news", news[i]), displayColour);
						else
							messages.Add(news[i], displayColour);
					}
					continue;
				}
				#endregion

				messages.Add(newLine, displayColour);
			}
			return messages;
		}
		#endregion

		#region Get Latest News
		public static List<string> GetLatestNews()
		{
			var path = Path.Combine(TSDocs.SavePath, TSDocs.getConfig.news_file);
			CheckFile(path);
			var file = File.ReadAllLines(path);
			List<string> r = new List<string>();
			if (file.Length < 1)
				return new List<string> { "none" };

			for (int i = 0; i < Math.Min(TSDocs.getConfig.news_lines, file.Length); i++)
				r.Add(file[i]);
			return r;
		}
		#endregion

		#region Get World Time
		public static string GetWorldTime()
		{
			string Suffix = "AM";
			double MainTime = Main.time;
			if (!Main.dayTime)
			{
				MainTime += 54000.0;
			}

			MainTime = MainTime / 86400.0 * 24.0;
			MainTime = MainTime - 7.5 - 12.0;

			if (MainTime < 0.0)
			{
				MainTime += 24.0;
			}
			if (MainTime >= 12.0)
			{
				Suffix = "PM";
			}
			int TimeInt = (int)MainTime;
			double TimeMinutes = MainTime - (double)TimeInt;
			TimeMinutes = (double)((int)(TimeMinutes * 60.0));
			string Minutes = string.Concat(TimeMinutes);
			if (TimeMinutes < 10.0)
			{
				Minutes = "0" + Minutes;
			}
			if (TimeMinutes > 59.0)
				Minutes = "59";
			if (TimeInt > 12)
			{
				TimeInt -= 12;
			}
			if (TimeInt == 0)
			{
				TimeInt = 12;
			}

			string Time = string.Concat(new object[]
			{
			    TimeInt,
			    ":",
			    Minutes,
			    " ",
			    Suffix
			});
			return Time;
		}
		#endregion

		#region Get a list of players in the specifed group(s)
		public static string GetPlayersInGroup(string line, out string replace)
		{
			string group = string.Empty;
			var data = line.Split('%');
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i] == "playersingroup")
				{
					group = data[i + 1];
					break;
				}
			}

			List<string> groups = new List<string>();
			if (!group.Contains(','))
				groups.Add(group);
			else
			{
				foreach (var g in group.Split(','))
					groups.Add(g);
			}

			var sb = new StringBuilder();
			foreach (TSPlayer splayer in TShock.Players)
			{
				if (splayer != null && splayer.Active && groups.Contains(splayer.Group.Name))
				{
					if (sb.Length != 0)
						sb.Append(", ");

					sb.Append(splayer.Name);
				}
			}

			replace = "%playersingroup%" + group + "%";
			if (sb.Length == 0)
				sb.Append("none");
			return sb.ToString();
		}
		#endregion

		#region Get a list of players with their group name
		public static string GetPlayersWithGroups()
		{
			var sb = new StringBuilder();
			foreach (TSPlayer tsPly in TShock.Players)
			{
				if (tsPly != null && tsPly.Active && !TSDocs.getConfig.disclude_from_playerswg.Contains(tsPly.Group.Name))
				{
					if (sb.Length != 0)
					{
						sb.Append(", ");
					}
					sb.Append("{0} ({1})".SFormat(tsPly.Name, tsPly.Group.Name));
				}
			}

			if (sb.Length == 1)
				sb.Append("none");
			return sb.ToString();
		}
		#endregion

		#region number of players online
		public static int GetPlayerCount()
		{
			int count = 0;
			foreach (TSPlayer splayer in TShock.Players)
			{
				if (splayer != null && splayer.Active)
					count++;
			}
			return count;
		}
		#endregion

		#region number of players in specified group(s) online
		public static int GetPlayerCountInGroup(string line, out string replace)
		{
			int count = 0;

			string group = string.Empty;
			var data = line.Split('%');
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i] == "playersingroupcount")
				{
					group = data[i + 1];
					break;
				}
			}

			List<string> groups = new List<string>();
			if (!group.Contains(','))
				groups.Add(group);
			else
			{
				foreach (var g in group.Split(','))
					groups.Add(g);
			}

			foreach (TSPlayer splayer in TShock.Players)
			{
				if (splayer != null && splayer.Active && groups.Contains(splayer.Group.Name))
					count++;
			}

			replace = "%playersingroupcount%" + group + "%";
			return count;
		}
		#endregion

		#region Get Pagination Header
		public static KeyValuePair<string, Color> GetPaginationHeader(string commandName, string command, int currentPage, int pageCount)
		{
			string currentHeader = TSDocs.getConfig.pagination_header_format;
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
			catch { }
			int R = 0; int G = 255; int B = 0;
			string[] cData = new string[3] { "000", "255", "000" };
			try
			{
				cData = colorString.Split(',');
			}
			catch { }
			int.TryParse(cData[0], out R); int.TryParse(cData[1], out G); int.TryParse(cData[2], out B);
			KeyValuePair<string, Color> Pair = new KeyValuePair<string, Color>(displayLine, new Color(R, G, B));

			return Pair;
		}
		#endregion

		#region Paginate
		public static void Paginate(KeyValuePair<string, Color> header, Dictionary<string, Color> messages, int page, TSPlayer player)
		{
			if (messages.Count <= 7)
			{
				foreach (var Pair in messages)
				{
					player.SendMessage(Pair.Key, Pair.Value);
				}
			}
			else
			{
				//pagenation:
				const int pagelimit = 6;

				int pagecount = messages.Count / pagelimit;
				if (page > pagecount)
				{
					player.SendMessage(string.Format("Page number exceeds pages ({0}/{1})", page + 1, pagecount + 1), Color.Red);
					return;
				}
				player.SendMessage(header.Key, header.Value);
				var flines = new List<string>(messages.Keys);
				var fcolours = new List<Color>(messages.Values);

				var messagelist = new List<string>();
				for (int i = (page * pagelimit); (i < ((page * pagelimit) + pagelimit)) && i < messages.Count; i++)
					messagelist.Add(flines[i]);
				var colourlist = new List<Color>();
				for (int i = (page * pagelimit); (i < ((page * pagelimit) + pagelimit)) && i < messages.Count; i++)
					colourlist.Add(fcolours[i]);

				for (int i = 0; i < messagelist.Count; i++)
				{
					player.SendMessage(messagelist[i], colourlist[i]);
				}
			}
		}
		#endregion
	}
}
