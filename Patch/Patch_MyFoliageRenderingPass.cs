namespace avaness.CameraLCDRevived.Patch
{
    public static class Patch_MyFoliageRenderingPass
    {
        public static bool Prefix(object __instance)
        {
            //return false;
            if (CameraLCD.IsRenderFrame() && CameraLCD.HasNextDisplay())
            {
                return false;
            }
            return true;
        }
    }

}