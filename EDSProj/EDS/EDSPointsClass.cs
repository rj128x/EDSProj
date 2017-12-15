using EDSProj.EDSWebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EDSProj
{
	public class EDSPointInfo
	{
		public string IESS { get; set; }
		public string Desc { get; set; }
		public string TG { get; set; }
		public List<int> Groups { get; set; }
		public string FullName { get; set; }

		public EDSPointInfo(string iess, string desc,string tg) {
			this.IESS = iess;
			this.Desc = desc;
			this.TG = tg;
			Groups = new List<int>();
			try {
				string[] groups = tg.Split(new char[] { ';' });				
				foreach (string group in groups) {
					try {
						Groups.Add(Int32.Parse(group));
					} catch { }
				}
			} catch { }
			this.FullName = String.Format("{0,-40}{1}", iess, desc);
		}
	}

	public class EDSPointsClass
	{
		protected static Dictionary<string,EDSPointInfo> _allAnalogPoints { get; set; }

		public static Dictionary<string, EDSPointInfo> AllAnalogPoints {
			get {
				if (_allAnalogPoints == null)
					GetAllPoints();
				return _allAnalogPoints;
			}
		}

		protected static void GetAllPoints() {
			_allAnalogPoints = new Dictionary<string, EDSPointInfo>();
			try {
				string[] lines = System.IO.File.ReadAllLines("Data/allPoints.txt");
				foreach (string line in lines) {
					if (!line.Contains("POINT"))
						continue;
					try {
						Regex regex = new Regex(@"RT=(\w+).+IESS='([^']+).+DESC='([^']+).+TG='([^']+)");
						Match match = regex.Match(line);
						if (match.Success) {
							string type = match.Groups[1].Value.ToLower();
							string iess = match.Groups[2].Value;
							string desc = match.Groups[3].Value;
							string tg = match.Groups[4].Value;
							if (type == "analog" || type == "double" ) {
								_allAnalogPoints.Add(iess,new EDSPointInfo(iess, desc,tg));
							}
						}
					} catch (Exception e) {
						Logger.Info(String.Format("Ошибка при разборе строки {0}: {1}", line, e));
					}
				}
			} catch (Exception e) {
				Logger.Info(("Ошибка при получении списка точек: " + e.ToString()));
			}
		}
	}
}
