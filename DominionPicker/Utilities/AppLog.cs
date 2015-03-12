using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Media;
using GoogleAnalytics;

namespace Ben.Utilities
{
    public class AppLog : NotifyPropertyChangedBase
    {
        private static AppLog instance;

        private ObservableCollection<LogLine> lines;

        public AppLog()
        {
            this.Clear();
        }

        public static AppLog Instance
        {
            get { return instance ?? (instance = new AppLog()); }
        }

        public ObservableCollection<LogLine> Lines
        {
            get { return this.lines; }
            set { this.SetProperty(ref this.lines, value, "Lines"); }
        }

        public void Clear()
        {
            this.Lines = new ObservableCollection<LogLine>();
        }

        public void Log(String text)
        {
            this.Lines.Add(new LogLine(text));
            System.Diagnostics.Debug.WriteLine("Info: " + text);
        }

        public void Log(string format, params object[] args)
        {
            this.Log(string.Format(format, args));
        }

        public void Debug(string format, params object[] args)
        {
            this.Debug(string.Format(format, args));
        }

        public void Debug(string message)
        {
            this.Lines.Add(new LogLine(message, LogLevel.Debug));
        }

        public void Error(String text)
        {
            this.Lines.Add(new LogLine(text, LogLevel.Error));
            System.Diagnostics.Debug.WriteLine("Error: " + text);
        }

        public void Error(String error, Exception exception)
        {
            this.Error(error);
            BugSense.BugSenseHandler.Instance.LogException(exception, "Message", error);
            EasyTracker.GetTracker().SendException(exception.ToString(), false);
        }
    }

    public enum LogLevel
    {
        None,
        Debug,
        Info,
        Warning,
        Error
    }

    public class LogLine
    {
        public LogLine()
        {
        }

        public LogLine(String text) : this(text, LogLevel.Info)
        {
        }

        public LogLine(String text, LogLevel level)
        {
            this.Text = text;
            this.Level = level;
        }

        public String Text { get; set; }
        public LogLevel Level { get; set; }

        public Color Color
        {
            get { return GetColorForLevel(this.Level); }
        }

        public static Color GetColorForLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return Colors.LightGray;
                case LogLevel.Info:
                    return Colors.Gray;
                case LogLevel.Warning:
                    return Colors.Orange;
                case LogLevel.Error:
                    return Colors.Red;
                default:
                    return Colors.Green;
            }
        }
    }
}