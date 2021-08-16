using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

using static RWCustom.Custom;


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
            string result = string.Empty;
            for (int i = 0; i < IndentLevel; i++) { result += "\t"; }
            result += o?.ToString() ?? "null";
            result += "\n";
            Write(result);
        }
        public static void WriteLine()
        {
            WriteLine(string.Empty);
        }
        public static void Write(object o)
        {

            Console.Write(o);
            SpinUp();
            lock (WriteQueue) WriteQueue.Enqueue(o ?? "null");
        }

        public static void SetNewPathAndErase(string tar)
        {
            LogPath = tar;
            File.CreateText(tar).Dispose();
        }
        public static Queue<object> WriteQueue { get { _wc = _wc ?? new Queue<object>(); return _wc; } set { _wc = value; } }
        private static Queue<Object> _wc = new Queue<object>();
        private static Queue<Tuple<Exception, DateTime>> _encEx = new Queue<Tuple<Exception, DateTime>>();
        public static string LogPath { get => LogTarget?.FullName; set { LogTarget = new FileInfo(value); } }
        public static FileInfo LogTarget;
        public static int IndentLevel { get { return _indl; } set { _indl = Math.Max(value, 0); } }
        private static int _indl = 0;

        public static void SpinUp()
        {
            Lifetime = 125;
            if (wrThr?.IsAlive ?? false) return;
            wrThr = new Thread(EternalWrite);
            wrThr.IsBackground = false;
            wrThr.Priority = ThreadPriority.BelowNormal;
            wrThr.Start();
        }
        public static int Lifetime = 0;
        public static void EternalWrite()
        {
            string startMessage = $"PETRIFIED_WOOD writer thread {Thread.CurrentThread.ManagedThreadId} booted up: {DateTime.Now}";
            Console.WriteLine(startMessage);
            WriteQueue.Enqueue(startMessage);
            while (Lifetime > 0)
            {
                Thread.Sleep(250);
                Lifetime--;
                if (LogTarget == null) continue;
                try
                {
                    using (var wt = LogTarget.AppendText())
                    {
                        while (WriteQueue.Count > 0)
                        {
                            lock (WriteQueue)
                            {
                                var toWrite = WriteQueue.Dequeue();
                                wt.Write(toWrite.ToString());
                                wt.Flush();
                            }
                            Thread.Sleep(15);
                            //if (WriteQueue.TryDequeue(out var toWrite))
                            //{
                            //    //var bytesTW = Encoding.UTF8.GetBytes(toWrite.ToString());
                            //    //wt.Seek(0, SeekOrigin.End);
                                

                            //}
                            //wt.Write(Encoding.UTF8.GetBytes(res.ToString()));
                        }

                        while (_encEx.Count > 0)
                        {
                            lock (_encEx)
                            {
                                var oldex = _encEx.Dequeue();
                                wt.Write($"\nWrite exc encountered on {oldex.Item2}:\n{oldex.Item1}");
                                wt.Flush();
                            }
                            Thread.Sleep(15);

                        }
                    }
                }
                catch (Exception e)
                {
                    _encEx.Enqueue(new Tuple<Exception, DateTime>(e, DateTime.Now));
                }

            }
            using (var wt = LogTarget.AppendText())
            {
                string endMessage = $"Logger thread {Thread.CurrentThread.ManagedThreadId} expired due to inactivity: {DateTime.Now}\n";
                Console.WriteLine(endMessage);
                //var bytesTW = Encoding.UTF8.GetBytes(endMessage);
                wt.Write(endMessage);
                wt.Flush();
            }
        }
        public static Thread wrThr;
        //internal static void BootUp()
        //{
        //    if (iothread != null && iothread.IsAlive) return;
        //    UnityEngine.Debug.Log("RegionKit.PetrifiedWood: starting a logger thread.");
        //    tarFile = tarFile ?? new FileInfo(Path.Combine(RootFolderDirectory(), defaultLogFileName));
        //    if (!tarFile.Exists) tarFile.CreateText().Dispose();
        //    iothread = new Thread(PerpetualWrite);
        //    iothread.Priority = ThreadPriority.BelowNormal;
        //    iothread.IsBackground = false;
        //    iothread.Start();
        //    if (encEx.Count > 0)
        //    {
        //        UnityEngine.Debug.Log("\n- - - - - -\nEXCEPTIONS ENCOUNTERED DURING A PREVIOUS LOGGER THREAD RUN:");
        //        foreach (var e in encEx) UnityEngine.Debug.Log(e);
        //        UnityEngine.Debug.Log("\n- - - - - -\n");
        //    }
        //    encEx.Clear();
        //}
        //internal static void ShutDown()
        //{
        //    iShouldExist = false;
        //    _wq.Clear();
        //}

        //internal static void SetTarget(FileInfo tg)
        //{
        //    tarFile = tg;
        //}
        //internal static void ClearLogs()
        //{
        //    if (tarFile?.Exists ?? false) tarFile.Delete();
        //}

        //internal static void WriteLine(object o, int addIndent)
        //{
        //    IndentLevel += addIndent;
        //    WriteLine(o);
        //    IndentLevel -= addIndent;
        //}

        //internal static void WriteLine(object o)
        //{
        //    var r = $"{o}\n";
        //    for (int i = 0; i < _indLv; i++) r += "\t";
        //    Write(r);
        //}

        //internal static void Write(object o)
        //{
        //    if (iothread == null || !iothread.IsAlive) BootUp();
        //    _wq.Enqueue(o);
        //}

        //internal static void PerpetualWrite()
        //{
        //    //UnityEngine.Debug.Log($"Starting a logger thread: {Thread.CurrentThread.ManagedThreadId}.");
        //    _wq.Enqueue($"Starting a logger thread {Thread.CurrentThread.ManagedThreadId}: {DateTime.Now}\n\n");

        //    int selfCheckCounter = 0;
        //    int cyclesRunningIdle = 0;
        //    bool thingsAreBad = false;
        //    iShouldExist = true;
        //    //bool iShouldExist = true;
        //    bool currentCycleIdle = true;
        //    while (iShouldExist)
        //    {
        //        Thread.Sleep(0);
        //        if (_wq.Count > 0)
        //        {
        //            currentCycleIdle = false;
        //            var co = _wq.Peek();
        //            var sw = tarFile?.AppendText();
        //            try
        //            {
        //                sw.Write(co.ToString());
        //                sw.Flush();
        //                sw.Dispose();
        //            }
        //            catch (Exception e)
        //            {
        //                encEx.Add(new Tuple<Exception, DateTime>(e, DateTime.Now));
        //                continue;
        //            }
        //            _wq.Dequeue();

        //        }
        //        else
        //        {
        //            if (thingsAreBad || cyclesRunningIdle > 49) iShouldExist = false;
        //        }

        //        selfCheckCounter++;
        //        if (selfCheckCounter == 999) try
        //            {
        //                selfCheckCounter = 0;
        //                if (currentCycleIdle) cyclesRunningIdle++;
        //                currentCycleIdle = true;

        //                var test = UnityEngine.Random.seed;
        //            }
        //            catch 
        //            {
        //                thingsAreBad = true;
        //            }
        //    }
        //    var ams = thingsAreBad ? "No one will hear this." : "Task expired.";
        //    var ws = tarFile.AppendText();
        //    ws.Write($"\n\nLogger thread {Thread.CurrentThread.ManagedThreadId} exiting. " + ams + "\n");
        //    ws.Flush();
        //    ws.Dispose();
        //}
        //internal static bool iShouldExist;
        //internal static int IndentLevel { get { return _indLv; } set { _indLv = IntClamp(value, 0, 35); } }
        //private static int _indLv = 0;
        //private const string defaultLogFileName = "WHISPERS_OF_WOOD.txt";

        //private static List<Tuple<Exception, DateTime>> encEx = new List<Tuple<Exception, DateTime>>();
        //private static Queue<object> _wq = new Queue<object>();
        //private static Thread iothread;
        //public static FileInfo tarFile;
    }
    
}
