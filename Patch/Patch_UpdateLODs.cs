namespace avaness.CameraLCDRevived.Patch
{
    public static class Patch_UpdateLODs
    {
        public static bool Prefix(object __instance)
        {
            if (CameraLCD.rendering)
            {
                return false;
            }
            return true;
        }
    }

}