using EDSProj.EDSWebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSProj
{
	public enum EDSReportPeriod { minute, hour, day }
	public enum EDSReportFunction { avg,max,min,val}
	public class EDSClass {
		public static EDSClass Single { get; protected set; }


		protected string _authStr { get; set; }
		protected edsPortTypeClient _client { get; set; }
		
		public static Dictionary<EDSReportPeriod, string> ReportPeriods { get; protected set; }
		public static Dictionary<EDSReportFunction, string> ReportFunctions { get; protected set; }
		
		protected EDSClass() {
			
		}

		public static long toTS(DateTime date) {
			DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			TimeSpan diff = date.ToUniversalTime() - origin;
			return (long)(diff.TotalSeconds);
		}

		public static DateTime fromTS(long sec) {
			DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			return origin.AddSeconds(sec).ToLocalTime() ;
		}

		protected bool _connect() {
			Logger.Info("Подключение к EDS серверу");
			if (_client == null) {
				_client = new edsPortTypeClient();								
			}
			Logger.Info("Проверка состояния подключения");
			if (_client.State != System.ServiceModel.CommunicationState.Opened) {
				_authStr = _client.login(Settings.Single.EDSUser, Settings.Single.EDSPassword, ClientType.CLIENTTYPEDEFAULT);
			}
			Logger.Info("Состояние подключения: " + Client.State);
			return _client.State == System.ServiceModel.CommunicationState.Opened;
		}

		public static bool Connected {
			get {
				if (Single._client == null)
					return false;
				else
					return Single._client.State == System.ServiceModel.CommunicationState.Opened;
			}
		}

		public static edsPortTypeClient Client {
			get {
				return Single._client;
			}
		}

		public static string AuthStr {
			get {
				return Single._authStr;
			}
		}

		public static bool Connect() {
			return Single._connect();
		}

		static EDSClass() {
			Single = new EDSClass();
			ReportPeriods = new Dictionary<EDSReportPeriod, string>();
			ReportPeriods.Add(EDSReportPeriod.minute, "Минута");
			ReportPeriods.Add(EDSReportPeriod.hour, "Час");
			ReportPeriods.Add(EDSReportPeriod.day, "Сутки");

			ReportFunctions = new Dictionary<EDSReportFunction, string>();
			ReportFunctions.Add(EDSReportFunction.avg, "Среднее");
			ReportFunctions.Add(EDSReportFunction.min, "Минимум");
			ReportFunctions.Add(EDSReportFunction.max, "Максимум");
			ReportFunctions.Add(EDSReportFunction.val, "Значение");
		}

		public static string getReportFunctionName(EDSReportFunction func) {
			string name = "AVG";
			switch (func) {
				case EDSReportFunction.avg:
					name = "AVG";
					break;
				case EDSReportFunction.val:
					name = "VALUE";
					break;
				case EDSReportFunction.min:
					name = "MIN_VALUE";
					break;
				case EDSReportFunction.max:
					name = "MAX_VALUE";
					break;
			}
			return name;
		}

		public static long getPeriodSeconds(EDSReportPeriod period) {
			long res = 3600;
			switch (period) {
				case EDSReportPeriod.minute:
					res=60;
					break;
				case EDSReportPeriod.hour:
					res= 3600;
					break;
				case EDSReportPeriod.day:
					res= 3600 * 24;
					break;
			}
			return res;
		}

		public  static bool ProcessQuery(uint id) {
			bool finished = false;
			RequestStatus status;
			float progress;
			string msg;
			int i = 0;
			bool ok = false;
			do {
				i++;
				Single._client.getRequestStatus(Single._authStr, id, out status, out progress, out msg);
				Logger.Info(String.Format("{3} {0}: {1} ({2})", status, progress * 100, msg, i));
				ok = status == RequestStatus.REQUESTSUCCESS;
				finished = ok || i >= 100;
				System.Threading.Thread.Sleep(2000);
			} while (!finished);
			//Console.ReadLine();
			return ok;
		}


	}
}
