using EDSProj;
using EDSProj.EDS;
using EDSProj.EDSWebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			cmbFunction.ItemsSource = EDSClass.ReportFunctions;
			clndFrom.SelectedDate = DateTime.Now.Date.AddDays(-1);
			clndTo.SelectedDate = DateTime.Now.Date;
		}

		private void btnCreate_Click(object sender, RoutedEventArgs e) {
			if (!clndFrom.SelectedDate.HasValue) {
				MessageBox.Show("Выберите дату начала");
				return;
			}
			if (!clndFrom.SelectedDate.HasValue) {
				MessageBox.Show("Выберите дату конца");
				return;
			}
			if (cmbFunction.SelectedItem == null) {
				MessageBox.Show("Выберите функцию");
				return;
			}
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
			EDSReportFunction func = (EDSReportFunction)cmbFunction.SelectedValue;
			EDSReportPeriod period = (EDSReportPeriod)cmbPeriod.SelectedValue;

			EDSReport report = new EDSReport(dtStart, dtEnd, period, chbMsk.IsChecked.Value);

			foreach (EDSPointInfo point in cntrlSelectPoints.SelectedPoints) {
				report.addRequestField(point, func);
			}

			report.ReadData();

			String header = "";
			foreach (EDSReportRequestRecord rec in report.RequestData.Values) {
				header += String.Format("<th>{0}</th>", rec.Point.Desc);
			}

			String txt = string.Format(@"<html>
				<head>
					<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
            </head>
				<table border='1'><tr><th>точка</th>{0}</tr>", header);

			foreach (KeyValuePair<DateTime, Dictionary<string, double>> de in report.ResultData) {
				DateTime dt = de.Key;
				string ValuesStr = "";
				foreach (double val in de.Value.Values) {
					ValuesStr += String.Format("<td align='right'>{0:0.00}</td>", val);
				}
				txt += String.Format("<tr><th>{0}</th>{1}</tr>", dt.ToString("dd.MM.yyyy HH:mm"), ValuesStr);
			}
			txt += "</table></html>";



			ReportResultWindow win = new ReportResultWindow();
			win.wbResult.NavigateToString(txt);
			win.Show();

		}
	}
}

