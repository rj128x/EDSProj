using EDSProj;
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

			TabularRequest req = new TabularRequest();
			req.period = new TimePeriod() {
				from = new Timestamp() { second = EDSClass.toTS(dtStart) },
				till = new Timestamp() { second = EDSClass.toTS(dtEnd) }
			};

			EDSReportFunction func = (EDSReportFunction)cmbFunction.SelectedValue;
			EDSReportPeriod period = (EDSReportPeriod)cmbPeriod.SelectedValue;
			List<TabularRequestItem> list = new List<TabularRequestItem>();
			foreach (EDSPointInfo point in cntrlSelectPoints.SelectedPoints) {
				list.Add(new TabularRequestItem() {
					function = EDSClass.getReportFunctionName(func),
					pointId = new PointId() { iess = point.IESS }
				});
			}

			req.step = new TimeDuration() { seconds = EDSClass.getPeriodSeconds(period) };
			req.items = list.ToArray();

			if (!EDSClass.Connected)
				EDSClass.Connect();
			if (EDSClass.Connected) {
				uint id = EDSClass.Client.requestTabular(EDSClass.AuthStr, req);
				TabularRow[] rows;
				EDSClass.ProcessQuery(id);
				PointId[] points = EDSClass.Client.getTabular(EDSClass.AuthStr, id, out rows);

				String header = "";
				foreach (PointId pnt in points) {
					header+=String.Format("<th>{0}</th>", EDSPointsClass.AllAnalogPoints[pnt.iess].Desc);
				}

				String txt = string.Format(@"<html>
				<head>
					<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
            </head>
				<table border='1'><tr><th>точка</th>{0}</tr>", header);

				foreach (TabularRow row in rows) {
					DateTime dt = EDSClass.fromTS(row.ts.second);
					string ValuesStr = "";
					foreach (TabularValue val in row.values) {
						ValuesStr += String.Format("<td align='right'>{0:0.00}</td>", val.value.av);
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
}
