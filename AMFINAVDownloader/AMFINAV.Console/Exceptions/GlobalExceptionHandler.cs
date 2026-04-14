using Serilog;

namespace AMFINAV.Console.Exceptions
{
    public static class GlobalExceptionHandler
    {
        public static void Register()
        {
            // Level 1 — Unhandled exceptions on any thread
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            // Level 2 — Unobserved Task exceptions (async fire-and-forget)
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            Log.Information("Global exception handlers registered.");
        }

        private static void OnUnhandledException(
            object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;

            Log.Fatal(exception,
                "💥 Unhandled exception — IsTerminating: {IsTerminating}",
                e.IsTerminating);

            // Write to Event Viewer
            System.Diagnostics.EventLog.WriteEntry("AMFINAV NAV Downloader", $"Unhandled exception: {exception?.Message}\n{exception?.StackTrace}", System.Diagnostics.EventLogEntryType.Error);

            Log.CloseAndFlush();
        }

        private static void OnUnobservedTaskException(
            object? sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.Error(e.Exception,
                "⚠️ Unobserved task exception — marking as observed");

            // Write to Event Viewer
            System.Diagnostics.EventLog.WriteEntry("AMFINAV NAV Downloader", $"Unobserved task exception: {e.Exception?.Message}", System.Diagnostics.EventLogEntryType.Warning);

            // Prevent process crash for unobserved task exceptions
            e.SetObserved();
        }
    }
}