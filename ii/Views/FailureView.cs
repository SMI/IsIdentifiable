using IsIdentifiable.Failures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace ii.Views;

class FailureView : View
{
    Attribute _attNormal;
    Attribute _attHighlight;

    public Failure? CurrentFailure { get; set; }


    public FailureView()
    {
        _attNormal = Attribute.Make(Color.Gray, Color.Black);
        _attHighlight = Attribute.Make(Color.BrightRed, Color.Black);
    }

    public override void Redraw(Rect bounds)
    {
        var w = bounds.Width;
        var h = bounds.Height;

        var problemValue = CurrentFailure?.ProblemValue ?? " ";

        //if the original string validated 
        var originalNewlines = new HashSet<int>();

        for (var i = 0; i < problemValue.Length; i++)
            if (problemValue[i] == '\n')
                originalNewlines.Add(i);

        var lines = Helpers.Wrap(problemValue, bounds.Width).Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var characterOffset = 0;
        Attribute? oldColor = null;

        for (var y = 0; y < h; y++)
        {
            var currentLine = lines.Length > y ? lines[y] : null;

            for (var x = 0; x < w; x++)
            {
                Attribute newColor;
                char symbol;

                if (currentLine == null || x + 1 > currentLine.Length)
                {
                    newColor = _attNormal;
                    symbol = ' ';
                }
                else
                {
                    newColor = CurrentFailure?.Parts.Any(p => p.Includes(characterOffset)) == true
                        ? _attHighlight
                        : _attNormal;

                    symbol = currentLine[x];
                    characterOffset++;

                    //we dropped a \n in our split so have to compensate for that
                    if (originalNewlines.Contains(characterOffset))
                        characterOffset++;
                }

                if (newColor != oldColor)
                {
                    Driver.SetAttribute(newColor);
                    oldColor = newColor;
                }

                AddRune(x, y, symbol);
            }
        }

        if (CurrentFailure == null)
            return;

        Driver.SetAttribute(_attNormal);
        Move(0, h);

        var sb = new StringBuilder();
        sb.Append($"ProblemField: {CurrentFailure.ProblemField}. ");
        sb.Append($"Classifications: ");
        foreach (var failurePart in CurrentFailure.Parts)
            sb.Append($"'{failurePart.Word}' at {failurePart.Offset} => {failurePart.Classification}, ");
        sb.Length -= 2;
        sb.Append('.');
        Driver.AddStr(sb.ToString().PadRight(w));
    }
}
