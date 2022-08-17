using System;

namespace DisableDateTimeNow.Console;

internal static class Program
{
    public static void Main()
    {
        var now = DateTime.Now;
        System.Console.WriteLine(now);
    }
}