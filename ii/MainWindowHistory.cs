﻿using IsIdentifiable.Redacting;

namespace ii;

internal class MainWindowHistory
{
    public int Index { get; }
    public OutBase OutputBase { get; }

    public MainWindowHistory(int index, OutBase outputBase)
    {
        Index = index;
        OutputBase = outputBase;
    }
}
