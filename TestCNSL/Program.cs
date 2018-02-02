using EDSProj;
using EDSProj.EDSWebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCNSL
{
	class Program
	{
		static  void Main(string[] args) {
			Settings.init("Data/Settings.xml");
			Logger.InitFileLogger(Settings.Single.LogPath, "pbrExport");

			run();
			Console.ReadLine();
		}

		public static async void run() {
			uint cnt;
			uint total;
			if (!EDSClass.Connected)
				EDSClass.Connect();
			List<PointType> types = new List<PointType>();
			types.Add(PointType.POINTTYPEANALOG);
			types.Add(PointType.POINTTYPEDOUBLE);

			PointFilter pf = new PointFilter() {
				iessRe = "30VT_CE1011AM-180.UNIT0@SCADA"
			};

			getPointsRequest req = new getPointsRequest();
			req.authString = EDSClass.AuthStr;
			req.filter = pf;
			req.maxCount = 20000;
			req.order = "";			
			bool finish = false;
			uint index = 0;
			
			while (!finish) {
				req.startIdx = index ;
				getPointsResponse resp = await EDSClass.Client.getPointsAsync(req);
				//Point[] points = EDSClass.Client.getPoints(EDSClass.AuthStr, filter, "", index, 1000, out cnt, out total);
				foreach (Point point in resp.points) {
					try {
						/*string tg = string.Join(";", point.tg);
						_allAnalogPoints.Add(point.id.iess, new EDSPointInfo(point.id.iess, point.desc, tg, point.ar.ToString()));*/
						Console.WriteLine(point.desc);
					} catch { }
				}
				index += (uint)resp.points.Count();
				finish = index >= resp.matchCount;
			}




			//Console.WriteLine(resp.matchCount);			
			
			/*foreach (Point pt in resp.points) {
				Console.WriteLine(pt.desc);
				foreach (byte tg in pt.sg) {
					Console.WriteLine(tg);
				}

				//Console.WriteLine(pt.sg);
			}*/
			
		}
	}
}
