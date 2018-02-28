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
		protected static DateTime GetDate(string arg) {
			int val = 0;
			bool isInt = int.TryParse(arg, out val);			
			if (isInt) {
				return DateTime.Now.Date.AddHours(DateTime.Now.Hour).AddHours(-val);
			} else {
				bool isDate;
				DateTime date = DateTime.Now.Date.AddHours(DateTime.Now.Hour);
				isDate = DateTime.TryParse(arg, out date);
				if (isDate) {
					return date;
				} else {
					return date = DateTime.Now.Date.AddHours(DateTime.Now.Hour);
				}
			}
		}

		static void Main(string[] args) {
			Settings.init("Data/Settings.xml");
			Logger.InitFileLogger(Settings.Single.LogPath, "aisImport");
			//AIS_OLD ais = new AIS_OLD();
			AIS_NEW ais = new AIS_NEW();
			ais.readPoints();

			/*refreshData(DateTime.Parse("01.01.2008"), DateTime.Parse("01.01.2018"));
			return;*/
			DateTime date = DateTime.Now.Date.AddHours(DateTime.Now.Hour-12);
			
			DateTime dateEnd = DateTime.Now.Date.AddHours(DateTime.Now.Hour);
			
			if (args.Length == 2) {	
				date = GetDate(args[0]);
				dateEnd = GetDate(args[1]);
			}

			//date = DateTime.Parse("01.01.2008");
			//dateEnd = DateTime.Parse("01.01.2018 00:00");
			while (date < dateEnd) {
				DateTime de = date.AddHours(24);
				de = de > dateEnd ? dateEnd : de;
				ais.readDataFromDB(date, de);
				date = de.AddHours(0);
			}

			/*date = DateTime.Parse("30.03.2008");
			dateEnd = DateTime.Parse("31.03.2008");
			while (date < dateEnd) {
				DateTime de = date.AddHours(1);
				de = de > dateEnd ? dateEnd : de;
				ais.readDataFromDB(date,de);
				date = de.AddHours(0);
			}

			date = DateTime.Parse("29.03.2009");
			dateEnd = DateTime.Parse("30.03.2009");
			while (date < dateEnd) {
				DateTime de = date.AddHours(1);
				de = de > dateEnd ? dateEnd : de;
				ais.readDataFromDB(date, de);
				date = de.AddHours(0);
			}

			date = DateTime.Parse("28.03.2010");
			dateEnd = DateTime.Parse("29.03.2010");
			while (date < dateEnd) {
				DateTime de = date.AddHours(1);
				de = de > dateEnd ? dateEnd : de;
				ais.readDataFromDB(date, de);
				date = de.AddHours(0);
			}

			date = DateTime.Parse("27.03.2011");
			dateEnd = DateTime.Parse("28.03.2011");
			while (date < dateEnd) {
				DateTime de = date.AddHours(1);
				de = de > dateEnd ? dateEnd : de;
				ais.readDataFromDB(date, de);
				date = de.AddHours(0);
			}*/
		}

		public static void refreshData(DateTime dateStart,DateTime dateEnd) {
			AIS_OLD ais = new AIS_OLD();
			ais.readPoints();
			DateTime date = dateStart.AddHours(0);
			while (date < dateEnd) {
				ais.refreshDataInEDS(date, date.AddHours(24*10));
				date = date.AddHours(24*10);
			}
		}
	}
}
