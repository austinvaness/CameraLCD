namespace avaness.CameraLCD
{
    public class CameraLCDSettings
    {
        private bool _steal = true;
        public bool steal
        {
            get
            {
                return _steal && ratio > 1;
            }
            set
            {
                _steal = value;
            }
        }
        public bool Enabled = true;
        public int ratio = 2;
        public bool Shadows = false;
        public bool BillboardsStatic = false;
        public bool BillboardsDynamic = false;
        public bool Flares = false;
        public bool Fxaa = false;
        public bool Bloom = false;
        public bool UpdateLOD = false;
        public bool SpriteFix = true;
        public bool ShowDebug = false;
        public bool HeadFix = true;
        public int Range = 40;

    }

}