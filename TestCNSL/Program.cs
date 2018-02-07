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

		public async static  void run() {
			uint cnt;
			uint total;
			if (!EDSClass.Connected)
				EDSClass.Connect();

			TabularRequest req = new TabularRequest();
			List<TabularRequestItem> items = new List<TabularRequestItem>();
			items.Add(new TabularRequestItem() {
				function = "UNDER_TIME",
				pointId = new PointId() {
					iess = "04VT_DS02DI-01.MCR@GRARM"
				},
				@params=new double[] {0.9},
				shadePriority=ShadePriority.REGULARONLY
				
			});
			req.items = items.ToArray();			

			req.period = new TimePeriod() {
				from = new Timestamp() {
					second = EDSClass.toTS(DateTime.Parse("01.01.2018"))
				},
				till = new Timestamp() {
					second = EDSClass.toTS(DateTime.Parse("05.02.2018"))
				}
			};

			req.step = new TimeDuration() {
				seconds = 30 * 60
			};

			uint id=EDSClass.Client.requestTabular(EDSClass.AuthStr, req);

			EDSClass.ProcessQuery(id);

			TabularRow[] rows;

			getTabularRequest request = new getTabularRequest() {
				authString = EDSClass.AuthStr,
				requestId=id
			};

			getTabularResponse resp= await EDSClass.Client.getTabularAsync(request);

			foreach (TabularRow row in resp.rows) {
				DateTime date = EDSClass.fromTS(row.ts.second);
				List<double> vals = new List<double>();
				foreach (TabularValue val in row.values) {
					vals.Add(EDSClass.getVal(val.value));

				}
				string str = string.Format("{0}: {1}", date, string.Join("   ", vals));
				Console.WriteLine(str);
			}



		}
	}
}
