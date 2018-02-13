using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSProj.Diagnostics
{
	public class SvodDataRecord
	{

		public DateTime DateStart { get; set; }
		public DateTime DateEnd { get; set; }		
		public double PAvg { get; set; }
		public double PMin { get; set; }
		public double PMax { get; set; }
		public bool isUst { get; set; }

		public double LN1Time { get; set; }
		public double LN2Time { get; set; }
		public double DN1Time { get; set; }
		public double DN2Time { get; set; }
		public double MNU1Time { get; set; }
		public double MNU2Time { get; set; }
		public double MNU3Time { get; set; }

		public int LN1Pusk { get; set; }
		public int LN2Pusk { get; set; }
		public int DN1Pusk { get; set; }
		public int DN2Pusk { get; set; }
		public int MNU1Pusk { get; set; }
		public int MNU2Pusk { get; set; }
		public int MNU3Pusk { get; set; }

		public double GPHot { get; set; }
		public double GPCold { get; set; }
		public double GPLevel { get; set; }

		public double PPHot { get; set; }
		public double PPCold { get; set; }
		public double PPLevel { get; set; }


	}

	public class SvodFileReader
	{
		public static string InsertIntoHeader = "INSERT INTO SvodTable (DateStart, DateEnd,GG, P_Min,P_Max,P_Avg,IsUst,LN1_Time,LN2_Time,DN1_Time,DN2_Time,MNU1_Time,MNU2_Time,MNU3_Time,LN1_Pusk,LN2_Pusk,DN1_Pusk,DN2_Pusk,MNU1_Pusk,MNU2_Pusk,MNU3_Pusk,GP_Hot,GP_Cold,GP_Level,PP_Hot,PP_Cold,PP_Level)";
		public static string InsertIntoFormat = "SELECT '{0}','{1}',{2}, {3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26}";
		public static string DateFormat = "yyyy-MM-dd HH:mm:ss";
		SortedList<DateTime, SvodDataRecord> Data = new SortedList<DateTime, SvodDataRecord>();

		public string FileName { get; set; }
		public SvodFileReader(string fileName) {
			FileName = fileName;

		}

		public bool ReadData() {
			Data = new SortedList<DateTime, SvodDataRecord>();
			PumpTypeEnum type = PumpTypeEnum.Drenage;
			ReportOutputFile outFile = new ReportOutputFile(FileName);
			try {
				outFile.ReadData();
			} catch (Exception e) {
				Logger.Info("Ошибка инициализации файла " + FileName);
				Logger.Info(e.ToString());
				return false;
			}

			for (int i = 2; i < outFile.Data.Count; i++) {
				try {
					Dictionary<int, string> fileRec = outFile.Data[i];
					SvodDataRecord rec = new SvodDataRecord();
					rec.DateStart = ReportOutputFile.getDate(fileRec[0]);
					rec.DateEnd = ReportOutputFile.getDate(fileRec[1]);
					rec.PMin = ReportOutputFile.getDouble(fileRec[2]);
					rec.PMax = ReportOutputFile.getDouble(fileRec[3]);
					rec.PAvg = ReportOutputFile.getDouble(fileRec[4]);

					rec.LN1Time = ReportOutputFile.getDouble(fileRec[5]);
					rec.LN2Time = ReportOutputFile.getDouble(fileRec[6]);
					rec.DN1Time = ReportOutputFile.getDouble(fileRec[7]);
					rec.DN2Time = ReportOutputFile.getDouble(fileRec[8]);
					rec.MNU1Time = ReportOutputFile.getDouble(fileRec[9]);
					rec.MNU2Time = ReportOutputFile.getDouble(fileRec[10]);
					rec.MNU3Time = ReportOutputFile.getDouble(fileRec[11]);

					rec.LN1Pusk = (int)ReportOutputFile.getDouble(fileRec[12]);
					rec.LN2Pusk = (int)ReportOutputFile.getDouble(fileRec[13]);
					rec.DN1Pusk = (int)ReportOutputFile.getDouble(fileRec[14]);
					rec.DN2Pusk = (int)ReportOutputFile.getDouble(fileRec[15]);
					rec.MNU1Pusk = (int)ReportOutputFile.getDouble(fileRec[16]);
					rec.MNU2Pusk = (int)ReportOutputFile.getDouble(fileRec[17]);
					rec.MNU3Pusk = (int)ReportOutputFile.getDouble(fileRec[18]);

					rec.GPHot = ReportOutputFile.getDouble(fileRec[19]);
					rec.GPCold = ReportOutputFile.getDouble(fileRec[20]);
					rec.GPLevel = ReportOutputFile.getDouble(fileRec[21]);

					rec.PPHot = ReportOutputFile.getDouble(fileRec[22]);
					rec.PPCold = ReportOutputFile.getDouble(fileRec[23]);
					rec.PPLevel = ReportOutputFile.getDouble(fileRec[24]);

					rec.isUst = Math.Abs(rec.PAvg - rec.PMax) < 2;
					rec.isUst = rec.isUst && Math.Abs(rec.PAvg - rec.PMin) < 2;
					while (Data.ContainsKey(rec.DateStart))
						rec.DateStart = rec.DateStart.AddMilliseconds(1);
					Data.Add(rec.DateStart, rec);
				} catch (Exception e) {
					Logger.Info("ошибка при разборе строки");
					Logger.Info(e.ToString());
					return false;
				}

			}
			return true;
		}

		public bool writeToDB(int gg) {
			if (Data.Count == 0)
				return true;
			SqlConnection con = ReportOutputFile.getConnection();
			try {
				List<string> insQueries = new List<string>();

				string delQ = String.Format("delete FROM SvodTable where  DateStart>='{0}' and DateStart<='{1}' and GG={2}",
						Data.Keys.Min().ToString(DateFormat), Data.Keys.Max().ToString(DateFormat), gg);
				
				foreach (KeyValuePair<DateTime, SvodDataRecord> de in Data) {
					string ins = string.Format(InsertIntoFormat, de.Value.DateStart.ToString(DateFormat), de.Value.DateEnd.ToString(DateFormat), gg,
						de.Value.PMin.ToString().Replace(",", "."),
						de.Value.PMax.ToString().Replace(",", "."),
						de.Value.PAvg.ToString().Replace(",", "."),					
						de.Value.isUst ? 1 : 0,
						de.Value.LN1Time.ToString().Replace(",", "."),
						de.Value.LN2Time.ToString().Replace(",", "."),
						de.Value.DN1Time.ToString().Replace(",", "."),
						de.Value.DN2Time.ToString().Replace(",", "."),
						de.Value.MNU1Time.ToString().Replace(",", "."),
						de.Value.MNU2Time.ToString().Replace(",", "."),
						de.Value.MNU3Time.ToString().Replace(",", "."),
						de.Value.LN1Pusk.ToString().Replace(",", "."),
						de.Value.LN2Pusk.ToString().Replace(",", "."),
						de.Value.DN1Pusk.ToString().Replace(",", "."),
						de.Value.DN2Pusk.ToString().Replace(",", "."),
						de.Value.MNU1Pusk.ToString().Replace(",", "."),
						de.Value.MNU2Pusk.ToString().Replace(",", "."),
						de.Value.MNU3Pusk.ToString().Replace(",", "."),
						de.Value.GPHot.ToString().Replace(",", "."),
						de.Value.GPCold.ToString().Replace(",", "."),
						de.Value.GPLevel.ToString().Replace(",", "."),
						de.Value.PPHot.ToString().Replace(",", "."),
						de.Value.PPCold.ToString().Replace(",", "."),
						de.Value.PPLevel.ToString().Replace(",", ".")
					);
					insQueries.Add(ins);
				}

				con.Open();
				SqlTransaction trans = con.BeginTransaction();
				SqlCommand com = con.CreateCommand();
				com.CommandText = delQ;
				com.Transaction = trans;
				com.ExecuteNonQuery();

				string insertStr = String.Format("{0} {1}", InsertIntoHeader, String.Join("\nUNION ALL\n", insQueries));

				//Logger.Info(insertStr);

				SqlCommand comIns = con.CreateCommand();
				comIns.CommandText = insertStr;
				comIns.Transaction = trans;
				comIns.ExecuteNonQuery();

				trans.Commit();
				con.Close();
				return true;
			} catch (Exception e) {
				Logger.Info(e.ToString());
				return false;
			} finally {
				try {
					con.Close();
				} catch { }
			}
		}
	}
}
