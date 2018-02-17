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
		public DiagWindow() {
			InitializeComponent();
			clndFrom.SelectedDate = DateTime.Now.Date.AddMonths(-3);
			clndTo.SelectedDate = DateTime.Now.Date;

		}

		private void btnCreate_Click(object sender, RoutedEventArgs e) {

		}

		private void drenClick_Click(object sender, RoutedEventArgs e) {
			int splitPower = Int32.Parse(txtDNSplitPower.Text);
			PumpDiagnostics report = new PumpDiagnostics(clndFrom.SelectedDate.Value, clndTo.SelectedDate.Value);
			Dictionary<DateTime, SvodDataRecord> Data = report.ReadPumpSvod("P_Avg", -1, 200);
			Dictionary<DateTime, double> DataRun = report.ReadGGRun();


			createPumpRunChart(DNTimeWork, PumpTypeEnum.Drenage, chbDNSplitPower.IsChecked.Value, splitPower);
			createPumpPuskChart(DNTimeDay, PumpTypeEnum.Drenage, Data, DataRun, true);
			createPumpPuskChart(DNPuskDay, PumpTypeEnum.Drenage, Data, DataRun, false);
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



		public void createPumpRunChart(ChartZedControl chart, PumpTypeEnum type, bool split, int splitPower) {
			int ind = 0;
			PumpDiagnostics report = new PumpDiagnostics(clndFrom.SelectedDate.Value, clndTo.SelectedDate.Value);
			Dictionary<DateTime, PumpDataRecord> Data = new Dictionary<DateTime, PumpDataRecord>();

			chart.init();

			List<int> powers = new List<int>();
			if (split) {
				powers.Add(-1);
				int power = 35;
				while (power <= 115) {
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


				if (Data.Count > 10) {

					System.Drawing.Color color = ChartZedSerie.NextColor();

					Dictionary<DateTime, double> data = new Dictionary<DateTime, double>();
					foreach (KeyValuePair<DateTime, PumpDataRecord> de in Data) {
						data.Add(de.Key, de.Value.RunTime);
					}

					chart.AddSerie(header, data, color, false, true);

					Dictionary<DateTime, double> appr = report.Approx(data);

					chart.AddSerie(header, appr, color, true, false);
				}
			}
		}



		

		public void createPumpPuskChart(ChartZedControl chart, PumpTypeEnum type, Dictionary<DateTime, SvodDataRecord> Data, Dictionary<DateTime, double> DataRun, bool time) {
			int ind = 0;
			chart.init();
			List<int> powers = new List<int>();

			if (Data.Count > 0) {
				Dictionary<DateTime, double> data = new Dictionary<DateTime, double>();
				System.Drawing.Color color = ChartZedSerie.NextColor();
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
					data.Add(de.Key, val);


				}
				chart.AddSerie(String.Format("{0}", time ? "Работа (ceк)" : "Пусков"), data, color, true, true);

			}

			if (DataRun.Count > 0) {
				System.Drawing.Color color = ChartZedSerie.NextColor();
				chart.AddSerie(String.Format("Работа ГГ"), DataRun, color, true, true);

			}

		}

		private void mnuClick_Click(object sender, RoutedEventArgs e) {
			int splitPower = Int32.Parse(txtMNUSplitPower.Text);
			createPumpRunChart(MNUTimeWork, PumpTypeEnum.MNU, chbMNUSplitPower.IsChecked.Value, splitPower);
			PumpDiagnostics report = new PumpDiagnostics(clndFrom.SelectedDate.Value, clndTo.SelectedDate.Value);
			Dictionary<DateTime, SvodDataRecord> Data = report.ReadPumpSvod("P_Avg", -1, 200);
			Dictionary<DateTime, double> DataRun = report.ReadGGRun();
			createPumpPuskChart(MNUTimeDay, PumpTypeEnum.MNU, Data, DataRun, true);
			createPumpPuskChart(MNUPuskDay, PumpTypeEnum.MNU, Data, DataRun, false);
			createPumpPuskChart(LNTimeDay, PumpTypeEnum.Leakage, Data, DataRun, true);
			createPumpPuskChart(LNPuskDay, PumpTypeEnum.Leakage, Data, DataRun, false);
		}

		public void createOilChart(ChartZedControl chart, bool gp, bool splitHot, bool splitCold, double step) {
			int ind = 0;
			PumpDiagnostics report = new PumpDiagnostics(clndFrom.SelectedDate.Value, clndTo.SelectedDate.Value);
			Dictionary<DateTime, SvodDataRecord> Data = new Dictionary<DateTime, SvodDataRecord>();

			chart.init();

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


				Dictionary<DateTime, double> RunForApprox = new Dictionary<DateTime, double>();
				Dictionary<DateTime, double> StopForApprox = new Dictionary<DateTime, double>();

				Data = report.ReadSvod(obj, t, t + step);
				foreach (KeyValuePair<DateTime, SvodDataRecord> de in Data) {
					if (de.Value.PAvg ==0) {
						if (gp) {
							StopForApprox.Add(de.Key, de.Value.GPLevel);
						} else {
							StopForApprox.Add(de.Key, de.Value.PPLevel);
						}
					} else {
						if (gp) {
							RunForApprox.Add(de.Key, de.Value.GPLevel);
						} else {
							RunForApprox.Add(de.Key, de.Value.PPLevel);
						}
					}
				}
				System.Drawing.Color color = ChartZedSerie.NextColor();
				if (RunForApprox.Count > 10) {
					chart.AddSerie(headerRun, RunForApprox, color, false, true);
					
					Dictionary<DateTime, double> appr = report.Approx(RunForApprox);
					chart.AddSerie(headerRun, appr, color, true, false);					

				}
				//line.Line.IsVisible = false;
				if (StopForApprox.Count > 10) {
					chart.AddSerie(headerStop, StopForApprox, color, false, true);

					Dictionary<DateTime, double> appr = report.Approx(StopForApprox);
					chart.AddSerie(headerStop, appr, color, true, false);
					
				}


				//line.Line.IsVisible = false;
			}

		}

		private void oilClick_Click(object sender, RoutedEventArgs e) {
			bool splitHot = chbOilSplitHot.IsChecked.Value;
			bool splitCold = chbOilSplitCold.IsChecked.Value;
			double step = Double.Parse(txtOilSplitTemp.Text);
			createOilChart(GPLevel, true, splitHot, splitCold, step);
			createOilChart(PPLevel, false, splitHot, splitCold, step);
		}


	}
}
