using EDSProj.Diagnostics;
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
using ZedGraph;

namespace EDSApp
{
	/// <summary>
	/// Логика взаимодействия для DiagWindow.xaml
	/// </summary>
	public partial class DiagWindow : Window
	{
		public List<System.Drawing.Color> Colors;
		public DiagWindow() {
			InitializeComponent();
			clndFrom.SelectedDate = DateTime.Now.Date.AddMonths(-1);
			clndTo.SelectedDate = DateTime.Now.Date;
			Colors = new List<System.Drawing.Color>();
			Colors.Add(System.Drawing.Color.Black);
			Colors.Add(System.Drawing.Color.Red);
			Colors.Add(System.Drawing.Color.Green);
			Colors.Add(System.Drawing.Color.Blue);
			Colors.Add(System.Drawing.Color.Purple);
			Colors.Add(System.Drawing.Color.Pink);
			Colors.Add(System.Drawing.Color.Orange);
		}

		private void btnCreate_Click(object sender, RoutedEventArgs e) {

		}

		private void drenClick_Click(object sender, RoutedEventArgs e) {
			int splitPower = Int32.Parse(txtDNSplitPower.Text);
			createPumpRunChart(z1, PumpTypeEnum.Drenage, chbDNSplitPower.IsChecked.Value, splitPower);
			createPumpPuskChart(z2, PumpTypeEnum.Drenage, true);
			createPumpPuskChart(z3, PumpTypeEnum.Drenage,  false);

		}

		public void prepareChart(ZedGraphControl chart) {
			chart.GraphPane.CurveList.Clear();
			chart.GraphPane.XAxis.Type = AxisType.Date;
			chart.GraphPane.XAxis.Scale.Format = "dd.MM";
			chart.GraphPane.XAxis.Title.IsVisible = false;
			chart.GraphPane.YAxis.Title.IsVisible = false;
			chart.GraphPane.Title.IsVisible = false;
		}

		public void createPumpRunChart(ZedGraphControl chart, PumpTypeEnum type, bool split, int splitPower) {
			int ind = 0;
			PumpDiagnostics report = new PumpDiagnostics(clndFrom.SelectedDate.Value, clndTo.SelectedDate.Value);
			Dictionary<DateTime, PumpDataRecord> Data = new Dictionary<DateTime, PumpDataRecord>();

			prepareChart(chart);

			List<int> powers = new List<int>();
			if (split) {
				powers.Add(-1);
				int power = 35;
				while (power + splitPower <= 115) {
					powers.Add(power);
					power += splitPower;
				}
			} else {
				splitPower = 200;
				powers.Add(-1);
			}
			foreach (int p in powers) {
				string header = "";
				if (!split) {
					Data = report.ReadDataPump(type, -1, 200);
					header = "Время работы насосов";
				} else {
					if (p == -1) {
						Data = report.ReadDataPump(type, -1, 0);
						header = "Простой";
					} else {
						Data = report.ReadDataPump(type, p, p + splitPower);
						header = String.Format("Работа при P {0}-{1}", p, p + splitPower);
					}
				}


				if (Data.Count > 0) {
					System.Drawing.Color color = Colors[ind++ % 7];
					PointPairList points = new PointPairList();
					foreach (KeyValuePair<DateTime, PumpDataRecord> de in Data) {
						points.Add(new PointPair(new XDate(de.Key), de.Value.RunTime));
					}
					LineItem line = chart.GraphPane.AddCurve(header, points, color, SymbolType.Circle);
					line.Line.IsVisible = false;
					line.Symbol.Fill = new Fill(color);
					line.Symbol.Size = 2;
				}
			}
			chart.AxisChange();
			chart.Invalidate();
		}

		public void createPumpPuskChart(ZedGraphControl chart, PumpTypeEnum type, bool time) {
			int ind = 0;
			PumpDiagnostics report = new PumpDiagnostics(clndFrom.SelectedDate.Value, clndTo.SelectedDate.Value);
			Dictionary<DateTime, SvodDataRecord> Data = new Dictionary<DateTime, SvodDataRecord>();

			prepareChart(chart);



			List<int> powers = new List<int>();

			Data = report.ReadPumpSvod("P_Avg", -1, 200);


			if (Data.Count > 0) {
				System.Drawing.Color color = Colors[ind++ % 7];
				PointPairList points = new PointPairList();
				foreach (KeyValuePair<DateTime, SvodDataRecord> de in Data) {
					switch (type) {
						case PumpTypeEnum.Drenage:
							points.Add(new PointPair(new XDate(de.Key), time ? de.Value.DN1Time + de.Value.DN2Time : de.Value.DN1Pusk + de.Value.DN2Pusk));
							break;
						case PumpTypeEnum.MNU:
							points.Add(new PointPair(new XDate(de.Key), time ? de.Value.MNU1Time + de.Value.MNU2Time + de.Value.MNU3Time : de.Value.MNU1Pusk + de.Value.MNU2Pusk + de.Value.MNU3Pusk));
							break;
					}
				}
				LineItem line = chart.GraphPane.AddCurve(String.Format("{0}", time ? "Работа (ceк)" : "Пусков"), points, color, SymbolType.Circle);
				line.Line.IsVisible = true;
				line.Symbol.Size = 1;
				line.Symbol.Fill = new Fill(color);
			}

			chart.AxisChange();
			chart.Invalidate();
		}

