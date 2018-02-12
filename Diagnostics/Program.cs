using EDSProj;
using EDSProj.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diagnostics
{
	class Program
	{
		static void Main(string[] args) {
			Settings.init("Data/Settings.xml");
			Logger.InitFileLogger(Settings.Single.LogPath, "Diagnostics");
			if (args.Length >= 1) {
				string task = args[0];
				task = task.ToLower();
				switch (task) {
					case "processPump":
						try {
							DirectoryInfo di = new DirectoryInfo(Settings.Single.DiagFolder);
							DirectoryInfo newDI = new DirectoryInfo(Settings.Single + "/archive");
							if (!newDI.Exists)
								newDI.Create();
							FileInfo[] files = di.GetFiles();
							foreach (FileInfo fi in files) {
								try {
									PumpFileReader pumpReader = new PumpFileReader(fi.FullName);
									bool ok=pumpReader.ReadData();
									if (ok) {
										ok = pumpReader.writeToDB(4);
										if (ok) {
											fi.MoveTo(newDI.FullName);
										}
									}
								}catch (Exception e) {
									Logger.Info("Ошибка при обработке файла " + fi.FullName);
									Logger.Info(e.ToString());
								}
							}

							
						}catch (Exception e) {
							Logger.Info("Ошибка при обработке каталога " + e.ToString());
						}
						break;
				}
			} else {
				Console.WriteLine("Ключи командной строки: \r\n import: для импорта ПБР \r\n copy: для копирования файлов на ftp");
				Console.ReadLine();
			}
		}
	}
}
