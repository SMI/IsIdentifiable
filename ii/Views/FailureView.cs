using System;
using System.Collections.Generic;
using System.Linq;
using IsIdentifiable.Reporting;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace IsIdentifiable.Views;

class FailureView : View
{
    Attribute _attNormal;
    Attribute _attHighlight;

    public Failure? CurrentFailure { get; set; }


    public FailureView()
    {
        _attNormal = Attribute.Make(Color.Gray, Color.Black);
        _attHighlight = Attribute.Make(Color.BrightGreen, Color.Black);
    }

    public override void Redraw(Rect bounds)
    {
        var w = bounds.Width;
        var h = bounds.Height;

        var toDisplay = CurrentFailure?.ProblemValue ?? " ";

        //if the original string validated 
        var originalNewlines = new HashSet<int>();

        for (int i = 0; i < toDisplay.Length; i++)
            if (toDisplay[i] == '\n')
                originalNewlines.Add(i);

        var lines = MainWindow.Wrap(toDisplay, bounds.Width).Split('\n', StringSplitOptions.RemoveEmptyEntries);

        int characterOffset = 0;
        Attribute? oldColor = null;

        for (int y = 0; y < h; y++)
        {
            var currentLine = lines.Length > y ? lines[y] : null;

            for (int x = 0; x < w; x++)
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
                    if (CurrentFailure != null && CurrentFailure.Parts.Any(p => p.Includes(characterOffset)))
                        newColor = _attHighlight;
                    else
                        newColor = _attNormal;

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

        if (CurrentFailure != null)
        {
            Driver.SetAttribute(_attNormal);
            Move(0, h);

            string classification =
                $"C:{string.Join(",", CurrentFailure.Parts.Select(p => p.Classification).Distinct().ToArray())}";

            string field = CurrentFailure.ProblemField;
            classification = classification.PadRight(w - field.Length);

            Driver.AddStr(classification + field);
        }

    }
}