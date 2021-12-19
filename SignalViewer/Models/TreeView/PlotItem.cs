namespace SignalViewer.Models.TreeView;

public class PlotItem {
    public string Name { get; set; } = string.Empty;
    public string Indicator { get; set; } = string.Empty;
    public TreeViewItemType ItemType { get; set; } = TreeViewItemType.Sub;

}