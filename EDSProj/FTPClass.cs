﻿using BytesRoad.Net.Ftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDSProj
{
	/// <summary>
	/// Класс для работы с ftp
	/// </summary>
	public class FTPClass
	{
		/// <summary>
		/// Отправка файла на сервер
		/// </summary>
		/// <param name="fileName">Имя файла</param>
		/// <returns>true, если файл успешно отправлен</returns>
		public static bool SendFile(string fileName) {
			bool ok = true;
			try {
				Logger.Info("Отправка файла на ftp: " + fileName);
				FtpClient client = new FtpClient();
				int timeout = 10000;

				//Подключение к ftp
				client.PassiveMode = !Settings.Single.FTPActive;
				Logger.Info("try connect");
				client.Connect(timeout, Settings.Single.FTPServer, Settings.Single.FTPPort);
				Logger.Info("try login");
				client.Login(timeout, Settings.Single.FTPUser, Settings.Single.FTPPassword);
				Logger.Info("login");
				FileInfo fi = new FileInfo(fileName);


				try //Отправка файла на ftp
				{
					client.PutFile(timeout, fi.Name, fileName);
				} catch (Exception e) {
					Logger.Info("-----");
					Logger.Info(e.ToString());
					ok = false;
				}
				client.Disconnect(timeout);
			} catch (Exception e) {
				Logger.Info("Ошибка при отправке файла");
				Logger.Info(e.ToString());
				ok = false;
				//MailClass.SendTextMail(String.Format("Ошибка при отправке отчета НПРЧ {0} ", fileName), e.ToString());
			}
			Logger.Info("Отправка завершена: " + ok.ToString());
			return ok;
		}

		public static bool copyFolder() {
			bool ok = true;
			try {
				DirectoryInfo dir = new DirectoryInfo(Settings.Single.AOLocalDir);
				FileInfo[] files = dir.GetFiles();
				foreach (FileInfo file in files) {
					try {
						Logger.Info("Копирование файла " + file.FullName);
						FtpClient client = new FtpClient();
						int timeout = 10000;
						//Подключение к ftp
						client.PassiveMode = !Settings.Single.FTPAOActive;
						Logger.Info("try connect");
						client.Connect(timeout, Settings.Single.FTPAOServer, Settings.Single.FTPAOPort);
						Logger.Info("try login");
						client.Login(timeout, Settings.Single.FTPAOUser, Settings.Single.FTPAOPassword);
						Logger.Info("login");

						client.PutFile(timeout, file.Name, file.FullName);

						client.Disconnect(timeout);
						file.Delete();
					} catch (Exception e) {
						Logger.Info(e.ToString());
						ok = false;
					}
				}
			} catch (Exception e) {
				Logger.Info("Ошибка при копировании файлов " + e.ToString());
				ok = false;
			}
			return ok;
		}




	}
}
