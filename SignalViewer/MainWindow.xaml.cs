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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SignalViewer;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {

    private List<PlotGroup> _plotGroups = new() {
        new() { 
            Name = "Main", 
            Members = new ObservableCollection<PlotItem>() {
                new PlotItem() { Name = "Main signal", Indicator = "Raw" }
            } 
        }
    };

    public MainWindow() {
        InitializeComponent();

        trvPlot.ItemsSource = _plotGroups;
    }

    private void meiOpenMain_Click(object sender, RoutedEventArgs e) {

    }

    private void meiAttach_Click(object sender, RoutedEventArgs e) {

    }
}