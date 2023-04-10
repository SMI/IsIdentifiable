using System.Collections.Generic;
using System.IO.Abstractions;
using IsIdentifiable.Rules;
using Terminal.Gui;

namespace ii.Views.Manager;

class RuleDetailView : View
{
    private Label lblType;
    private readonly List<Label> _properties = new();

    public RuleDetailView()
    {
        lblType = new Label() { Text = "Type:", Height = 1, Width = Dim.Fill()};

        base.Add(lblType);
    }

    public void SetupFor(object obj, IFileInfo file)
    {
        ClearProperties();

        var type = obj.GetType();
        lblType.Text = $"Type:{type.Name}";

        var lbl1 = new Label("Path:")
        {
            Y = 1
        };
        var lbl2 = new Label(file.DirectoryName)
        {
            Y = 2
        };
        var lbl3 = new Label($"File:")
        {
            Y = 3
        };
        var lbl4 = new Label(file.Name)
        {
            Y = 4
        };

        Add(lbl1);
        Add(lbl2);
        Add(lbl3);
        Add(lbl4);

        _properties.Add(lbl1);
        _properties.Add(lbl2);
        _properties.Add(lbl3);
        _properties.Add(lbl4);

        SetNeedsDisplay();
    }

    public void SetupFor(ICustomRule rule)
    {
        ClearProperties();

        var type = rule.GetType();
        lblType.Text = $"Type:{type.Name}";

        var y = 1;
        foreach(var prop in type.GetProperties())
        {
            var val = prop.GetValue(rule);
            var lbl = new Label($"{prop.Name}:{val}")
            {
                Y = y
            };
            y++;

            Add(lbl);
            _properties.Add(lbl);
        }

        SetNeedsDisplay();
    }

    private void ClearProperties()
    {
        foreach (var c in _properties)
        {
            Remove(c);
            c.Dispose();
        }

        _properties.Clear();
    }
}