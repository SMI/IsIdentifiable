using System.Linq;
using Terminal.Gui.Trees;

namespace ii.Views;

internal class TreeNodeWithCount : TreeNode
{
    public string Heading { get; }

    private readonly bool _countSubChildren;

    public TreeNodeWithCount(string heading, bool countSubChildren = false)
    {
        Heading = heading;
        _countSubChildren = countSubChildren;
    }

    public override string ToString()
    {
        var count = 0;
        if (_countSubChildren)
            count = Children.Sum(x => x.Children.Count);
        else
            count = Children.Count;
        return $"{Heading} ({count:N0})";
    }
}
