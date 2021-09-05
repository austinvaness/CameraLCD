namespace avaness.CameraLCDRevived.Patch
{
    public static class Patch_HighlightsRun
    {
        public static bool Prefix()
        {
            return !CameraLCD.rendering;
        }
    }

}