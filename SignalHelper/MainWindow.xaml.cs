using Microsoft.Win32;
using ScottPlot;
using ScottPlot.Plottable;
using SignalHelper.Classes;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SignalHelper;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {

    private FinancePlot? _highlightedPoint;

    private ScatterPlot? _indicator1p1;
    private ScatterPlot? _indicator1p2;

    private ScatterPlot? _indicator2p1;
    private ScatterPlot? _indicator2p2;

    private ScatterPlot? _buySignal;
    private ScatterPlot? _sellSignal;

    private readonly ContextMenu _rightClickMenu;
    private Runner? _runner;

    private const int _refreshRateMs = 50;
    private readonly Stopwatch _refreshRateCounter = new();

    private double _signalXCoordinate;

    private readonly MenuItem _openSignalMenuItem = new() { Header = "Open signal" };
    private readonly MenuItem _attachSignalMenuItem = new() { Header = "Attach additional signal", IsEnabled = false };
    private readonly MenuItem _exportSignalMenuItem = new() { Header = "Export to CSV", IsEnabled = false };
    private readonly MenuItem _buyPositionMenuItem = new() { Header = "Buy", IsEnabled = false };
    private readonly MenuItem _sellPositionMenuItem = new() { Header = "Sell", IsEnabled = false };
    private readonly MenuItem _returnViewMenuItem = new() { Header = "Reset View", IsEnabled = false };
    private readonly MenuItem _closeSignalMenuItem = new() { Header = "Close opened", IsEnabled = false };

    private double _minX;
    private double _maxX;

    public MainWindow() {
        InitializeComponent();

        SignalPlot.RightClicked -= SignalPlot.DefaultRightClickEvent;
        IndicatorSignal1.RightClicked -= IndicatorSignal1.DefaultRightClickEvent;
        IndicatorSignal2.RightClicked -= IndicatorSignal2.DefaultRightClickEvent;

        _openSignalMenuItem.Click += OpenSignalEvent;
        _attachSignalMenuItem.Click += AttachSignalEvent;
        _buyPositionMenuItem.Click += SetBuyPosition;
        _sellPositionMenuItem.Click += SetSellPosition;
        _returnViewMenuItem.Click += ReturnView;
        _closeSignalMenuItem.Click += CloseSignalEvent;
        _exportSignalMenuItem.Click += ExportToCsv;

        _rightClickMenu = new ContextMenu();
        _rightClickMenu.Items.Add(_buyPositionMenuItem);
        _rightClickMenu.Items.Add(_sellPositionMenuItem);
        _rightClickMenu.Items.Add(new Separator());
        _rightClickMenu.Items.Add(_openSignalMenuItem);
        _rightClickMenu.Items.Add(_closeSignalMenuItem); 
        _rightClickMenu.Items.Add(_attachSignalMenuItem);
        _rightClickMenu.Items.Add(_exportSignalMenuItem);
        _rightClickMenu.Items.Add(new Separator());
        _rightClickMenu.Items.Add(_returnViewMenuItem);
        
        SignalPlot.RightClicked += AddCustomContextMenuEvent;
        
        SignalPlot.Refresh();
    }

    private void AddCustomContextMenuEvent(object? sender, EventArgs e) {
        if (_runner != null)
            _runner.SetClickedIndex(SignalPlot.GetMouseCoordinates().x);

        _rightClickMenu.IsOpen = true;
    } 

    private void OpenSignalEvent(object? sender, EventArgs e) {
        var ofd = new OpenFileDialog() {
            Filter = "Comma separated values (*.csv)|*.csv",
            Multiselect = false
        };

        ofd.ShowDialog();

        if (!ofd.FileNames.Any())
            return;

        _runner = new Runner(ofd.FileName);
        SignalPlot.Reset();
        IndicatorSignal1.Reset();

        SignalPlot.Plot.Title(_runner.SignalName);
        IndicatorSignal1.Plot.Title($"{_runner.SignalName}: RSi/MFi");
        SignalPlot.Plot.AddCandlesticks(_runner.Plot.ToArray());

        _indicator1p1 = IndicatorSignal1.Plot.AddScatterLines(xs: _runner.Signal.Select(m => m.Date.ToOADate()).ToArray(), ys: _runner.Signal.Select(m => m.Rsi).ToArray());
        _indicator1p1.MarkerShape = MarkerShape.filledSquare;
        _indicator1p1.MarkerSize = 5;
        _indicator1p1.Label = "RSi";

        _indicator1p2 = IndicatorSignal1.Plot.AddScatterLines(xs: _runner.Signal.Select(m => m.Date.ToOADate()).ToArray(), ys: _runner.Signal.Select(m => m.Mfi).ToArray());
        _indicator1p2.MarkerShape = MarkerShape.filledDiamond;
        _indicator1p2.MarkerSize = 5;
        _indicator1p2.Label = "MFi";

        SignalPlot.Plot.XAxis.DateTimeFormat(true);
        IndicatorSignal1.Plot.XAxis.DateTimeFormat(true);

        _minX = _runner.Signal.Min(m => m.Date.ToOADate());
        _maxX = _runner.Signal.Max(m => m.Date.ToOADate());

        SignalPlot.Plot.SetOuterViewLimits(yMin: _runner.Signal.Min(m => m.Low), yMax: _runner.Signal.Max(m => m.High), xMin: _minX, xMax: _maxX);

        IndicatorSignal1.Plot.SetOuterViewLimits(yMin: 0, yMax: 1, xMin: _minX, xMax: _maxX);

        SignalPlot.Render();
        IndicatorSignal1.Render();

        _highlightedPoint = SignalPlot.Plot.AddCandlesticks(_runner.SelectedCandle);
        _highlightedPoint.WickColor = System.Drawing.Color.Black;

        _refreshRateCounter.Start();

        _openSignalMenuItem.IsEnabled = false;
        _attachSignalMenuItem.IsEnabled = true;
        _exportSignalMenuItem.IsEnabled = true;
        _buyPositionMenuItem.IsEnabled = true;
        _sellPositionMenuItem.IsEnabled = true;
        _returnViewMenuItem.IsEnabled = true;
        _closeSignalMenuItem.IsEnabled = true;
    }

    private void AttachSignalEvent(object sender, EventArgs e) {
        if (_runner == null) 
            return;

        var ofd = new OpenFileDialog() {
            Filter = "Comma separated values (*.csv)|*.csv",
            Multiselect = false
        };

        ofd.ShowDialog();

        if (!ofd.FileNames.Any())
            return;

        _runner.AttachSignal(ofd.FileName);
        IndicatorSignal2.Plot.Title($"{_runner.AttachedSignalName}: RSi/MFi");

        _indicator2p1 = IndicatorSignal2.Plot.AddScatterLines(xs: _runner.Signal.Select(m => m.Date.ToOADate()).ToArray(), ys: _runner.Signal.Select(m => m.AttachedRsi).ToArray());
        _indicator2p1.MarkerSize = 5;
        _indicator2p1.Label = "RSi";

        _indicator2p2 = IndicatorSignal2.Plot.AddScatterLines(xs: _runner.Signal.Select(m => m.Date.ToOADate()).ToArray(), ys: _runner.Signal.Select(m => m.AttachedMfi).ToArray());
        _indicator2p2.MarkerSize = 5;
        _indicator2p2.Label = "MFi";

        IndicatorSignal2.Plot.XAxis.DateTimeFormat(true);
        IndicatorSignal2.Plot.SetOuterViewLimits(yMin: 0, yMax: 1, xMin: _minX, xMax: _maxX);
        IndicatorSignal2.Plot.SetAxisLimits(yMin: 0, yMax: 1, xMin: _minX, xMax: _maxX);

        IndicatorSignal2.Render();

        _attachSignalMenuItem.IsEnabled = false;
    }

    private void AxesChangedSignal(object sender, EventArgs e) {
        var changedPlotAxesLimits = SignalPlot.Plot.GetAxisLimits();

        IndicatorSignal1.Configuration.AxesChangedEventEnabled = false;
        IndicatorSignal2.Configuration.AxesChangedEventEnabled = false;

        IndicatorSignal1.Plot.SetAxisLimitsX(changedPlotAxesLimits.XMin, changedPlotAxesLimits.XMax);
        IndicatorSignal2.Plot.SetAxisLimitsX(changedPlotAxesLimits.XMin, changedPlotAxesLimits.XMax);

        IndicatorSignal1.Refresh();
        IndicatorSignal2.Refresh();

        IndicatorSignal1.Configuration.AxesChangedEventEnabled = true;
        IndicatorSignal2.Configuration.AxesChangedEventEnabled = true;
    }

    private void AxesChangedIndicator1(object sender, EventArgs e) {
        var changedPlotAxesLimits = IndicatorSignal1.Plot.GetAxisLimits();

        SignalPlot.Configuration.AxesChangedEventEnabled = false;
        IndicatorSignal2.Configuration.AxesChangedEventEnabled = false;

        SignalPlot.Plot.SetAxisLimitsX(changedPlotAxesLimits.XMin, changedPlotAxesLimits.XMax);
        IndicatorSignal2.Plot.SetAxisLimitsX(changedPlotAxesLimits.XMin, changedPlotAxesLimits.XMax);

        SignalPlot.Refresh();
        IndicatorSignal2.Refresh();

        SignalPlot.Configuration.AxesChangedEventEnabled = true;
        IndicatorSignal2.Configuration.AxesChangedEventEnabled = true;
    }

    private void AxesChangedIndicator2(object sender, EventArgs e) {
        var changedPlotAxesLimits = IndicatorSignal2.Plot.GetAxisLimits();

        SignalPlot.Configuration.AxesChangedEventEnabled = false;
        IndicatorSignal1.Configuration.AxesChangedEventEnabled = false;

        SignalPlot.Plot.SetAxisLimitsX(changedPlotAxesLimits.XMin, changedPlotAxesLimits.XMax);
        IndicatorSignal1.Plot.SetAxisLimitsX(changedPlotAxesLimits.XMin, changedPlotAxesLimits.XMax);

        SignalPlot.Refresh();
        IndicatorSignal1.Refresh();

        SignalPlot.Configuration.AxesChangedEventEnabled = true;
        IndicatorSignal1.Configuration.AxesChangedEventEnabled = true;

    }

    private void CloseSignalEvent(object sender, EventArgs e) {
        SignalPlot.Plot.Clear();
        IndicatorSignal1.Plot.Clear();
        IndicatorSignal2.Plot.Clear();
        _runner = null;

        _openSignalMenuItem.IsEnabled = true;
        _attachSignalMenuItem.IsEnabled = false;
        _exportSignalMenuItem.IsEnabled = false;
        _buyPositionMenuItem.IsEnabled = false;
        _sellPositionMenuItem.IsEnabled = false;
        _returnViewMenuItem.IsEnabled = false;
        _closeSignalMenuItem.IsEnabled = false;

        SignalPlot.Render();
        IndicatorSignal1.Render();
        IndicatorSignal2.Render();
    }

    private void ExportToCsv(object sender, EventArgs e) {
        if (_runner == null)
            return;

        var ofd = new SaveFileDialog();

        ofd.ShowDialog();

        if (ofd.FileName == String.Empty)
            return;

        _runner.ExportToCsv(ofd.FileName);
    }

    private void ReturnView(object sender, EventArgs e) {
        SignalPlot.Plot.AxisAuto();
        IndicatorSignal1.Plot.AxisAuto();
        IndicatorSignal2.Plot.AxisAuto();

        SignalPlot.Render();
        IndicatorSignal1.Render();
        IndicatorSignal2.Render();
    }

    private void SetBuyPosition(object sender, EventArgs e) {
        if (_runner == null)
            return;

        _runner.SetBuy();

        var buySignal = _runner.Signal.Where(m => m.IsBuy);

        SignalPlot.Plot.Remove(_buySignal);

        _buySignal = SignalPlot.Plot.AddScatter(xs: buySignal.Select(m => m.Date.ToOADate()).ToArray(), ys: buySignal.Select(m => m.Low).ToArray(), markerSize: 10, markerShape: MarkerShape.asterisk);
        _buySignal.Color = System.Drawing.Color.Blue;
        _buySignal.LineStyle = LineStyle.None;

        SignalPlot.Refresh();
    }

    private void SetSellPosition(object sender, EventArgs e) {
        if (_runner == null)
            return;

        _runner.SetSell();

        var sellSignal = _runner.Signal.Where(m => m.IsSell);

        SignalPlot.Plot.Remove(_sellSignal);

        _sellSignal = SignalPlot.Plot.AddScatter(xs: sellSignal.Select(m => m.Date.ToOADate()).ToArray(), ys: sellSignal.Select(m => m.High).ToArray(), markerSize: 10, markerShape: MarkerShape.asterisk);
        _sellSignal.Color = System.Drawing.Color.Red;
        _sellSignal.LineStyle = LineStyle.None;

        SignalPlot.Refresh();
    }

    private void SignalPlot_MouseMove(object? sender, EventArgs e) {
        if (_runner == null)
            return;

        if (_refreshRateCounter.ElapsedMilliseconds < _refreshRateMs)
            return;

        _signalXCoordinate = SignalPlot.GetMouseCoordinates().x;

        _refreshRateCounter.Restart();
        
        if (!_runner.GetNearbyCandle(_signalXCoordinate))
            return;

        SignalPlot.Plot.Remove(_highlightedPoint);
        _highlightedPoint = SignalPlot.Plot.AddCandlesticks(_runner.SelectedCandle);
        _highlightedPoint.WickColor = System.Drawing.Color.Black;

        SignalPlot.Refresh();
    }
}
