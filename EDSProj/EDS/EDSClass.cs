using EDSProj.EDSWebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSProj
{
	public class EDSClass
	{
		public static EDSClass Single { get; protected set; }

		protected string _authStr { get; set; }

		protected edsPortTypeClient _client { get; set; }

		protected EDSClass() {
			
		}

		public static long toTS(DateTime date) {
			DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			TimeSpan diff = date.ToUniversalTime() - origin;
			return (long)(diff.TotalSeconds);
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
				finished = ok || i >= 5;
				System.Threading.Thread.Sleep(2000);
			} while (!finished);
			//Console.ReadLine();
			return ok;
		}


	}
}
