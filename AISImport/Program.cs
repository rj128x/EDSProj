using EDSProj;
using EDSProj.AIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISImport
{
	class Program
	{
		static void Main(string[] args) {
			Settings.init("Data/Settings.xml");
			Logger.InitFileLogger(Settings.Single.LogPath, "aisImport");
			AIS_OLD ais = new AIS_OLD();
			ais.readPoints();

			DateTime date = DateTime.Parse("01.12.2017");
			DateTime dateEnd = DateTime.Parse("02.12.2017");
			if (args.Length == 2) {
				date = DateTime.Parse(args[0]);
				dateEnd = DateTime.Parse(args[1]);
			}

			while (date < dateEnd) {
				ais.readDataFromDB(date,date.AddHours(4));
				date = date.AddHours(4);
			}

			Console.ReadLine();
		}
	}
}
