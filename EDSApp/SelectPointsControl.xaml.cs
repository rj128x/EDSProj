using EDSProj;
using EDSProj.EDS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EDSApp
{
	/// <summary>
	/// Логика взаимодействия для SelectPointsControl.xaml
	/// </summary>
	public partial class SelectPointsControl : UserControl
	{
		public List<EDSReportRequestRecord> SelectedPoints;
		public List<EDSPointInfo> FilteredPoints;
		public List<TechGroupInfo> TechGroups;
		public SelectPointsControl() {
			InitializeComponent();
			try {
				cmbFunction.ItemsSource = EDSClass.ReportFunctions;
				cmbFunction.SelectedValue = EDSReportFunction.avg;
			} catch { }
		}

		public void init() {
			SelectedPoints = new List<EDSReportRequestRecord>();
			FilteredPoints = EDSPointsClass.AllAnalogPoints.Values.ToList();
			TechGroups = EDSClass.TechGroups.Values.ToList();
			//lbPoints.ItemsSource = FilteredPoints;
			grdPoints.ItemsSource = FilteredPoints;
			lbSelPoints.ItemsSource = SelectedPoints;
			lbGroups.ItemsSource = TechGroups;
			RefreshSelection("");
		}

		public void RefreshSelection(string text) {
			FilteredPoints.Clear();
			Regex regex = new Regex(text);
			foreach (EDSPointInfo point in EDSPointsClass.AllAnalogPoints.Values) {
				foreach (TechGroupInfo tg in TechGroups) {
					if (FilteredPoints.Contains(point))
						continue;
					if (tg.Selected) {
						if (point.Groups.Contains(tg.Id)) {
							if (regex.Match(point.FullName).Success)
								FilteredPoints.Add(point);
						}
					}
				}
			}
			//lbPoints.Items.Refresh();
			grdPoints.Items.Refresh();
		}

		private void lbPoints_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			/*try {
				EDSPointInfo selPoint = lbPoints.SelectedItem as EDSPointInfo;
				EDSReportFunction func = (EDSReportFunction)cmbFunction.SelectedValue;
				EDSReportRequestRecord rec = new EDSReportRequestRecord(selPoint,func);

				bool exists=SelectedPoints.Exists(req => req.Id == rec.Id);
				if (!exists) {
					SelectedPoints.Add(rec);
					lbSelPoints.Items.Refresh();
				}
			} catch { }*/

		}

		private void lbSelPoints_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			try {
				EDSReportRequestRecord rec = lbSelPoints.SelectedItem as EDSReportRequestRecord;
				if (SelectedPoints.Contains(rec)) {
					SelectedPoints.Remove(rec);
					lbSelPoints.Items.Refresh();
				}
			} catch { }
		}

		private void txtFilter_KeyUp(object sender, KeyEventArgs e) {
			try {
				RefreshSelection(txtFilter.Text);
			} catch { }
		}

		private void CheckBox_Checked(object sender, RoutedEventArgs e) {
			RefreshSelection(txtFilter.Text);
		}

		private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
			RefreshSelection(txtFilter.Text);
		}

		private void grdPoints_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			try {
				EDSPointInfo selPoint = grdPoints.SelectedItem as EDSPointInfo;
				EDSReportFunction func = (EDSReportFunction)cmbFunction.SelectedValue;
				EDSReportRequestRecord rec = new EDSReportRequestRecord(selPoint, func);

				bool exists = SelectedPoints.Exists(req => req.Id == rec.Id);
				if (!exists) {
					SelectedPoints.Add(rec);
					lbSelPoints.Items.Refresh();
				}
			} catch { }
		}
	}
}
