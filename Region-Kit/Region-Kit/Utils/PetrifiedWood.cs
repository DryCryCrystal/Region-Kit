using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;


namespace RegionKit.Utils
{
    static class PetrifiedWood
    {
        public static void Indent()
        {
            IndentLevel++;
        }
        public static void Unindent()
        {
            IndentLevel--;
        }
        public static void WriteLineIf(bool cond, object o)
        {
            if (cond) WriteLine(o);
        }
        public static void WriteLine(object o, int AddedIndent)
        {
            IndentLevel += AddedIndent;
            WriteLine(o);
            IndentLevel -= AddedIndent;
        }
        public static void WriteLine(object o)
        {
            //string result = string.Empty;
            StringBuilder result = new();
            for (int i = 0; i < IndentLevel; i++) { result.Append("\t"); }
            result.Append(o?.ToString() ?? "null");
            if (!routeback) result.Append("\n");
            Write(result);
        }
        public static void WriteLine()
        {
            WriteLine(string.Empty);
        }
        public static void Write(object o)
        {
            if (routeback) RegionKitMod.ME.publog.Log(BepInEx.Logging.LogLevel.Info, o);
            else
            {
                Console.Write(o);
                SpinUp();
                lock (WriteQueue) WriteQueue.Enqueue(o ?? "null");
            }
        }

        public static void SetNewPathAndErase(string tar, bool noFile = false)
        {
            routeback = noFile;
            LogPath = tar;
            if (!routeback) File.CreateText(tar).Dispose();
        }
        public static Queue<object> WriteQueue { get { _wc ??= new Queue<object>(); return _wc; } set { _wc = value; } }
        private static Queue<Object> _wc = new();
        private static readonly Queue<Tuple<Exception, DateTime>> _encEx = new();
        public static string LogPath { get => LogTarget?.FullName; set { LogTarget = new FileInfo(value); } }
        public static FileInfo LogTarget;
        internal static bool routeback;
        public static int IndentLevel { get { return _indl; } set { _indl = Math.Max(value, 0); } }
        private static int _indl = 0;

        public static void SpinUp()
        {
            //todo(thalber): improve threading?
            Lifetime = 125;
            if (wrThr?.IsAlive ?? false) return;
            wrThr = new Thread(EternalWrite)
            {
                IsBackground = false,
                Priority = ThreadPriority.BelowNormal
            };
            wrThr.Start();
        }
        public static int Lifetime = 0;
        public static bool selfDestruct = false;
        public static void EternalWrite()
        {
            string startMessage = $"PETRIFIED_WOOD writer thread {Thread.CurrentThread.ManagedThreadId} booted up: {DateTime.Now}\n";
            Console.WriteLine(startMessage);
            WriteQueue.Enqueue(startMessage);
            while (Lifetime > 0)
            {
                Lifetime--;
                if (LogTarget == null) continue;
                try
                {
                    using var wt = LogTarget.AppendText();
                    while (WriteQueue.Count > 0)
                    {
                        lock (WriteQueue)
                        {
                            var toWrite = WriteQueue.Dequeue();
                            wt.Write(toWrite.ToString());
                            wt.Flush();
                        }
                        Thread.Sleep(10);
                    }

                    while (_encEx.Count > 0)
                    {
                        lock (_encEx)
                        {
                            var oldex = _encEx.Dequeue();
                            wt.Write($"\nWrite exc encountered on {oldex.Item2}:\n{oldex.Item1}");
                            wt.Flush();
                        }
                        Thread.Sleep(10);

                    }
                }
                catch (Exception e)
                {
                    lock (_encEx) _encEx.Enqueue(new Tuple<Exception, DateTime>(e, DateTime.Now));
                }
                Thread.Sleep(250);
            }
            using (var wt = LogTarget.AppendText())
            {
                string endMessage = $"Logger thread {Thread.CurrentThread.ManagedThreadId} expired due to inactivity: {DateTime.Now}\n";
                Console.WriteLine(endMessage);
                //var bytesTW = Encoding.UTF8.GetBytes(endMessage);
                wt.Write(endMessage);
                wt.Flush();
            }
            if (selfDestruct) typeof(PetrifiedWood).CleanUpStatic();
        }
        public static Thread wrThr;
        
    }
    
}
