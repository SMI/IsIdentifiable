using IsIdentifiable.Rules;
using Terminal.Gui.Trees;

namespace ii.Views;

internal class DuplicateRulesNode : TreeNode
{
    private IsIdentifiableRule[] Rules { get; }

    public DuplicateRulesNode(string pattern, IsIdentifiableRule[] rules)
    {
        Rules = rules;

        base.Text = $"{pattern} ({Rules.Length})";
    }

}
