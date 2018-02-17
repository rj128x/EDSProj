using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZedGraph;

namespace EDSApp
{
	public class ChartZedSerie
	{
		public static List<System.Drawing.Color> Colors;
		static ChartZedSerie() {
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
		public string Header { get; set; }
		public LineItem Item { get; set; }
		public bool IsVisible { get; set; }
		protected System.Drawing.Color _color;
		public System.Drawing.Color Color {
			get {
				return _color;
			}
			set {
				_color = value;
				FillBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(_color.A, _color.R, _color.G, _color.B));
			}
		}

		public Brush FillBrush { get; set; }
		public Dictionary<DateTime, double> Data { get; set; }
	}
	/// <summary>
	/// Логика взаимодействия для ChartZedControl.xaml
	/// </summary>
	public partial class ChartZedControl : UserControl
	{
		public ObservableCollection<ChartZedSerie> ObsSeries;
		public Dictionary<string, ChartZedSerie> Series;
		public ChartZedControl() {
			ObsSeries = new ObservableCollection<ChartZedSerie>();
			InitializeComponent();
		}
		public void init() {
			ObsSeries = new ObservableCollection<ChartZedSerie>();
			grdLegend.ItemsSource = ObsSeries;
			chart.GraphPane.CurveList.Clear();
			chart.GraphPane.XAxis.Type = AxisType.Date;
			chart.GraphPane.XAxis.Scale.Format = "dd.MM";
			chart.GraphPane.XAxis.Title.IsVisible = false;
			chart.GraphPane.YAxis.Title.IsVisible = true;
			chart.GraphPane.YAxis.Title.FontSpec.Size = 10;
			chart.GraphPane.Title.IsVisible = false;
			chart.GraphPane.Legend.IsVisible = true;


		}

		public void AddSerie(String header,Dictionary<DateTime,double> values, System.Drawing.Color color,bool line,bool symbol) {
			PointPairList points = new PointPairList();
			foreach (KeyValuePair<DateTime,double> de in values) {
				points.Add(new PointPair(new XDate(de.Key), de.Value));
			}
			ChartZedSerie serie = new ChartZedSerie();
			serie.Header = header;
			serie.Data = values;
			serie.Color = color;
			serie.IsVisible = true;
			LineItem lineItem=chart.GraphPane.AddCurve(header, points, color, symbol ? SymbolType.Circle : SymbolType.None);
			serie.Item = lineItem;

			lineItem.Line.IsVisible = line;
			if (symbol) {
				lineItem.Symbol.Size = 1.5f;
				lineItem.Symbol.Fill = new Fill(color);
			}
			ObsSeries.Add(serie);
			

			chart.AxisChange();
			chart.Invalidate();
		}



		private void CheckBox_Click(object sender, RoutedEventArgs e) {
			try {
				CheckBox chb = sender as CheckBox;
				ChartZedSerie ser = grdLegend.SelectedItem as ChartZedSerie;
				ser.Item.IsVisible = chb.IsChecked.Value;
				ser.IsVisible = chb.IsChecked.Value;
				chart.AxisChange();
				chart.Invalidate();
			} catch { }
		}
	}
}
