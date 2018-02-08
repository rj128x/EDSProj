﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSProj.Diagnostics
{
	public class ReportOutputFile
	{
		public string FileName { get; set; }
		public List<Dictionary<int, string>> Data { get; set; }
		public ReportOutputFile(string fileName) {
			FileName = fileName;
			
		}

		public void ReadData() {
			Data = new List<Dictionary<int, string>>();
			TextReader reader = new StreamReader(FileName);
			string all=reader.ReadToEnd();
			reader.Close();
			string[] lines = all.Split(new char[] { '\n' });
			
			foreach (string line in lines) {
				if (line.Contains("NODATA") || line.Contains("NEVER"))
					continue;
				string[] objs = line.Split(new char[] { ',' });
				int index = 0;
				Dictionary<int, string> record = new Dictionary<int, string>();
				foreach (string obj in objs) {
					record.Add(index, obj);
					index++;
				}
				Data.Add(record);
			}
		}

		public static DateTime getDate(string dateStr) {
			try {
				string[] partsDateTime = dateStr.Split(new char[] { ' ' });
				string timePart = partsDateTime[0];
				string datePart = partsDateTime[1];

				string[] partsDate = datePart.Split(new char[] { '/' });
				int month = Int32.Parse(partsDate[0]);
				int day = Int32.Parse(partsDate[1]);
				int year = Int32.Parse(partsDate[2]);

				string[] partsTime = timePart.Split(new char[] { ':' });
				int hour = Int32.Parse(partsTime[0]);
				int min = Int32.Parse(partsTime[1]);
				int sec = Int32.Parse(partsTime[2]);

				DateTime result = new DateTime(year, month, day, hour, min, sec);
				return result;
			} catch {
				Logger.Info("Ошибка при разборе даты " + dateStr);
				return DateTime.MaxValue;
			}
		}

		public static double getDouble(string valStr) {
			try {
				double val = Double.Parse(valStr);
				return val;
			} catch {
				Logger.Info("Ошибка при разборе значения " + valStr);
				return Double.NaN;
			}
		}
	}
}