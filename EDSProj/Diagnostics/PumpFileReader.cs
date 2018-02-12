﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
		public static string InsertIntoHeader = "INSERT INTO PumpTable (DateStart, DateEnd,GG, PumpType, PumpNum, RunTime,LevelStart,LevelStop,PAvg,PMin,PMax,IsUst)";
		public static string InsertIntoFormat = "SELECT '{0}','{1}',{2},'{3}',{4},{5},{6},{7},{8},{9},{10},{11}";
		public static string DateFormat = "yyyy-MM-dd HH:mm:ss";
		SortedList<DateTime, PumpDataRecord> Data = new SortedList<DateTime, PumpDataRecord>();

		public string FileName { get; set; }
		public PumpFileReader(string fileName) {
			FileName = fileName;

		}

		public bool ReadData() {
			Data = new SortedList<DateTime, PumpDataRecord>();
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

		public static SqlConnection getConnection() {
			String str = String.Format("Data Source={0};Initial Catalog={1};Persist Security Info=True;User ID={2};Password={3};Trusted_Connection=False;",
				Settings.Single.DiadDBServer, Settings.Single.DiagDBName, Settings.Single.DiagDBUser, Settings.Single.DiagDBUser);
			return new SqlConnection(str);
		}

		public bool writeToDB(int gg) {
			SqlConnection con = getConnection();
			try {
				Dictionary<PumpTypeEnum, List<DateTime>> DelDates = new Dictionary<PumpTypeEnum, List<DateTime>>();
				Dictionary<PumpTypeEnum, string> DelQueries = new Dictionary<PumpTypeEnum, string>();
				List<string> insQueries = new List<string>();

				foreach (PumpDataRecord rec in Data.Values) {
					if (!DelDates.ContainsKey(rec.PumpType)) {
						DelDates.Add(rec.PumpType, new List<DateTime>());						
					}
					DelDates[rec.PumpType].Add(rec.DateStart);
				}
				foreach (PumpTypeEnum pumpType in DelDates.Keys) {
					List<DateTime> dd = DelDates[pumpType];
					string del=String.Format("delete FROM PumpTable where PumpType='{0}' and DateStart>='{1}' and DateStart<='{2}' and GG={3}",
						pumpType, dd.Min(), dd.Max(), gg);
					DelQueries.Add(pumpType, del);
				}
				foreach (KeyValuePair<DateTime,PumpDataRecord> de in Data) {
					string ins = string.Format(InsertIntoFormat, de.Value.DateStart.ToString(DateFormat), de.Value.DateEnd.ToString(DateFormat), gg,
						de.Value.PumpType.ToString(), de.Value.PumpNum,
						de.Value.RunTime.ToString().Replace(",", "."),
						de.Value.LevelStart.ToString().Replace(",", "."),
						de.Value.LevelStop.ToString().Replace(",", "."),
						de.Value.PAvg.ToString().Replace(",", "."),
						de.Value.PMin.ToString().Replace(",", "."),
						de.Value.PMax.ToString().Replace(",", "."),
						de.Value.isUst ? 1 : 0);
					insQueries.Add(ins);
				}

				con.Open();
				SqlTransaction trans=con.BeginTransaction();
				foreach (string delStr in DelQueries.Values) {
					SqlCommand com = con.CreateCommand();
					com.CommandText = delStr;
					com.ExecuteNonQuery();					
				}
				string insertStr = String.Format("{0} {1}", InsertIntoHeader, String.Join("\nUNION ALL\n",insQueries));

				SqlCommand comIns = con.CreateCommand();
				comIns.CommandText = insertStr;
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
