using EDSProj;
using EDSProj.ModesCentre;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSCnsl
{
	class Program
	{
		static void Main(string[] args) {
			Settings.init("Data/Settings.xml");
			Logger.InitFileLogger(Settings.Single.LogPath, "pbrExport");
			
			MCSettings.init("Data/MCSettings.xml");
			 
			MCServerReader reader = new MCServerReader(DateTime.Now.Date);
		}
	}
}
