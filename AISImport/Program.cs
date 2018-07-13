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
		protected static DateTime GetDate(string arg,bool min=false) {
			int val = 0;
			bool isInt = int.TryParse(arg, out val);			
			if (isInt) {
                return !min ? DateTime.Now.Date.AddHours(DateTime.Now.Hour).AddHours(-val) : DateTime.Now.Date.AddHours(DateTime.Now.Hour).AddMinutes(DateTime.Now.Minute).AddMinutes(-val);
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

            
            		

            bool min = false;
            if (args.Length > 0)
            {
                min = args[0].ToLower().Contains("min");
            }

            DateTime date = DateTime.Now.Date.AddHours(DateTime.Now.Hour-2 - (min?1:12));
            DateTime dateEnd = !min?DateTime.Now.Date.AddHours(DateTime.Now.Hour):DateTime.Now.AddHours(-2);

            if (args.Length == 3) {	
				date = GetDate(args[1],min);
				dateEnd = GetDate(args[2],min);
			}

			if (min) {
                AIS_1MIN ais = new AIS_1MIN();
                ais.readPoints();
                while (date < dateEnd)
                {
                    DateTime de = date.AddMinutes(10);
                    de = de > dateEnd ? dateEnd : de;
                    ais.readDataFromDB(date, de);
                    date = de.AddHours(0);
                }
            }
            else {
                AIS_NEW ais = new AIS_NEW();
                ais.readPoints();
                while (date < dateEnd)
                {
                    DateTime de = date.AddHours(24);
                    de = de > dateEnd ? dateEnd : de;
                    ais.readDataFromDB(date, de);
                    date = de.AddHours(0);
                }
            }

		}

	}
}
