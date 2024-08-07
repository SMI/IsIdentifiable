﻿using Terminal.Gui;

namespace ii;

public class TerminalGuiTheme
{
    public ColorSchemeBlueprint TopLevel { get; set; } = new();
    public ColorSchemeBlueprint Base { get; set; } = new();
    public ColorSchemeBlueprint Dialog { get; set; } = new();
    public ColorSchemeBlueprint Menu { get; set; } = new();
    public ColorSchemeBlueprint Error { get; set; } = new();
}

public class ColorSchemeBlueprint
{
    public Color FocusForeground { get; set; }
    public Color FocusBackground { get; set; }

    public Color DisabledForeground { get; set; }
    public Color DisabledBackground { get; set; }

    public Color HotFocusForeground { get; set; }
    public Color HotFocusBackground { get; set; }

    public Color HotNormalForeground { get; set; }
    public Color HotNormalBackground { get; set; }

    public Color NormalForeground { get; set; }
    public Color NormalBackground { get; set; }

    public ColorScheme GetScheme()
    {
        return new ColorScheme
        {
            Focus = Application.Driver.MakeAttribute(FocusForeground, FocusBackground),
            Disabled = Application.Driver.MakeAttribute(DisabledForeground, DisabledBackground),
            HotFocus = Application.Driver.MakeAttribute(HotFocusForeground, HotFocusBackground),
            HotNormal = Application.Driver.MakeAttribute(HotNormalForeground, HotNormalBackground),
            Normal = Application.Driver.MakeAttribute(NormalForeground, NormalBackground),
        };
    }
}
