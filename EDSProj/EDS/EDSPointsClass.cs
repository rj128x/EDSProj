using EDSProj.EDSWebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSProj
{
	public class EDSPointInfo
	{
		public string IESS { get; set; }
		public string Desc { get; set; }

		public EDSPointInfo(string iess,string desc) {
			this.IESS = iess;
			this.Desc = desc;
		}
	}

	public class EDSPointsClass
	{
		public static List<EDSPointInfo> GetAllPoints() {
			List<EDSPointInfo> res = new List<EDSPointInfo>();
			//try {
				if (!EDSClass.Connected)
					EDSClass.Connect();
				if (EDSClass.Connected) {
					PointFilter filter = new PointFilter();
					//filter.rt = new PointType[] { PointType.POINTTYPEANALOG, PointType.POINTTYPEDOUBLE, PointType.POINTTYPEINT64 };
					uint cnt;
					uint total;
					EDSClass.Client.getPoints(EDSClass.AuthStr, filter, "desc", 0, 1024, out cnt, out total);
					/*foreach (Point point in points) {
						res.Add(new EDSPointInfo(point.id.iess, point.desc));
					}*/
				}
			/*}catch (Exception e) {
				Logger.Info(("Ошибка при получении списка точек: " + e.ToString()));
			}*/
			return res;
		}
	}
}
