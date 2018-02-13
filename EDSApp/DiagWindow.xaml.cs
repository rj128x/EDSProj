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
	/// Логика взаимодействия для DiagWindow.xaml
	/// </summary>
	public partial class DiagWindow : Window
	{
		public DiagWindow() {
			InitializeComponent();
			clndFrom.SelectedDate = DateTime.Now.Date.AddMonths(-1);
			clndTo.SelectedDate = DateTime.Now.Date;

		}

		private void btnCreate_Click(object sender, RoutedEventArgs e) {

		}
	}
}
