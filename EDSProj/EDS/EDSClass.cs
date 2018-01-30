using EDSProj.EDSWebService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EDSProj
{
	public enum EDSReportPeriod { minute, hour, day, month }
	public enum EDSReportFunction { avg, max, min, val, vyrab }
	public class TechGroupInfo {
		public string Name { get; set; }
		public string Desc { get; set; }
		public int Id { get; set; }
		public bool Selected { get; set; }
	}

	public delegate void StateChangeDelegate();

	public class EDSClass : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public static EDSClass Single { get; protected set; }


		protected string _authStr { get; set; }
		protected edsPortTypeClient _client { get; set; }
		
		public static Dictionary<EDSReportPeriod, string> ReportPeriods { get; protected set; }
		public static Dictionary<EDSReportFunction, string> ReportFunctions { get; protected set; }


		protected static Dictionary<int, TechGroupInfo> _techGroups { get; set; }
		public static Dictionary<int, TechGroupInfo> TechGroups {
			get {
				if (_techGroups == null)
					_techGroups = Single.getTechGroups();
				return _techGroups;
			}
		}

		public void NotifyChanged(string propName) {
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propName));
		}

		protected  string _globalInfo;
		public String GlobalInfo { get {
				return _globalInfo;
			}
			protected set {
				_globalInfo = value;
				NotifyChanged("GlobalInfo");
			}
		}

		protected string _connectInfo;
		public  String ConnectInfo { get {
				return _connectInfo;
			} protected set {
				_connectInfo = value;
				NotifyChanged("ConnectInfo");
			}
		}

		public  EDSClass() {
			
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
			ConnectInfo = "Попытка подключения";
			if (_client == null) {
				_client = new edsPortTypeClient();								
			}
			Logger.Info("Проверка состояния подключения");
			if (_client.State != System.ServiceModel.CommunicationState.Opened) {
				_authStr = _client.login(Settings.Single.EDSUser, Settings.Single.EDSPassword, ClientType.CLIENTTYPEDEFAULT);
			}
			Logger.Info("Состояние подключения: " + Client.State);
			ConnectInfo = "Состояние подключения: " + Client.State;
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
			ReportPeriods.Add(EDSReportPeriod.month, "Месяц");

			ReportFunctions = new Dictionary<EDSReportFunction, string>();
			ReportFunctions.Add(EDSReportFunction.avg, "Среднее");
			ReportFunctions.Add(EDSReportFunction.vyrab, "Выработка");
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
				case EDSReportFunction.vyrab:
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
				Single.GlobalInfo=String.Format("Запрос {3} {0}: {1} ({2})", status, progress * 100, msg, i);
				finished = ok || i >= 100;
				Thread.Sleep(1000);
			} while (!finished);

			return ok;
		}

		public static async Task<bool> ProcessQueryAsync(uint id) {
			bool finished = false;
			int i = 0;
			bool ok = false;
			getRequestStatusRequest req = new getRequestStatusRequest(Single._authStr, id);
			do {
				i++;
				getRequestStatusResponse res=await Single._client.getRequestStatusAsync(req);
				Logger.Info(String.Format("{3} {0}: {1} ({2})", res.status, res.progress * 100, res.message, i));
				ok = res.status == RequestStatus.REQUESTSUCCESS;
				Single.GlobalInfo = String.Format("{3} {0}: {1:0.00} ({2})", res.status, res.progress * 100, res.message, i);
				finished = ok || i >= 100;
				
				Thread.Sleep(1000);
			} while (!finished);
			return ok;
		}

		protected Dictionary<int, TechGroupInfo> getTechGroups() {
			if (!Connected)
				Connect();
			Group[] groups=Client.getTechnologicalGroups(AuthStr);
			Dictionary<int, TechGroupInfo> Result = new Dictionary<int, TechGroupInfo>();
			foreach (Group gr in groups) {
				if (!String.IsNullOrEmpty(gr.desc)) {
					TechGroupInfo tg = new TechGroupInfo();
					tg.Id = gr.id;
					tg.Name = gr.name;
					tg.Desc = gr.desc;
					Result.Add(gr.id, tg);
				}
			}
			return Result;
		}


	}
}
