using SignalViewer.Models.TreeView;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SignalViewer;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {

    private ContextMenu _cmTreeViewRoot, _cmTreeViewSub;

    private List<PlotGroup> _plotGroups = new() {
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