using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
	public class ChartZedSerie : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public void NotifyChanged(string propName) {
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propName));
		}

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
		public static int indexColor = 0;

		public static System.Drawing.Color NextColor() {
			return Colors[indexColor++ % Colors.Count];
		}
		public string Header { get; set; }
		public LineItem Item { get; set; }

		private bool _isVisible;
		public bool IsVisible {
			get => _isVisible; set {
				_isVisible = value;
				NotifyChanged("IsVisible");
			}
		}
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
	public partial class ChartZedControl : UserControl, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public void NotifyChanged(string propName) {
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propName));
		}

		public ObservableCollection<ChartZedSerie> ObsSeries;
		public Dictionary<string, ChartZedSerie> Series;
		private DateTime _minDate;
		private DateTime _maxDate;

		public DateTime MinDate { get => _minDate; set { _minDate = value; NotifyChanged("MinDate"); } }
		public DateTime MaxDate { get => _maxDate; set { _maxDate = value; NotifyChanged("MinDate"); } }
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
			chart.GraphPane.Legend.IsVisible = false;
			chart.IsZoomOnMouseCenter = false;
			ChartZedSerie.indexColor = 0;

		}


		public void refreshDates() {
			DateTime min = DateTime.MaxValue;
			DateTime max = DateTime.MinValue;
			foreach (ChartZedSerie ser in ObsSeries) {
				DateTime minD = ser.Data.Keys.Min();
				DateTime maxD = ser.Data.Keys.Max();
				min = min > minD ? minD : min;
				max = max < maxD ? maxD : max;
			}
			chart.GraphPane.XAxis.Scale.Min = XDate.DateTimeToXLDate(min);
			chart.GraphPane.XAxis.Scale.Max = XDate.DateTimeToXLDate(max);
		}

		public void AddSerie(String header, Dictionary<DateTime, double> values, System.Drawing.Color color, bool line, bool symbol,int y2axisIndex=-1) {
			PointPairList points = new PointPairList();
			foreach (KeyValuePair<DateTime, double> de in values) {
				points.Add(new PointPair(new XDate(de.Key), de.Value));
			}
			ChartZedSerie serie = new ChartZedSerie();
			serie.Header = header;
			serie.Data = values;
			serie.Color = color;
			serie.IsVisible = true;
			LineItem lineItem = chart.GraphPane.AddCurve(header, points, color, symbol ? SymbolType.Circle : SymbolType.None);
			serie.Item = lineItem;

			lineItem.Line.IsVisible = line;
			if (symbol) {
				lineItem.Symbol.Size = 1.5f;
				lineItem.Symbol.Fill = new Fill(color);
			}
			ObsSeries.Add(serie);

			if (y2axisIndex > -1) {
				while (chart.GraphPane.Y2AxisList.Count()< y2axisIndex + 1) {
					chart.GraphPane.Y2AxisList.Add(new Y2Axis());
				}
				lineItem.IsY2Axis = true;
				lineItem.YAxisIndex = y2axisIndex;
			}
			refreshDates();
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

		private void SetAll(bool visible) {
			foreach (ChartZedSerie ser in ObsSeries) {
				ser.Item.IsVisible = visible;
				ser.IsVisible = visible;
				chart.AxisChange();
				chart.Invalidate();
			}
		}

		private void btnSelectAll_Click(object sender, RoutedEventArgs e) {
			SetAll(true);
		}

		private void btnDeselectAll_Click(object sender, RoutedEventArgs e) {
			SetAll(false);
		}
	}
}
