using System;
using System.Text.RegularExpressions;

namespace GlitchWin
{
    public static class Dates
    {
        public static ulong ParseTime(string dataToParse)
        {
            var tokenizer = new StringTokenizer(dataToParse);
            var time = 0UL;
            
            foreach (var s in tokenizer)
            {
                time = ParseTokenInternal(tokenizer, s.Trim(), time);
            }

            return time;
        }
        
        // TODO i've noticed all my returns are just "time" or "time + X", why not make the return value a relative
        // instead of an absolute?

        /// <summary>
        /// Action for an individual token in the reminder text.
        /// </summary>
        /// <param name="tokenizer">The tokenizer instance</param>
        /// <param name="s">The token</param>
        /// <param name="time">The current time state</param>
        /// <returns>A <see cref="ValueTuple{T1}"/>&lt;<see cref="Boolean"/>, <see cref="UInt64"/>&gt; containing
        /// <c>true</c> to continue parsing execution or <c>false</c> to escape control flow. The returned value of
        /// <c>time</c> is stored and used for the next token, or for the return value if control flow is escaped.</returns>
        /// <exception cref="Exception"></exception>
        private static ulong ParseTokenInternal(StringTokenizer tokenizer, string s, ulong time)
        {
            // filler words
            if (DateLexer.IsFillerWord(s))
                return time;

            // read values like "5 seconds"
            if (DateLexer.TryIsNumber(s, out var i))
            {
                // ended early, so assume it's talking about minutes (e.g remindme in 5)
                if (!tokenizer.Next(out var s2))
                    return time + (i * (ulong)Unit.Minutes);
                
                // ending words, so assume it's talking about minutes (e.g remindme in 5 to ...)
                if (DateLexer.IsFinishingWord(s2))
                    return time + (i * (ulong)Unit.Minutes);
                
                // get the amount of ms that corresponds to the unit of time s2
                if (!Enum.TryParse<Unit>(s2.Trim(), true, out var tk))
                    throw new Exception($"Unknown amount of time '{i}' of '{s2}'. If you think this is an unaccounted-for scenario, notify the dev!");

                // return N * MsValue
                return time + ((ulong) tk * i);
            }
            
            // read compound values like "15h14min" or "5s"
            if (TryParseCompound(s, out var j))
                return time + j;
            
            // read values like "15:14" as 15h14min
            if (TimeSpan.TryParse(s, out var ts))
                return time + (ulong) ts.TotalMilliseconds;

            // ending words, so break and make the rest into a message
            if (DateLexer.IsFinishingWord(s))
                return time;

            Console.WriteLine("####\n" +
                              $"Unrecognized token: {s}\n" +
                              $"In text: {tokenizer.String}\n" +
                              $"At pos:  {new string('-', Math.Max(0, (tokenizer.Index==-1?tokenizer.String.Length:tokenizer.Index)-tokenizer.Current?.Length??0))}^ (semi-accurate)\n" + // this is bad
                              "####");
            
            // what to do with invalid tokens? break? continue? i guess break
            return time;
        }

        /// <summary>
        /// Attempt to parse a length of time in a single word in <c>15h5m</c> format
        /// </summary>
        /// <param name="s">The token to parse</param>
        /// <param name="time">The resulting time to set</param>
        /// <returns>True if there was anything to parse, false otherwise</returns>
        /// <exception cref="Exception">If an invalid unit is encountered</exception>
        private static bool TryParseCompound(string s, out ulong time)
        {
            time = 0UL;
            var re = new Regex("([0-9]+)([a-zA-Z]+)");
            foreach (Match match in re.Matches(s))
            {
                var i = uint.Parse(match.Groups[1].Value);
                var unit = match.Groups[2].Value;

                // get the amount of ms that corresponds to the unit of time unit
                if (!Enum.TryParse<Unit>(unit.Trim(), true, out var tk))
                    throw new Exception($"Unknown amount of time '{i}' of '{unit}'. If you think this is an unaccounted-for scenario, notify the dev!");

                // add N * MsValue
                time += (ulong) tk * i;
            }
            
            // we succeed if we matched/parsed anything, so the output is >0
            return time != 0UL;
        }

        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedMember.Global
        private enum Unit : ulong
        {
            Ms = 1,
            Milis = Ms,
            Millis = Ms,
            Milisecond = Ms,
            Miliseconds = Ms,
            Millisecond = Ms,
            Milliseconds = Ms,

            S = 1000 * Milisecond,
            Sec = S,
            Second = S,
            Secs = S,
            Seconds = S,

            M = 60 * Second,
            Min = M,
            Minute = M,
            Mins = M,
            Minutes = M,

            H = 60 * Minute,
            Hr = H,
            Hrs = H,
            Hour = H,
            Hours = H,

            D = 24 * Hour,
            Ds = D,
            Day = D,
            Days = D,

            W = 7 * Day,
            Ws = W,
            Week = W,
            Weeks = W,

            // Uncommon / fictional units

            Fortnight = 14 * Day, // 14 days
            Month = 30 * Day,
            Quarter = 3 * Month, // 3 months
            Semester = 6 * Month, // 6 months
            Year = 12 * Month,
            Astrosecond = 498 * Millisecond, //0.498 seconds
            Breem = 498 * Second, //8.3 minutes
            Cyberweek = 7 * Day, //7 days
            Cycle = 75 * Minute, //1.25 hours
            Decacycle = 21 * Day, //21 days
            Groon = 70 * Hour, //1 hour
            Klik = 72 * Second, //1.2 minutes
            Lightyear = 100 * Year, //1 year
            Megacycle = 156 * Minute, //2.6 hours
            Metacycle = 13 * Month, //13 months
            Nanocycle = 1 * Second, //1 second
            Nanoklik = 1200 * Millisecond, //1.2 second
            Orbital = 10 * Month, //1 month
            Quartex = 15 * Month, //1 month
            Stellarcycle = 225 * Day, //7.5 months
            Vorn = 83 * Year, //83 years
            Decivorn = 2988 * Day, //8.3 years
            Exapi = 3_141_590_400 * Millisecond, //36.361 days

            // Plural forms, ditto

            Fortnights = Fortnight,
            Months = Month,
            Quarters = Quarter,
            Semesters = Semester,
            Years = Year,
            Astroseconds = Astrosecond,
            Breems = Breem,
            Cyberweeks = Cyberweek,
            Cycles = Cycle,
            Decacycles = Decacycle,
            Groons = Groon,
            Kliks = Klik,
            Lightyears = Lightyear,
            Megacycles = Megacycle,
            Metacycles = Metacycle,
            Nanocycles = Nanocycle,
            Nanokliks = Nanoklik,
            Orbitals = Orbital,
            Quartexs = Quartex,
            Quartexes = Quartex,
            Stellarcycles = Stellarcycle,
            Vorns = Vorn,
            Decivorns = Decivorn,
            Exapis = Exapi
        }
        // ReSharper restore UnusedMember.Global
        // ReSharper restore UnusedMember.Local

        /// <summary>
        /// Turn a length of milliseconds into a <see cref="TimeSpan"/>
        /// </summary>
        /// <param name="time">The amount of milliseconds to convert</param>
        /// <returns>The newly created <see cref="TimeSpan"/></returns>
        private static TimeSpan Ts(double time) => TimeSpan.FromMilliseconds(time);
    }
}