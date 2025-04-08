using System.Linq;
using Terminal.Gui.Trees;

namespace ii.Views;

internal class TreeNodeWithCount : TreeNode
{
    public string Heading { get; }

    private readonly bool _countSubChildren;

    public int OverrideCount { get; set; } = -1;

    public TreeNodeWithCount(string heading, bool countSubChildren = false)
    {
        Heading = heading;
        _countSubChildren = countSubChildren;
    }

    public override string ToString()
    {
        var count = 0;
        if (OverrideCount != -1)
            count = OverrideCount;
        else if (_countSubChildren)
            count = Children.Sum(x => x.Children.Count);
        else
            count = Children.Count;
        return $"{Heading} ({count:N0})";
    }
}
