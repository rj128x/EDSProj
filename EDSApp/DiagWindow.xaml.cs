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
			clndFrom.SelectedDate = DateTime.Now.Date.AddMonths(-3);
			clndTo.SelectedDate = DateTime.Now.Date;
			Colors = new List<System.Drawing.Color>();			
			Colors.Add(System.Drawing.Color.Red);
			Colors.Add(System.Drawing.Color.Green);
			Colors.Add(System.Drawing.Color.Blue);
			Colors.Add(System.Drawing.Color.Purple);
			Colors.Add(System.Drawing.Color.YellowGreen);
			Colors.Add(System.Drawing.Color.Pink);
			Colors.Add(System.Drawing.Color.Orange);
			Colors.Add(System.Drawing.Color.Gray);			
		}

		private void btnCreate_Click(object sender, RoutedEventArgs e) {

		}

		private void drenClick_Click(object sender, RoutedEventArgs e) {
			int splitPower = Int32.Parse(txtDNSplitPower.Text);
			createPumpRunChart(z1, PumpTypeEnum.Drenage, chbDNSplitPower.IsChecked.Value, splitPower);
			PumpDiagnostics report = new PumpDiagnostics(clndFrom.SelectedDate.Value, clndTo.SelectedDate.Value);
			Dictionary<DateTime, SvodDataRecord> Data = report.ReadPumpSvod("P_Avg", -1, 200);
			Dictionary<DateTime, double> DataRun = report.ReadGGRun();
			createPumpPuskChart(z2, PumpTypeEnum.Drenage,Data,DataRun, true);
			createPumpPuskChart(z3, PumpTypeEnum.Drenage, Data,DataRun,  false);

		}

		public void prepareChart(ZedGraphControl chart) {
			chart.GraphPane.CurveList.Clear();
			chart.GraphPane.XAxis.Type = AxisType.Date;
			chart.GraphPane.XAxis.Scale.Format = "dd.MM";
			chart.GraphPane.XAxis.Title.IsVisible = false;
			chart.GraphPane.YAxis.Title.IsVisible = true;
			chart.GraphPane.YAxis.Title.FontSpec.Size = 10;
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
				while (power  <= 115) {
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
					Dictionary<DateTime, double> DataForApprox = new Dictionary<DateTime, double>();
					System.Drawing.Color color = Colors[ind++ % 7];
					PointPairList points = new PointPairList();
					foreach (KeyValuePair<DateTime, PumpDataRecord> de in Data) {
						points.Add(new PointPair(new XDate(de.Key), de.Value.RunTime));
						DataForApprox.Add(de.Key, de.Value.RunTime);
					}
					LineItem line = chart.GraphPane.AddCurve("", points, color, SymbolType.Circle);
					line.Line.IsVisible = false;
					line.Symbol.Fill = new Fill(color);
					line.Symbol.Size = 1.5F;					

					Dictionary<DateTime, double> appr = report.Approx(DataForApprox);
					points = new PointPairList();
					foreach (KeyValuePair<DateTime, double> de in appr) {
						points.Add(new PointPair(new XDate(de.Key), de.Value));
					}

					line = chart.GraphPane.AddCurve(header, points, color, SymbolType.None);
					line.Line.IsVisible = true;
					line.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;

				}
			}
			chart.AxisChange();
			chart.Invalidate();
		}

		public void createPumpPuskChart(ZedGraphControl chart, PumpTypeEnum type, Dictionary<DateTime, SvodDataRecord> Data, Dictionary<DateTime,double> DataRun, bool time) {
			int ind = 0;
			prepareChart(chart);
			List<int> powers = new List<int>();

			if (Data.Count > 0) {
				System.Drawing.Color color = Colors[0];
				PointPairList points = new PointPairList();
				foreach (KeyValuePair<DateTime, SvodDataRecord> de in Data) {
					double val = 0;
					switch (type) {
						case PumpTypeEnum.Drenage:
							val = time ? de.Value.DN1Time + de.Value.DN2Time : de.Value.DN1Pusk + de.Value.DN2Pusk;
							break;
						case PumpTypeEnum.Leakage:
							val = time ? de.Value.LN1Time + de.Value.LN2Time : de.Value.LN1Pusk + de.Value.LN2Pusk;
							break;
						case PumpTypeEnum.MNU:
							val = time ? de.Value.MNU1Time + de.Value.MNU2Time + de.Value.MNU3Time : de.Value.MNU1Pusk + de.Value.MNU2Pusk + de.Value.MNU3Pusk;
							break;
					}
					points.Add(new PointPair(new XDate(de.Key), val));


				}
				LineItem line = chart.GraphPane.AddCurve(String.Format("{0}", time ? "Работа (ceк)" : "Пусков"), points, color, SymbolType.Circle);
				line.Line.IsVisible = true;
				line.Symbol.Size = 1;
				line.Symbol.Fill = new Fill(color);
			}

			if (DataRun.Count > 0) {
				System.Drawing.Color color = Colors[1];
				PointPairList points = new PointPairList();
				foreach (KeyValuePair<DateTime, double> de in DataRun) {
					if (Data.ContainsKey(de.Key)) {
						points.Add(new PointPair(new XDate(de.Key), de.Value));
					}

				}
				LineItem line = chart.GraphPane.AddCurve(String.Format("Работа ГГ"), points, color, SymbolType.Circle);
				line.Line.IsVisible = true;
				line.Symbol.Size = 1;
				line.Symbol.Fill = new Fill(color);
				//chart.GraphPane.YAxisList.Add("ГГ");
				line.IsY2Axis = true;
			}

			chart.AxisChange();
			chart.Invalidate();
		}

		private void mnuClick_Click(object sender, RoutedEventArgs e) {
			int splitPower = Int32.Parse(txtMNUSplitPower.Text);
			createPumpRunChart(z4, PumpTypeEnum.MNU, chbMNUSplitPower.IsChecked.Value, splitPower);
			PumpDiagnostics report = new PumpDiagnostics(clndFrom.SelectedDate.Value, clndTo.SelectedDate.Value);
			Dictionary<DateTime, SvodDataRecord> Data = report.ReadPumpSvod("P_Avg", -1, 200);
			Dictionary<DateTime, double> DataRun = report.ReadGGRun();
			createPumpPuskChart(z5, PumpTypeEnum.MNU,Data,DataRun, true);
			createPumpPuskChart(z6, PumpTypeEnum.MNU, Data,DataRun, false);
			createPumpPuskChart(z51, PumpTypeEnum.Leakage, Data,DataRun, true);
			createPumpPuskChart(z61, PumpTypeEnum.Leakage, Data,DataRun, false);
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
				string header = "";
				if (!split) {
					headerRun = "Уровень масла (ГГ в работе)";
					headerStop = "Уровень масла (ГГ стоит)";
					header = "Уровень масла";
				} else {
					headerRun = String.Format("Уровень при t {0:0.0}-{1:0.0} (ГГ в работе)", t, t + step);
					headerStop = String.Format("Уровень при t {0:0.0}-{1:0.0} (стоит)", t, t + step);
					header = String.Format("Уровень при t {0:0.0}-{1:0.0}", t, t + step);
				}

				PointPairList pointsRun = new PointPairList();
				PointPairList pointsStop = new PointPairList();
				Dictionary<DateTime, double> RunForApprox = new Dictionary<DateTime, double>();
				Dictionary<DateTime, double> StopForApprox = new Dictionary<DateTime, double>();

				Data = report.ReadSvod(obj, t, t + step);
				foreach (KeyValuePair<DateTime, SvodDataRecord> de in Data) {
					if (de.Value.PAvg < 1) {
						if (gp) {
							pointsStop.Add(new PointPair(new XDate(de.Key), de.Value.GPLevel));
							StopForApprox.Add(de.Key, de.Value.GPLevel);
						} else {
							pointsStop.Add(new PointPair(new XDate(de.Key), de.Value.PPLevel));
							StopForApprox.Add(de.Key, de.Value.PPLevel);
						}
					} else {
						if (gp) {
							pointsRun.Add(new PointPair(new XDate(de.Key), de.Value.GPLevel));
							RunForApprox.Add(de.Key, de.Value.GPLevel);
						} else {
							pointsRun.Add(new PointPair(new XDate(de.Key), de.Value.PPLevel));
							RunForApprox.Add(de.Key, de.Value.PPLevel);
						}
					}
				}
				System.Drawing.Color color = Colors[ind++ % 7];
				if (pointsRun.Count > 5) {
					LineItem line = chart.GraphPane.AddCurve("", pointsRun, color, SymbolType.Diamond);
					line.Line.IsVisible = false;
					line.Symbol.Size = 1.5f;
					line.Symbol.Fill = new Fill(color);
					
					pointsRun = new PointPairList();
					Dictionary<DateTime, double> appr = report.Approx(RunForApprox);
					foreach (KeyValuePair<DateTime, double> de in appr) {
						pointsRun.Add(new PointPair(new XDate(de.Key), de.Value));
					}
					LineItem ln = chart.GraphPane.AddCurve(headerRun, pointsRun, color, SymbolType.None);
					ln.Line.IsVisible = true;
					ln.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;

				}
				//line.Line.IsVisible = false;
				if (pointsStop.Count > 5) {
					LineItem line = chart.GraphPane.AddCurve("", pointsStop, color, SymbolType.Circle);
					line.Line.IsVisible = false;
					line.Symbol.Size = 1.5f;
					line.Symbol.Fill = new Fill(color);

					pointsStop = new PointPairList();
					Dictionary<DateTime, double> appr = report.Approx(StopForApprox);
					foreach (KeyValuePair<DateTime, double> de in appr) {
						pointsStop.Add(new PointPair(new XDate(de.Key), de.Value));
					}
					LineItem ln = chart.GraphPane.AddCurve(headerStop, pointsStop, color, SymbolType.None);
					ln.Line.IsVisible = true;
					ln.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;
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
