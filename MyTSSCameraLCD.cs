using Sandbox.Game.GameSystems.TextSurfaceScripts;
using VRageMath;

namespace avaness.CameraLCDRevived
{
    [MyTextSurfaceScript("TSS_CAMLCD", "Camera Display")]
    public class MyTSSCameraLCD : MyTSSCommon
    {
        public override ScriptUpdate NeedsUpdate
        {
            get
            {
                return ScriptUpdate.Update100;
            }
        }

        public MyTSSCameraLCD(Sandbox.ModAPI.Ingame.IMyTextSurface surface, VRage.Game.ModAPI.IMyCubeBlock block, Vector2 size) : base(surface, block, size)
        {

        }

        public override void Run()
        {
            base.Run();
        }

        static MyTSSCameraLCD()
        {
        }
    }

}