		private void mnuClick_Click(object sender, RoutedEventArgs e) {
			int splitPower = Int32.Parse(txtMNUSplitPower.Text);
			createPumpRunChart(z4, PumpTypeEnum.MNU, chbMNUSplitPower.IsChecked.Value, splitPower);
			createPumpPuskChart(z5, PumpTypeEnum.MNU, true);
			createPumpPuskChart(z6, PumpTypeEnum.MNU, false);
		}

		public void createOilChart(ZedGraphControl chart, bool gp, bool splitHot, bool splitCold, double step) {
			int ind = 0;
			PumpDiagnostics report = new PumpDiagnostics(clndFrom.SelectedDate.Value, clndTo.SelectedDate.Value);
			Dictionary<DateTime, SvodDataRecord> Data = new Dictionary<DateTime, SvodDataRecord>();

			prepareChart(chart);

			string obj = gp ? "GP_" : "PP_";
			string temp = splitHot ? "Hot" : "Cold";
			obj = obj + temp;
			bool split = splitHot || splitCold;

			List<double> AllTemps = new List<double>();
			if (split) {
				double t = 10;
				while (t <= 50) {
					AllTemps.Add(t);
					t += step;
				}
			} else {
				step = 50;
				AllTemps.Add(10);
			}
			foreach (double t in AllTemps) {
				string headerRun = "";
				string headerStop = "";
				if (!split) {
					headerRun = "Уровень масла (ГГ в работе)";
					headerStop = "Уровень масла (ГГ стоит)";
				} else {
					headerRun = String.Format("Уровень при t {0:0.0}-{1:0.0} (ГГ в работе)", t, t + step);
					headerStop = String.Format("Уровень при t {0:0.0}-{1:0.0} (стоит)", t, t + step);
				}

				PointPairList pointsRun = new PointPairList();
				PointPairList pointsStop = new PointPairList();

				Data = report.ReadSvod(obj, t, t + step);
				foreach (KeyValuePair<DateTime, SvodDataRecord> de in Data) {
					if (de.Value.PAvg < 1) {
						if (gp)
							pointsStop.Add(new PointPair(new XDate(de.Key), de.Value.GPLevel));
						else
							pointsStop.Add(new PointPair(new XDate(de.Key), de.Value.PPLevel));
					} else {
						if (gp)
							pointsRun.Add(new PointPair(new XDate(de.Key), de.Value.GPLevel));
						else
							pointsRun.Add(new PointPair(new XDate(de.Key), de.Value.PPLevel));
					}
				}
				System.Drawing.Color color = Colors[ind++ % 7];
				if (pointsRun.Count > 10) {
					LineItem line = chart.GraphPane.AddCurve(headerRun, pointsRun, color, SymbolType.Circle);
					line.Line.IsVisible = false;
					line.Symbol.Size = 2;
					line.Symbol.Fill = new Fill(color);
				}
				//line.Line.IsVisible = false;
				if (pointsRun.Count > 10) {
					LineItem line = chart.GraphPane.AddCurve(headerStop, pointsStop, color, SymbolType.Circle);
					line.Line.IsVisible = false;
					line.Symbol.Size = 2;
					line.Symbol.Fill = new Fill(color);

				}
				//line.Line.IsVisible = false;
			}
			chart.AxisChange();
			chart.Invalidate();
		}

		private void oilClick_Click(object sender, RoutedEventArgs e) {
			bool splitHot = chbOilSplitHot.IsChecked.Value;
			bool splitCold = chbOilSplitCold.IsChecked.Value;
			double step = Double.Parse(txtOilSplitTemp.Text);
			createOilChart(z7, true, splitHot, splitCold, step);
			createOilChart(z8, false, splitHot, splitCold, step);
		}


	}
}
