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

namespace Ben.Utilities
{
    public class AppLog : NotifyPropertyChangedBase
    {
        private static AppLog instance = null;

        private ObservableCollection<LogLine> lines;
        
        public AppLog()
        {
            Clear();
        }

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

        public ObservableCollection<LogLine> Lines
        {
            get { return lines; }
            set { this.SetProperty(ref lines, value, "Lines"); }
        }

        public void Clear()
        {
            Lines = new ObservableCollection<LogLine>();
        }

        public void Log(String text)
        {
            Lines.Add(new LogLine(text));
        }

        public void Log(string format, params object[] args)
        {
            this.Log(string.Format(format, args));
        }

        public void Error(String text)
        {
            Lines.Add(new LogLine(text, LogLevel.Error));
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
        public String Text { get; set; }
        public LogLevel Level { get; set; }
        public Color Color
        {
            get
            {
                return GetColorForLevel(Level);
            }
        }

        public LogLine() { }
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
                case LogLevel.Debug:
                    return Colors.DarkGray;
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
