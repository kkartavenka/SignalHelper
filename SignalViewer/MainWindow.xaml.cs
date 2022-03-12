using SignalViewer.Models.TreeView;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SignalViewer;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {

    private uint _plotCounter = 1;
    private readonly ContextMenu _cmTreeViewRoot, _cmTreeViewSub;
    private List<ScottPlot.WpfPlot> _signals = new();

    private ObservableCollection<PlotGroup> _plotGroups = new() {
        new() { 
            Name = "Main", 
            ItemType = TreeViewItemType.RootMain,
            Members = new ObservableCollection<PlotItem>() {
                new PlotItem() { Name = "Main signal", Indicator = "Raw", ItemType = TreeViewItemType.SubMain }
            } 
        }
    };

    public MainWindow() {
        InitializeComponent();

        _cmTreeViewRoot = (ContextMenu)FindResource("cmTreeViewRoot");
        _cmTreeViewSub = (ContextMenu)FindResource("cmTreeViewSub");

        trvPlot.ItemsSource = _plotGroups;
    }

    private void meiOpenMain_Click(object sender, RoutedEventArgs e) {

    }

    private void meiAttach_Click(object sender, RoutedEventArgs e) {

    }

    private void AddPlotMenu_Click(object sender, RoutedEventArgs e) {
        var newPlot = new ScottPlot.WpfPlot() { MinHeight = 100 };
        plotPanel.Children.Add(newPlot);
        _plotGroups.Add(new PlotGroup() {
            Name = $"New Plot [{_plotCounter}]", 
            ItemType = TreeViewItemType.Root, 
            Members = new()
        });
    }

    private void trvPlot_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e) {

        var source = e.OriginalSource as DependencyObject;

        while (source != null && !(source is TreeViewItem))
            source = VisualTreeHelper.GetParent(source);

        var treeViewItem = source as TreeViewItem;

        if (treeViewItem != null) {
            treeViewItem.Focus();
            e.Handled = true;

            if (treeViewItem.DataContext.GetType() == typeof(PlotGroup)) {

                ((MenuItem)_cmTreeViewRoot.Items[1]).IsEnabled = !(((PlotGroup)treeViewItem.DataContext).ItemType == TreeViewItemType.RootMain);

                _cmTreeViewRoot.PlacementTarget = treeViewItem;
                _cmTreeViewRoot.IsOpen = true;

            } else if (treeViewItem.DataContext.GetType() == typeof(PlotItem)) {

                ((MenuItem)_cmTreeViewSub.Items[1]).IsEnabled = !(((PlotItem) treeViewItem.DataContext).ItemType == TreeViewItemType.SubMain);
                ((MenuItem)_cmTreeViewSub.Items[2]).IsEnabled = !(((PlotItem)treeViewItem.DataContext).ItemType == TreeViewItemType.SubMain);

                _cmTreeViewSub.PlacementTarget = treeViewItem;
                _cmTreeViewSub.IsOpen = true;
            }
        } else {

            ((MenuItem)_cmTreeViewRoot.Items[1]).IsEnabled = false;
            _cmTreeViewRoot.PlacementTarget = (TreeView)sender;
            _cmTreeViewRoot.IsOpen = true;

        }
    }
}