using System.Collections.ObjectModel;

namespace SignalViewer.Models.TreeView;

public enum TreeViewItemType {
    Root,
    RootMain,
    Sub,
    SubMain
};

public class PlotGroup {
    public string Name { get; set; } = string.Empty;
    public TreeViewItemType ItemType { get; set; } = TreeViewItemType.Root;
    public ObservableCollection<PlotItem> Members { get; set; } = new();
}