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
using System.Diagnostics;

namespace Ben.Utilities
{
    public static class PerformanceHelper
    {
        public static long Measure(Action action)
        {
            return Measure(action, 1);
        }

        public static long Measure(Action action, int iterations)
        {
            // Call it once to get all JIT stuff done
            action();

            Stopwatch s = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                action();
            }
            s.Stop();

            return s.ElapsedMilliseconds;
        }
    }
}
