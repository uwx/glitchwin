using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using Timer = System.Timers.Timer;
using Encoder = System.Drawing.Imaging.Encoder;

namespace GlitchWin
{
    internal static class Program
    {
        private const string BackgroundRoot = @"C:\Windows\System32\oobe\info\backgrounds";
        private const string BackgroundPath = BackgroundRoot + @"\backgroundDefault.jpg";
        private const int BackgroundSizeLimit = 249 * 1000;
        
        private static readonly ImageCodecInfo JpegCodec = ImageCodecInfo.GetImageEncoders()
            .First(codec => codec.FormatID == ImageFormat.Jpeg.Guid);

        private const string BackgroundKey =
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI\Background";

        private const string IpcGuid = "21ec421a-841f-4723-87b8-c6dfbd470596";

        private static void SetOemBackground(bool value)
        {
            using (var key = Registry.LocalMachine.OpenSubKey(BackgroundKey, true))
            {
                Debug.Assert(key != null, $"{nameof(key)} != null");
                key.SetValue("OEMBackground", value ? 1 : 0, RegistryValueKind.DWord);
            }
        }

        private static bool GetOemBackground()
        {
            using (var key = Registry.LocalMachine.OpenSubKey(BackgroundKey))
            {
                Debug.Assert(key != null, $"{nameof(key)} != null");
                return (int) key.GetValue("OEMBackground", 0) == 1;
            }
        }

        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "delet")
                {
                    SetOemBackground(false);
                    File.Delete(BackgroundPath);
                    Console.WriteLine("deleeted");
                }
                else
                {
                    Console.WriteLine("GlitchWin usage:\n" +
                                      "  run with no arguments to start the service" +
                                      "  use argument 'delet' (no quotes) to unset the background");
                }
                return;
            }
            Directory.CreateDirectory(BackgroundRoot);
            
            if (!GetOemBackground())
            {
                Console.WriteLine("Setting OEM background flag to true");
                SetOemBackground(true);
            }
            
            GlitchWinConfig.LoadConfig(File.ReadAllLines("config.cfg"));

            SystemEvents.SessionEnded += (sender, e) =>
            {
                switch (e.Reason)
                {
                    case SessionEndReasons.Logoff:
                        if (GlitchWinConfig.ScreencapOnLock) Screencap();
                        Console.WriteLine("sess logoff");
                        break;
                    case SessionEndReasons.SystemShutdown:
                        if (GlitchWinConfig.ScreencapOnShutdown) Screencap();
                        Console.WriteLine("sess shutdown");
                        break;
                }
            };
            SystemEvents.SessionSwitch += (sender, e) =>
            {
                switch (e.Reason)
                {

                    case SessionSwitchReason.SessionLogoff:
                    case SessionSwitchReason.SessionLock:
                        if (GlitchWinConfig.ScreencapOnLock) Screencap();
                        Console.WriteLine("sess lock");
                        break;
                    case SessionSwitchReason.SessionLogon:
                    case SessionSwitchReason.SessionUnlock:
                        if (GlitchWinConfig.ScreencapOnUnlock) Screencap();
                        break;
                }
            };

            if (GlitchWinConfig.ScreencapOnLaunch) Screencap();

            Timer timer = null;
            if (GlitchWinConfig.ScreencapTimer != 0UL)
            {
                timer = new Timer();
                timer.Elapsed += (source, e) =>
                {
                    Screencap();
                };
                timer.Interval = GlitchWinConfig.ScreencapTimer;
                timer.Enabled = true;
            }

            // https://stackoverflow.com/a/12367882
            // Create a IPC wait handle with a unique identifier.
            var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, IpcGuid, out var createdNew);

            if (!createdNew)
            {
                Console.WriteLine("Inform other process to stop.");
                waitHandle.Set();
            }
            
            waitHandle.WaitOne();
            
            Console.WriteLine("Got signal to kill myself.");
            timer?.Dispose();
        }

        // https://stackoverflow.com/a/1163770
        private static void Screencap()
        {
            var screenLeft = Screen.PrimaryScreen.WorkingArea.Left;
            var screenTop = Screen.PrimaryScreen.WorkingArea.Top;
            var screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            var screenHeight = Screen.PrimaryScreen.WorkingArea.Height;

            using (var bitmap = new Bitmap(screenWidth, screenHeight))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(screenLeft, screenTop, 0, 0, bitmap.Size);
                }

                using (var stream = new MemoryStream())
                {
                    var length = long.MaxValue;
                    var quality = 100L;
                    while (length > BackgroundSizeLimit && quality > 10L)
                    {
                        stream.Position = 0;
                        stream.SetLength(0);

                        // https://stackoverflow.com/q/3957477
                        using (var parameters = new EncoderParameters(1)
                        {
                            Param = {[0] = new EncoderParameter(Encoder.Quality, quality)}
                        })
                        {
                            bitmap.Save(stream, JpegCodec, parameters);
                        }

                        length = stream.Length;
                        quality /= 2;
                    }

                    if (quality <= 10L)
                    {
                        throw new Exception("Could not make screenshot into an image that will fit!");
                    }

                    Console.WriteLine($"Made jpeg {length / 1024} kib ({quality}% quality)");

                    // not getbuffer since we mess with the length
                    var glitched = Glitch(stream.ToArray(), 18, 0, 21);
                    File.WriteAllBytes(BackgroundPath, glitched);
                }
            }

            if (GlitchWinConfig.Collect) GC.Collect();
        }

        // taken from https://github.com/snorpey/glitch-canvas
        // also contains stuff from http://stackoverflow.com/a/10424014/229189

        /*
         * The MIT License (MIT)
         * 
         * Copyright (c) 2013-2017 Georg Fischer
         * 
         * Permission is hereby granted, free of charge, to any person obtaining a copy of
         * this software and associated documentation files (the "Software"), to deal in
         * the Software without restriction, including without limitation the rights to
         * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
         * the Software, and to permit persons to whom the Software is furnished to do so,
         * subject to the following conditions:
         * 
         * The above copyright notice and this permission notice shall be included in all
         * copies or substantial portions of the Software.
         * 
         * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
         * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
         * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
         * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
         * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
         * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
         */

        public static int JpgHeaderLength(byte[] byteArr)
        {
            var result = 417;

            for (int i = 0, len = byteArr.Length; i < len; i++)
            {
                if (byteArr[i] != 0xFF || byteArr[i + 1] != 0xDA) continue;

                result = i + 2;
                break;
            }

            return result;
        }

        public static byte[] Glitch(byte[] byteArray, int seed, int amount, int iterationCount)
        {
            var headerLength = JpgHeaderLength(byteArray);
            var maxIndex = byteArray.Length - headerLength - 4;

            var amountPercent = amount / 100.0;
            var seedPercent = seed / 100.0;

            for (var iterationIndex = 0; iterationIndex < iterationCount; iterationIndex++)
            {
                var minPixelIndex = (maxIndex / iterationCount * iterationIndex) | 0;
                var maxPixelIndex = (maxIndex / iterationCount * (iterationIndex + 1)) | 0;

                var delta = maxPixelIndex - minPixelIndex;
                var pixelIndex = (int) (minPixelIndex + delta * seedPercent);

                if (pixelIndex > maxIndex)
                {
                    pixelIndex = maxIndex;
                }

                var indexInByteArray = headerLength + pixelIndex;

                byteArray[indexInByteArray] = (byte) (amountPercent * 256);
            }

            return byteArray;
        }
    }
}