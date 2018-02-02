using EDSProj;
using EDSProj.EDS;
using EDSProj.EDSWebService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EDSApp
{
	/// <summary>
	/// Логика взаимодействия для ReportWindow.xaml
	/// </summary>
	public partial class ReportWindow : Window
	{
		public ReportWindow() {
			InitializeComponent();
			cntrlSelectPoints.init();
			cmbPeriod.ItemsSource = EDSClass.ReportPeriods;

			clndFrom.SelectedDate = DateTime.Now.Date.AddDays(-1);
			clndTo.SelectedDate = DateTime.Now.Date;
			cmbPeriod.SelectedValue = EDSReportPeriod.hour;
			grdStatus.DataContext = EDSClass.Single;
		}



		private async void  btnCreate_Click(object sender, RoutedEventArgs e) {
			if (!EDSClass.Single.Ready) {
				MessageBox.Show("ЕДС сервер не готов");
				return;
			}
			if (!clndFrom.SelectedDate.HasValue) {
				MessageBox.Show("Выберите дату начала");
				return;
			}
			if (!clndFrom.SelectedDate.HasValue) {
				MessageBox.Show("Выберите дату конца");
				return;
			}
			/*if (cmbFunction.SelectedItem == null) {
				MessageBox.Show("Выберите функцию");
				return;
			}*/
			if (cmbPeriod.SelectedItem == null) {
				MessageBox.Show("Выберите период");
				return;
			}

			if (cntrlSelectPoints.SelectedPoints.Count() == 0) {
				MessageBox.Show("Выберите точки");
				return;
			}


			DateTime dtStart = clndFrom.SelectedDate.Value;
			DateTime dtEnd = clndTo.SelectedDate.Value;
			//EDSReportFunction func = (EDSReportFunction)cmbFunction.SelectedValue;

			EDSReportPeriod period = (EDSReportPeriod)cmbPeriod.SelectedValue;

			EDSReport report = new EDSReport(dtStart, dtEnd, period, chbMsk.IsChecked.Value);

			foreach (EDSReportRequestRecord rec in cntrlSelectPoints.SelectedPoints) {
				report.addRequestField(rec.Point, rec.Function);
			}

			//System.Windows.Application.Current.Dispatcher.Invoke( System.Windows.Threading.DispatcherPriority.Background, new System.Action(delegate { report.ReadData(); }));
			

			
			bool ready=await report.ReadData();

			String header = "";
			foreach (EDSReportRequestRecord rec in report.RequestData.Values) {
				header += String.Format("<th>{0}</th>", rec.Desc);
			}

			TextWriter tW = new StreamWriter("out.html");

			String txt = string.Format(@"<html>
				<head>
					<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
            </head>
				<table border='1'><tr><th>точка</th>{0}</tr>", header);
			tW.WriteLine(txt);

			foreach (KeyValuePair<DateTime, Dictionary<string, double>> de in report.ResultData) {
				DateTime dt = de.Key;
				string ValuesStr = "";
				foreach (double val in de.Value.Values) {
					ValuesStr += String.Format("<td align='right'>{0:0.00}</td>", val);
				}
				tW.WriteLine(String.Format("<tr><th>{0}</th>{1}</tr>", dt.ToString("dd.MM.yyyy HH:mm"), ValuesStr));
				//txt += String.Format("<tr><th>{0}</th>{1}</tr>", dt.ToString("dd.MM.yyyy HH:mm"), ValuesStr);
			}
			//txt += "</table></html>";
			tW.WriteLine("</table></html>");
			tW.Close();



			//ReportResultWindow win = new ReportResultWindow();
			//win.wbResult.NavigateToString(txt);
			/*Uri uri = new Uri(@"c:/out.html", UriKind.Relative);
			Stream stream = Application.GetContentStream(uri).Stream;
			win.wbResult.NavigateToStream(stream);
			win.Show();*/
			Process.Start("out.html");

			//thread.SetApartmentState(ApartmentState.STA);
			//thread.Start();

			//thread.Join();
			//report.ReadData();



		}

		private void btnAbort_Click(object sender, RoutedEventArgs e) {
			//MessageBox.Show("прерывание");
			EDSClass.Single.Abort();
		}
	}
}

