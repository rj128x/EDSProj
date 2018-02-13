﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSProj.Diagnostics
{
	public enum PumpTypeEnum { Drenage, Leakage, MNU }

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
		public bool IsUst { get; set; }
	}

	public class SvodDataRecord
	{

		public DateTime DateStart { get; set; }
		public DateTime DateEnd { get; set; }
		public double PAvg { get; set; }
		public double PMin { get; set; }
		public double PMax { get; set; }
		public bool IsUst { get; set; }

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


	public class PumpDiagnostics
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public static string DateFormat = "yyyy-MM-dd HH:mm:ss";

		public Dictionary<DateTime, PumpDataRecord> ReadDataPump(PumpTypeEnum type,int powerStart,int powerStop) {
			SqlConnection con = ReportOutputFile.getConnection();
			con.Open();
			string query = String.Format("Select * from pumpTable where dateStart>='{0}' and dateEnd<='{1}' and isUst=1 and pAvg>={2} and pAvg<={3} and PumpType='{4}'",
				StartDate.ToString(DateFormat), EndDate.ToString(DateFormat), powerStart, powerStop,type.ToString());

			SqlCommand com = con.CreateCommand();
			com.CommandText = query;

			Dictionary<DateTime, PumpDataRecord> Data = new Dictionary<DateTime, PumpDataRecord>();
			SqlDataReader reader = com.ExecuteReader();
			while (reader.Read()) {
				PumpDataRecord rec = new PumpDataRecord();
				rec.DateStart = reader.GetDateTime(reader.GetOrdinal("DateStart"));
				rec.DateEnd = reader.GetDateTime(reader.GetOrdinal("DateEnd"));
				rec.IsUst = reader.GetBoolean(reader.GetOrdinal("IsUst"));
				rec.PumpNum = reader.GetInt32(reader.GetOrdinal("PumpNum"));
				rec.RunTime = reader.GetDouble(reader.GetOrdinal("RunTime"));
				rec.LevelStart = reader.GetDouble(reader.GetOrdinal("LevelStart"));
				rec.LevelStop = reader.GetDouble(reader.GetOrdinal("LevelStop"));
				rec.PAvg = reader.GetDouble(reader.GetOrdinal("PAvg"));
				rec.PMin = reader.GetDouble(reader.GetOrdinal("PMin"));
				rec.PMax = reader.GetDouble(reader.GetOrdinal("PMax"));
				string tp = reader.GetString(reader.GetOrdinal("PumpType"));
				rec.PumpType=(PumpTypeEnum)Enum.Parse(typeof(PumpTypeEnum), tp);
				Data.Add(rec.DateStart, rec);

			}
			return Data;
		}

		public Dictionary<DateTime, SvodDataRecord> ReadSvod(string groupName,double start,double stop) {
			SqlConnection con = ReportOutputFile.getConnection();
			con.Open();
			string query = String.Format("Select * from svodTable where dateStart>='{0}' and dateEnd<='{1}' and isUst=1 and {2}>={3} and {2}<={4}",
				StartDate.ToString(DateFormat), EndDate.ToString(DateFormat), groupName, start.ToString().Replace(",","."), stop.ToString().Replace(",", "."));

			SqlCommand com = con.CreateCommand();
			com.CommandText = query;

			Dictionary<DateTime, SvodDataRecord> Data = new Dictionary<DateTime, SvodDataRecord>();
			SqlDataReader reader = com.ExecuteReader();
			while (reader.Read()) {
				SvodDataRecord rec = new SvodDataRecord();
				rec.DateStart = reader.GetDateTime(reader.GetOrdinal("DateStart"));
				rec.DateEnd = reader.GetDateTime(reader.GetOrdinal("DateEnd"));
				rec.IsUst = reader.GetBoolean(reader.GetOrdinal("IsUst"));
				rec.PAvg = reader.GetDouble(reader.GetOrdinal("P_Avg"));
				rec.PMin = reader.GetDouble(reader.GetOrdinal("P_Min"));
				rec.PMax = reader.GetDouble(reader.GetOrdinal("P_Max"));

				rec.LN1Time = reader.GetDouble(reader.GetOrdinal("LN1_Time"));
				rec.LN2Time = reader.GetDouble(reader.GetOrdinal("LN2_Time"));
				rec.DN1Time = reader.GetDouble(reader.GetOrdinal("DN1_Time"));
				rec.DN2Time = reader.GetDouble(reader.GetOrdinal("DN2_Time"));
				rec.MNU1Time = reader.GetDouble(reader.GetOrdinal("MNU1_Time"));
				rec.MNU2Time = reader.GetDouble(reader.GetOrdinal("MNU2_Time"));
				rec.MNU3Time = reader.GetDouble(reader.GetOrdinal("MNU3_Time"));

				rec.LN1Pusk = reader.GetInt32(reader.GetOrdinal("LN1_Pusk"));
				rec.LN2Pusk = reader.GetInt32(reader.GetOrdinal("LN2_Pusk"));
				rec.DN1Pusk = reader.GetInt32(reader.GetOrdinal("DN1_Pusk"));
				rec.DN2Pusk = reader.GetInt32(reader.GetOrdinal("DN2_Pusk"));
				rec.MNU1Pusk = reader.GetInt32(reader.GetOrdinal("MNU1_Pusk"));
				rec.MNU2Pusk = reader.GetInt32(reader.GetOrdinal("MNU2_Pusk"));
				rec.MNU3Pusk = reader.GetInt32(reader.GetOrdinal("MNU3_Pusk"));

				rec.GPHot = reader.GetDouble(reader.GetOrdinal("GP_Hot"));
				rec.GPCold = reader.GetDouble(reader.GetOrdinal("GP_Cold"));
				rec.GPLevel = reader.GetDouble(reader.GetOrdinal("GP_Level"));

				rec.PPHot = reader.GetDouble(reader.GetOrdinal("PP_Hot"));
				rec.PPCold = reader.GetDouble(reader.GetOrdinal("PP_Cold"));
				rec.PPLevel = reader.GetDouble(reader.GetOrdinal("PP_Level"));

			}
			return Data;
		}
	}
}