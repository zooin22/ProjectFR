using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Godot;

namespace ProjectFR.Core;

public enum DebugLogLevel
{
    Trace,
    Info,
    Warning,
    Error
}

/// <summary>
/// Lightweight debug logger for local development.
/// Writes to Godot output, keeps a short in-memory buffer, and mirrors to user://debug.
/// </summary>
public static class DebugLog
{
    private const int MaxRecentEntries = 250;
    private static readonly List<string> RecentEntriesInternal = new();
    private static readonly object SyncRoot = new();

    private static bool _initialized;
    private static bool _unhandledExceptionHooked;
    private static string _currentLogPath = string.Empty;

    public static bool Enabled => OS.IsDebugBuild() || HasAutomationArg("--projectfr-debug-log");
    public static string CurrentLogPath => _currentLogPath;
    public static IReadOnlyList<string> RecentEntries => RecentEntriesInternal.AsReadOnly();

    public static event Action<string>? EntryAdded;

    public static void Initialize()
    {
        if (_initialized || !Enabled)
            return;

        lock (SyncRoot)
        {
            if (_initialized)
                return;

            var debugDirectory = ProjectSettings.GlobalizePath("user://debug");
            Directory.CreateDirectory(debugDirectory);
            _currentLogPath = Path.Combine(debugDirectory, "projectfr-debug-latest.log");
            File.WriteAllText(_currentLogPath, $"=== ProjectFR debug session started {CreateTimestamp()} ==={System.Environment.NewLine}", Encoding.UTF8);

            if (!_unhandledExceptionHooked)
            {
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
                _unhandledExceptionHooked = true;
            }

            _initialized = true;
        }

        Info("DebugLog", $"initialized :: {_currentLogPath}");
    }

    public static void Trace(string category, string message)
    {
        Write(DebugLogLevel.Trace, category, message);
    }

    public static void Info(string category, string message)
    {
        Write(DebugLogLevel.Info, category, message);
    }

    public static void Warn(string category, string message)
    {
        Write(DebugLogLevel.Warning, category, message);
    }

    public static void Error(string category, string message)
    {
        Write(DebugLogLevel.Error, category, message);
    }

    public static void Exception(string category, Exception exception, string context = "")
    {
        var prefix = string.IsNullOrWhiteSpace(context) ? string.Empty : $"{context} :: ";
        Write(DebugLogLevel.Error, category, $"{prefix}{exception}");
    }

    private static void Write(DebugLogLevel level, string category, string message)
    {
        if (!Enabled)
            return;

        if (!_initialized)
            Initialize();

        var entry = $"[{CreateTimestamp()}] [{level.ToString().ToUpperInvariant()}] [{category}] {message}";

        lock (SyncRoot)
        {
            RecentEntriesInternal.Add(entry);
            if (RecentEntriesInternal.Count > MaxRecentEntries)
            {
                RecentEntriesInternal.RemoveAt(0);
            }

            if (!string.IsNullOrWhiteSpace(_currentLogPath))
            {
                File.AppendAllText(_currentLogPath, entry + System.Environment.NewLine, Encoding.UTF8);
            }
        }

        switch (level)
        {
            case DebugLogLevel.Warning:
                GD.PushWarning(entry);
                break;
            case DebugLogLevel.Error:
                GD.PushError(entry);
                break;
            default:
                GD.Print(entry);
                break;
        }

        EntryAdded?.Invoke(entry);
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        if (args.ExceptionObject is Exception exception)
        {
            Exception("Unhandled", exception, "fatal");
            return;
        }

        Error("Unhandled", $"fatal :: {args.ExceptionObject}");
    }

    private static bool HasAutomationArg(string arg)
    {
        return Array.IndexOf(OS.GetCmdlineUserArgs(), arg) >= 0;
    }

    private static string CreateTimestamp()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
    }
}
