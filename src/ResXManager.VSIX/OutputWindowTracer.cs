﻿namespace ResXManager.VSIX
{
    using System;

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    using ResXManager.Infrastructure;

    public class OutputWindowTracer : ITracer
    {
        private readonly IServiceProvider _serviceProvider;

        private static Guid _outputPaneGuid = new Guid("{C49C2D45-A34D-4255-9382-40CE2BDAD575}");

        public OutputWindowTracer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private void LogMessageToOutputWindow(string? value)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (!(_serviceProvider.GetService(typeof(SVsOutputWindow)) is IVsOutputWindow outputWindow))
                return;

            var errorCode = outputWindow.GetPane(ref _outputPaneGuid, out var pane);

            if (ErrorHandler.Failed(errorCode) || pane == null)
            {
                outputWindow.CreatePane(ref _outputPaneGuid, Resources.ToolWindowTitle, Convert.ToInt32(true), Convert.ToInt32(false));
                outputWindow.GetPane(ref _outputPaneGuid, out pane);
            }

            pane?.OutputString(value);
        }

#pragma warning disable VSTHRD010 // Accessing ... should only be done on the main thread.

        public void TraceError(string value)
        {
            WriteLine(string.Concat(Resources.Error, @" ", value));
        }

        public void TraceWarning(string value)
        {
            WriteLine(string.Concat(Resources.Warning, @" ", value));
        }

        public void WriteLine(string value)
        {
            if (!Microsoft.VisualStudio.Shell.ThreadHelper.CheckAccess())
            {
#pragma warning disable VSTHRD001 // Avoid legacy thread switching APIs
                Microsoft.VisualStudio.Shell.ThreadHelper.Generic.BeginInvoke(() => LogMessageToOutputWindow(value + Environment.NewLine));
#pragma warning restore VSTHRD001 // Avoid legacy thread switching APIs
            }
            else
            {
                LogMessageToOutputWindow(value + Environment.NewLine);
            }
        }

#pragma warning restore VSTHRD010 // Accessing ... should only be done on the main thread.
    }
}