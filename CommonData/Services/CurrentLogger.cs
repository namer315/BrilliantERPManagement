using System;
using System.Collections.Generic;
using System.Text;

namespace CommonData.Services;

public class CurrentLogger
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private CurrentLogger() { }

    public static NLog.Logger Instance => _logger;
}
