using EDSProj;
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
	/// Логика взаимодействия для WaterWin.xaml
	/// </summary>
	public partial class WaterWindow : Window
	{
		public WaterWindow() {
			InitializeComponent();
		}

		protected static double getDouble(string str) {
			try {
				return double.Parse(str.Replace(".", ","));
			} catch {
				try {
					return double.Parse(str.Replace(",", "."));
				} catch {
					return 0;
				}
			}
		}

		private void btnCalcGGP_Click(object sender, RoutedEventArgs e) {
			double q = getDouble(txtQGG.Text);
			double h = getDouble(txtHGG.Text);
			int gg = int.Parse(txtGG.Text);
			double p = InerpolationRashod.getPower(gg, q, h);
			txtPGG.Text = p.ToString("0.00");

		}

		private void btnCalcGGQ_Click(object sender, RoutedEventArgs e) {
			double p = getDouble(txtPGG.Text);
			double h = getDouble(txtHGG.Text);
			int gg = int.Parse(txtGG.Text);
			double q = InerpolationRashod.getRashodGA(gg, p, h);
			txtQGG.Text = q.ToString("0.00");
			
		}

		private void btnCalcGESP_Click(object sender, RoutedEventArgs e) {
			double q = getDouble(txtQGES.Text);
			double h = getDouble(txtHGES.Text);			
			double p = InerpolationRashod.getPowerGES(q, h);
			txtPGES.Text = p.ToString("0.00");
		}

		private void btnCalcGESQ_Click(object sender, RoutedEventArgs e) {
			double p = getDouble(txtPGES.Text);
			double h = getDouble(txtHGES.Text);
			double q = OptimRashod.getOptimRashod(p,h);
			txtQGES.Text = q.ToString("0.00");
		}

		private void btnRUSA_Click(object sender, RoutedEventArgs e) {
			double p = getDouble(txtPGES_RUSA.Text);
			double h = getDouble(txtHGES_RUSA.Text);
			String txt = string.Format(@"<html>
				<head>
					<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
            </head>
				<table border='1'><tr><th>Кол</th><th>Расход</th><th>КПД</th><th>Мощность</th><th>Состав</th></tr>");

			SortedList<double,List<int>> result= OptimRashod.getOptimRashodFull(p, h);
			foreach (KeyValuePair<double,List<int>> de in result) {
				double kpd = InerpolationRashod.KPD(p, h, de.Key)*100;
				double divP = p / de.Value.Count;				
				string tr = String.Format("<tr><td>{0}</td><td>{1:0.00}</td><td>{2:0.00}</td><td>{3:0.00}</td><td>{4}</td></tr>", de.Value.Count, de.Key, kpd, divP, string.Join(" ", de.Value));
				txt += tr;
			}
			txt += "</table>";
			wbResult.NavigateToString(txt);
		}
	}
}
