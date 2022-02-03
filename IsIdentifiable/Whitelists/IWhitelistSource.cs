﻿using System;
using System.Collections.Generic;

namespace IsIdentifiable.Allowlists
{
    public interface IAllowlistSource : IDisposable
    {
        /// <summary>
        /// Return all unique strings which should be ignored.  These strings should be trimmed.  Case is not relevant.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAllowlist();
    }
}