using System;
using VRage.Utils;

namespace avaness.CameraLCDRevived
{
    public class Logger
    {
        public void Log(string msg)
        {
            MyLog.Default.WriteLine("[CameraLCD] " + msg);
        }
    }
}