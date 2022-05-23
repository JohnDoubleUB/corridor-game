using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DebugStuff
{
    public class ConsoleToGUI : MonoBehaviour
    {
        public bool EnableDebugLogs;
        private CG_TestingData TestingData;

        string myLog = "*begin log";
        //string filename = "";
        bool doShow = false;
        int kChars = 700;
        void OnEnable()
        {
            Application.logMessageReceived += Log;
        }
        void OnDisable()
        {
            Application.logMessageReceived -= Log;
            TestingData.SessionLogs.Add(myLog);
            TestingData.SessionQuitCount++;
            TestingData.CorridorPieceLog.Add("[SESSION_END]");
            TestingData.SessionLogs.Add("[SESSION_END]");
            if (EnableDebugLogs) SaveSystem.SaveTestingData(TestingData);
        }
        void Update() { if (Input.GetKeyDown(KeyCode.BackQuote)) { doShow = !doShow; } }

        private void Awake()
        {

            string fileName = Application.persistentDataPath + "/" + "test" + "." + "bin";


            //using (var stream = File.Open(fileName, FileMode.Create))
            //{
            //    using (var writer = new BinaryWriter(stream, Encoding.UTF8, false))
            //    {
            //        writer.Write(1.250F);
            //        writer.Write(@"c:\Temp");
            //        writer.Write(10);
            //        writer.Write(true);
            //        writer.Write("test2");
   
            //    }
            //}

            //float aspectRatio;
            //string tempDirectory;
            //int autoSaveTime;
            //bool showStatusBar;
            //string test2;

            //if (File.Exists(fileName))
            //{
            //    using (var stream = File.Open(fileName, FileMode.Open))
            //    {
            //        using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
            //        {
            //            aspectRatio = reader.ReadSingle();
            //            tempDirectory = reader.ReadString();
            //            autoSaveTime = reader.ReadInt32();
            //            showStatusBar = reader.ReadBoolean();
            //            test2 = reader.ReadString();
            //        }
            //    }

            //    Debug.Log("Aspect ratio set to: " + aspectRatio);
            //    Debug.Log("Temp directory is: " + tempDirectory);
            //    Debug.Log("Auto save time set to: " + autoSaveTime);
            //    Debug.Log("Show status bar: " + showStatusBar);
            //    Debug.Log("Other test: " + test2);
            //}

            if (EnableDebugLogs)
            {
                TestingData = SaveSystem.LoadType == GameLoadType.Existing && SaveSystem.TryLoadTestingData(out CG_TestingData testData) ? testData : new CG_TestingData();
                TestingData.CorridorPieceLog.Add("[SESSION_START]");
                TestingData.SessionLogs.Add("[SESSION_START]");
                TestingData.SessionStartCount++;
            }
        }

        public void Log(string logString, string stackTrace, LogType type)
        {
            logString = logString + stackTrace;
            // for onscreen...
            myLog = myLog + "\n" + logString;
            if (myLog.Length > kChars) { myLog = myLog.Substring(myLog.Length - kChars); }

            // for the file ...
            //if (filename == "")
            //{
            //    string d = System.Environment.GetFolderPath(
            //       System.Environment.SpecialFolder.Desktop) + "/YOUR_LOGS";
            //    System.IO.Directory.CreateDirectory(d);
            //    string r = Random.Range(1000, 9999).ToString();
            //    filename = d + "/log-" + r + ".txt";
            //}
            //try { System.IO.File.AppendAllText(filename, logString + "\n"); }
            //catch { }
        }

        void OnGUI()
        {
            if (!doShow) { return; }
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
               new Vector3(Screen.width / 1200.0f, Screen.height / 800.0f, 1.0f));
            GUI.TextArea(new Rect(10, 10, 540, 370), myLog);
        }

        public void LogSectionChange(string sectionChange, bool uniqueChange = false)
        {
            if (EnableDebugLogs && TestingData != null)
            {
                TestingData.CorridorPieceLog.Add(sectionChange);
                if (uniqueChange) TestingData.CorridorPieceChangeCount++;
            }
        }

        public void GenerateTestingLog()
        {
            if (EnableDebugLogs)
            {
                //for the file ...
                string r = "debug1";
                string filename = "";
                if (filename == "")
                {
                    string d = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/BATHO-TEL_LOGS";
                    System.IO.Directory.CreateDirectory(d);
                    r = Random.Range(1000, 9999).ToString();
                    filename = d + "/log-" + r + ".txt";
                }
                try
                {
                    System.IO.File.AppendAllLines(filename, TestingData.GetLogByLines(r, myLog));
                }
                catch { }
            }
        }
    }
}

[System.Serializable]
public class CG_TestingData_Serialized
{
    public string[] SessionLogs;
    public string[] CorridorPieceLog;
    public int CorridorPieceChangeCount;
    public int SessionStartCount;
    public int SessionQuitCount;

    public CG_TestingData Deserialize()
    {
        CG_TestingData tD = new CG_TestingData();

        tD.SessionLogs = SessionLogs.ToList();
        tD.CorridorPieceLog = CorridorPieceLog.ToList();
        tD.CorridorPieceChangeCount = CorridorPieceChangeCount;
        tD.SessionStartCount = SessionStartCount;
        tD.SessionQuitCount = SessionQuitCount;

        return tD;
    }
}

public class CG_TestingData
{
    public List<string> SessionLogs = new List<string>();
    public List<string> CorridorPieceLog = new List<string>();
    public int CorridorPieceChangeCount;
    public int SessionStartCount;
    public int SessionQuitCount;

    public CG_TestingData_Serialized Serialize()
    {
        CG_TestingData_Serialized tD = new CG_TestingData_Serialized();

        tD.SessionLogs = SessionLogs.ToArray();
        tD.CorridorPieceLog = CorridorPieceLog.ToArray();
        tD.CorridorPieceChangeCount = CorridorPieceChangeCount;
        tD.SessionStartCount = SessionStartCount;
        tD.SessionQuitCount = SessionQuitCount;

        return tD;
    }

    public IEnumerable<string> GetLogByLines(string logNumber, string currentSessionLog = null)
    {
        List<string> fullLog = new List<string>();

        //Add log intial details
        fullLog.Add("LOG #" + logNumber);
        fullLog.Add("======================");
        fullLog.Add("CorridorPieceChangeCount: " + CorridorPieceChangeCount);
        fullLog.Add("SessionStartCount: " + SessionStartCount);
        fullLog.Add("SessionQuitCount: " + SessionQuitCount);
        fullLog.Add("======================");

        if (!string.IsNullOrEmpty(currentSessionLog))
        {
            fullLog.Add("Current Session Logs:-");
            fullLog.Add(currentSessionLog);
            fullLog.Add("======================");
        }

        fullLog.Add("Session Logs:-");
        fullLog.AddRange(SessionLogs);
        fullLog.Add("======================");
        fullLog.Add("CorridorPieceLog:-");
        fullLog.AddRange(CorridorPieceLog);


        return fullLog;
    }
}