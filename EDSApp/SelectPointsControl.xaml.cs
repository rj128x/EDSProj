using EDSProj;
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
		public List<EDSPointInfo> SelectedPoints;
		public List<EDSPointInfo> FilteredPoints;
		public SelectPointsControl() {
			InitializeComponent();
			
		}

		public void init() {
			SelectedPoints = new List<EDSPointInfo>();
			FilteredPoints = EDSPointsClass.AllAnalogPoints.Values.ToList();
			lbPoints.ItemsSource = FilteredPoints;
			lbSelPoints.ItemsSource = SelectedPoints;
		}

		private void lbPoints_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			try {
				EDSPointInfo selPoint = lbPoints.SelectedItem as EDSPointInfo;
				if (!SelectedPoints.Contains(selPoint)) {
					SelectedPoints.Add(selPoint);
					lbSelPoints.Items.Refresh();
				}
			} catch { }

		}

		private void lbSelPoints_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			try {
				EDSPointInfo selPoint = lbSelPoints.SelectedItem as EDSPointInfo;
				if (SelectedPoints.Contains(selPoint)) {
					SelectedPoints.Remove(selPoint);
					lbSelPoints.Items.Refresh();
				}
			} catch { }
		}

		private void txtFilter_KeyUp(object sender, KeyEventArgs e) {
			try {
				string str = txtFilter.Text;
				FilteredPoints.Clear();
				Regex regex = new Regex(str);
				foreach (EDSPointInfo point in EDSPointsClass.AllAnalogPoints.Values) {
					if (regex.Match(point.FullName).Success)
						FilteredPoints.Add(point);
				}
				lbPoints.Items.Refresh();
			} catch { }
		}
	}
}
