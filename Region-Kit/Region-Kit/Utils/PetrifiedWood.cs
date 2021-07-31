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
        internal static void BootUp()
        {
            UnityEngine.Debug.Log("RegionKit.PetrifiedWood: starting a logger thread.");
            iothread = new Thread(PerpetualWrite);
            iothread.Priority = ThreadPriority.BelowNormal;
            iothread.IsBackground = false;
            iothread.Start();
            if (encEx.Count > 0)
            {
                UnityEngine.Debug.Log("\n- - - - - -\nEXCEPTIONS ENCOUNTERED DURING A PREVIOUS LOGGER THREAD RUN:");
                foreach (var e in encEx) UnityEngine.Debug.Log(e);
                UnityEngine.Debug.Log("\n- - - - - -\n");
            }
            encEx.Clear();
        }


        internal static void SetTarget(FileInfo tg)
        {
            tarFile = tg;
        }

        internal static void WriteLine(object o, int addIndent)
        {
            IndentLevel += addIndent;
            WriteLine(o);
            IndentLevel -= addIndent;
        }

        internal static void WriteLine(object o)
        {
            var r = $"{o}\n";
            for (int i = 0; i < _indLv; i++) r += "\t";
            Write(r);
        }

        internal static void Write(object o)
        {
            if (!iothread?.IsAlive ?? false) BootUp();
            tarFile = tarFile ?? new FileInfo(Path.Combine(RootFolderDirectory(), defaultLogFileName));
            _wq.Enqueue(o);
        }

        internal static void PerpetualWrite()
        {
            UnityEngine.Debug.Log($"Starting a logger thread: {Thread.CurrentThread.ManagedThreadId}.");
            int selfCheckCounter = 0;
            int cyclesRunningIdle = 0;
            bool thingsAreBad = false;
            bool iShouldExist = true;
            bool currentCycleIdle = true;
            while (iShouldExist)
            {
                Thread.Sleep(0);
                if (_wq.Count > 0)
                {
                    currentCycleIdle = false;
                    var co = _wq.Peek();
                    var sw = tarFile?.AppendText();
                    try
                    {
                        sw.Write(co.ToString());
                        sw.Dispose();
                    }
                    catch (Exception e)
                    {
                        encEx.Add(e);
                        continue;
                    }
                    _wq.Dequeue();

                }
                else
                {
                    if (thingsAreBad || cyclesRunningIdle >= 99) iShouldExist = false;
                }

                selfCheckCounter++;
                if (selfCheckCounter == 999) try
                    {
                        selfCheckCounter = 0;
                        if (currentCycleIdle) cyclesRunningIdle++;
                        currentCycleIdle = true;

                        var test = UnityEngine.Random.seed;
                    }
                    catch 
                    {
                        thingsAreBad = true;
                    }
            }
            var ams = thingsAreBad ? "No one will hear this." : "Expired due to lack of input.";
            UnityEngine.Debug.Log($"Logger thread {Thread.CurrentThread.ManagedThreadId} exiting. " + ams);
        }

        internal static int IndentLevel { get { return _indLv; } set { _indLv = IntClamp(value, 0, 35); } }
        private static int _indLv = 0;
        private const string defaultLogFileName = "WHISPERS_OF_WOOD";

        private static List<Exception> encEx = new List<Exception>();
        private static Queue<object> _wq = new Queue<object>();
        private static Thread iothread;
        public static FileInfo tarFile;
    }
    
}
