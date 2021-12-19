using System.Collections.ObjectModel;

namespace SignalViewer.Models.TreeView;

public class PlotGroup {
    public string Name { get; set; } = string.Empty;
    public ObservableCollection<PlotItem> Members { get; set; } = new();
}