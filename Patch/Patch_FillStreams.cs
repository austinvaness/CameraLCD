using System.Threading;

namespace avaness.CameraLCDRevived.Patch
{
    /* Fix a crash to do with foliage rendering on planet */
    public static class Patch_FillStreams
    {
        public static bool Prefix(object __instance)
        {
            return false;
            //Interlocked.MemoryBarrier();
            //return !CameraLCD.rendering;
        }
    }

}