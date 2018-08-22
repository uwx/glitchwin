using System;
using System.Collections.Generic;
using System.Linq;

namespace GlitchWin
{
    internal static class GlitchWinConfig
    {
        private static readonly Dictionary<string, Func<string, object>> Processors = new Dictionary<string, Func<string, object>>()
        {
            ["ScreencapTimer"] = s => Dates.ParseTime(s),
            ["ScreencapOnLock"] = s => bool.Parse(s),
            ["ScreencapOnUnlock"] = s => bool.Parse(s),
            ["ScreencapOnShutdown"] = s => bool.Parse(s),
            ["ScreencapOnLaunch"] = s => bool.Parse(s),
            ["Collect"] = s => bool.Parse(s)
        };

        // screencap timer or 0 for no timer
        public static ulong ScreencapTimer { get; set; } = 0UL;
        
        // screencap on lock/logoff
        public static bool ScreencapOnLock { get; set; } = true;
        
        // screencap on unlock/logon
        public static bool ScreencapOnUnlock { get; set; } = true;
        
        // screencap on shutdown
        public static bool ScreencapOnShutdown { get; set; } = false;
        
        // screencap on opening GlitchWin
        public static bool ScreencapOnLaunch { get; set; } = true;

        // whether or not to run GC after screencap
        public static bool Collect { get; set; } = true;

        public static void LoadConfig(string[] cfgLines)
        {
            var cfgDict = cfgLines
                .Select(e =>
                {
                    if (e.Length == 0) return (null, null);
                    switch (e[0])
                    {
                        case '/':
                        case '\'': 
                        case ':': 
                        case ';': 
                        case '#':
                        case '!':
                            return (null, null);
                    }

                    var idx = e.IndexOf('=');
                    return idx == -1 
                        ? (null, null) 
                        : (key: e.Substring(0, idx).Trim(), value: e.Substring(idx + 1).Trim());
                })
                .Where(e => e.key != null && e.value != null)
                .ToDictionary(e => e.key, e => e.value);

            foreach (var kvp in Processors)
            {
                if (!cfgDict.TryGetValue(kvp.Key, out var v)) continue;

                var res = kvp.Value(v);
                var prop = typeof(GlitchWinConfig).GetProperty(kvp.Key);
                if (prop == null) throw new InvalidOperationException($"missing prop {kvp.Key}");
                prop.SetValue(null, res);

                Console.WriteLine($"Property {kvp.Key} is {res}");
            }
        }
    }
}