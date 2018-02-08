using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSProj.Diagnostics
{	
	public enum PumpTypeEnum { Drenage,Leakage,MNU}
	public class PumpDataRecord
	{

		public DateTime DateStart { get; set; }
		public DateTime DateEnd { get; set; }
		public PumpTypeEnum PumpType { get; set; }
		public int PumpNum { get; set; }
		public double RunTime { get; set; }
		public double LevelStart { get; set; }
		public double LevelStop { get; set; }
		public double PAvg { get; set; }
		public double PMin { get; set; }
		public double PMax { get; set; }
		public bool isUst { get; set; }
	}


	public class PumpFileReader
	{
		public string FileName { get; set; }
		public PumpFileReader(string fileName) {
			FileName = fileName;

		}

		public bool ReadData() {
			SortedList<DateTime, PumpDataRecord> Data = new SortedList<DateTime, PumpDataRecord>();
			PumpTypeEnum type = PumpTypeEnum.Drenage;
			int pumpNumber = 0;
			ReportOutputFile outFile = new ReportOutputFile(FileName);
			try {				
				outFile.ReadData();				
				FileInfo fi = new FileInfo(FileName);
				if (fi.Name.Contains("DN_"))
					type = PumpTypeEnum.Drenage;
				else if (fi.Name.Contains("MNU_"))
					type = PumpTypeEnum.MNU;
				else if (fi.Name.Contains("LN_"))
					type = PumpTypeEnum.Leakage;
				else {
					Logger.Info("Не определен тип насоса в файле " + FileName);
					return false;
				}
				switch (type) {
					case PumpTypeEnum.Drenage:
						pumpNumber = (int)ReportOutputFile.getDouble(outFile.Data[0][3]);
						break;
					case PumpTypeEnum.Leakage:
						pumpNumber = (int)ReportOutputFile.getDouble(outFile.Data[0][3]);
						break;
					case PumpTypeEnum.MNU:
						pumpNumber = (int)ReportOutputFile.getDouble(outFile.Data[0][4]);
						break;
				}
			}catch (Exception e) {
				Logger.Info("Ошибка инициализации файла " + FileName);
				Logger.Info(e.ToString());
			}
				
			for (int i = 2; i < outFile.Data.Count;i++) {
				try {
					Dictionary<int, string> fileRec = outFile.Data[i];
					PumpDataRecord rec = new PumpDataRecord();
					rec.DateStart = ReportOutputFile.getDate(fileRec[0]);
					rec.DateEnd = ReportOutputFile.getDate(fileRec[1]);
					rec.PAvg = ReportOutputFile.getDouble(fileRec[2]);
					rec.PMin = ReportOutputFile.getDouble(fileRec[3]);
					rec.PMax = ReportOutputFile.getDouble(fileRec[4]);
					rec.RunTime = ReportOutputFile.getDouble(fileRec[5]);
					rec.LevelStart = ReportOutputFile.getDouble(fileRec[6]);
					rec.LevelStop = ReportOutputFile.getDouble(fileRec[7]);
					rec.PumpType = type;
					rec.PumpNum = pumpNumber;
					rec.isUst = Math.Abs(rec.PAvg - rec.PMax) < 2;
					rec.isUst = rec.isUst && Math.Abs(rec.PAvg - rec.PMin) < 2;
					while (Data.ContainsKey(rec.DateStart))
						rec.DateStart = rec.DateStart.AddMilliseconds(1);
					Data.Add(rec.DateStart, rec);
				}catch (Exception e) {
					Logger.Info("ошибка при разборе строки");
					Logger.Info(e.ToString());
				}				

			}
			return true;
		}

		public void writeToDB() {

		}
	}
}
