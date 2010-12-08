using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace Ben.Dominion
{
    public class AppLog
    {
        private static AppLog instance = null;
        public static AppLog Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AppLog();
                }
                return instance;

            }
        }

        public ObservableCollection<LogLine> Lines { get; set; }

        public AppLog()
        {
            Clear();
        }

        public void Clear()
        {
            Lines = new ObservableCollection<LogLine>();
        }

        public void Log(String text)
        {
            Lines.Add(new LogLine(text));
        }

        public void Error(String text)
        {
            Lines.Add(new LogLine(text, LogLevel.Error));
        }
    }

    public enum LogLevel
    {
        None,
        Info,
        Error
    }

    public class LogLine
    {
        public String Text { get; set; }
        public LogLevel Level { get; set; }
        public Color Color
        {
            get
            {
                return GetColorForLevel(Level);
            }
        }

        public LogLine(String text) : this(text, LogLevel.Info) { }
        public LogLine(String text, LogLevel level)
        {
            this.Text = text;
            this.Level = level;
        }

        public static Color GetColorForLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Info:
                case LogLevel.Error:
                default:
                    return Colors.Red;
            }
        }
    }
}
