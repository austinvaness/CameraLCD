using HarmonyLib;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using VRage.Game.Entity;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Interfaces;
using VRageMath;
using VRageRender;
using VRageRender.Messages;

namespace avaness.CameraLCDRevived.Patch
{
    //[HarmonyPatch(typeof(MyRenderComponentScreenAreas))]
    //[HarmonyPatch("CreateTexture")]
    public static class Patch_CreateTexture
    {
        public static bool Prefix(MyRenderComponentScreenAreas __instance, int area, Vector2I textureSize, MyTextPanelComponent textComponent = null)
        {
            CameraLCD.log.Log("CreateTexture");
            //SECEF.log.Log("CreateTexture. bm: " + SECEF.videoBitmap + " Size: " + SECEF.videoBitmap.Width + "data: ");
            MyEntity m_entity = (MyEntity)AccessTools.Field(typeof(MyRenderComponentScreenAreas), "m_entity").GetValue(__instance);

            long id = m_entity.EntityId;
            string textureName = __instance.GenerateOffscreenTextureName(m_entity.EntityId, area);
            DisplayId displayId = new DisplayId(id, area);
            CameraLCD.log.Log(DateTime.Now + " Set texturename " + textureName + " x/y: " + textureSize + " id: " + displayId);
            if (CameraLCD.displays.ContainsKey(displayId))
            {
                CameraLCD.log.Log("Found existing display " + displayId);
                Display b = CameraLCD.displays[displayId];
                //MyRenderProxy.UnloadTexture(textureName);
                b.textureName = textureName;
                //b.area = area;
                MyRenderProxy.CreateGeneratedTexture(b.textureName, textureSize.X * b.resMult, textureSize.Y * b.resMult, MyGeneratedTextureType.RGBA_Linear, 1, b.videoData, false);

                return false;
            }
            CameraLCD.log.Log(DateTime.Now + " Set texturename " + textureName + " x/y: " + textureSize + "id: " + displayId);
            Display display = new Display(displayId, textureName, textureSize.X, textureSize.Y);
            MyRenderProxy.CreateGeneratedTexture(display.textureName, textureSize.X * display.resMult, textureSize.Y * display.resMult, MyGeneratedTextureType.RGBA_Linear, 1, display.videoData, false);

            CameraLCD.AddDisplay(displayId, display);
            MyTerminalBlock myTerminal = (MyTerminalBlock)m_entity;
            display.entity = myTerminal;
            myTerminal.CustomDataChanged -= UpdateCustomData;
            myTerminal.CustomDataChanged += UpdateCustomData;
            if (myTerminal.CustomData.Length > 0)
            {
                UpdateCustomData(myTerminal);
            }
            //MyRenderProxy.CreateGeneratedTexture(__instance.GenerateOffscreenTextureName(m_entity.EntityId, area), (int)SECEF.videoSize.X, (int)SECEF.videoSize.Y, MyGeneratedTextureType.RGBA, 1, data, true);
            return false;
        }
        public static void UpdateCustomData(MyTerminalBlock __instance)
        {
            //CameraLCD.log.Log("setcustomdata " + value + " " + __instance + " / " + (__instance is MyCameraBlock));
            string value = __instance.CustomData;
            CameraLCD.log.Log("instance: " + __instance + " / " + __instance.GetType());
            MyTerminalBlock entity = __instance;
            int found = 0;
            DisplayId myId = new DisplayId();
            string[] lines = new string[0];
            MyCommandLine cmd = new MyCommandLine();
            if (value.Length > 0)
            {
                lines = value.Split('\n');
            }
            foreach (string line in lines)
            {

                found = 0;
                value = line.Trim();
                CameraLCD.log.Log("processing line: " + line);


                int fov = 0;
                int range = 0;
                float brightness = 0;
                float contrast = 0;
                long entityId = 0;
                if ((__instance is IMyTextPanelProvider || __instance is Sandbox.ModAPI.Ingame.IMyTextSurfaceProvider) && value.Length > 0)
                {
                    if (value[1] == ':')
                    {
                        if (int.TryParse(value.Substring(0, 1), out int area))
                        {
                            CameraLCD.log.Log("got area id " + area);
                            myId = new DisplayId(entity.EntityId, area);
                        }
                        else
                        {
                            myId = new DisplayId(entity.EntityId, 0);
                        }
                        value = value.Substring(2);
                    }
                    else
                    {
                        myId = new DisplayId(entity.EntityId, 0);
                    }
                    if (value[0] == '-')
                    {
                        cmd.Clear();
                        cmd.TryParse(line);
                        if (cmd.ArgumentCount == 0)
                        {
                            continue;
                        }
                        if (cmd.Switches.Contains("fov"))
                        {

                            int.TryParse(cmd.Switch("fov", 0), out fov);
                            CameraLCD.log.Log("have fov switch " + fov);
                        }
                        if (cmd.Switches.Contains("range"))
                        {
                            int.TryParse(cmd.Switch("range", 0), out range);

                        }
                        if (cmd.Switches.Contains("brightness"))
                        {
                            float.TryParse(cmd.Switch("brightness", 0), out brightness);

                        }
                        if (cmd.Switches.Contains("contrast"))
                        {
                            float.TryParse(cmd.Switch("contrast", 0), out contrast);

                        }
                        if (cmd.Switches.Contains("name"))
                        {
                            value = cmd.Switch("name", 0).Trim();
                            CameraLCD.log.Log("Camera name " + value);
                        }
                        else if (cmd.Switches.Contains("entity"))
                        {
                            long.TryParse(cmd.Switch("entity", 0), out entityId);
                        }
                    }
                    if (value.Length == 0)
                    {
                        CameraLCD.log.Log("Missing camera -name ");
                        continue;
                    }
                    /*if (value.Substring(0, 5) == "!fov=")
                    {
                        int.TryParse(value.Substring(5, value.IndexOf(' ') - 5), out fov);
                        if (value.Length > value.IndexOf(' ') + 1)
                        {
                            value = value.Substring(value.IndexOf(' ') + 1);
                        }
                        CameraLCD.log.Log(value + " " + fov);
                    }*/

                    //foreach (var block in entity.CubeGrid.GetFatBlocks())
                    MyEntity targetEntity = entityId != 0 ? MyEntities.GetEntityById(entityId) : null;
                    if (targetEntity != null && targetEntity is IMyCameraController)
                    {
                        if (!((IMyCameraBlock)targetEntity).HasPlayerAccess(entity.OwnerId))
                        {
                            CameraLCD.log.Log("no access to " + targetEntity);
                            return;
                        }
                        if (CameraLCD.displays.ContainsKey(myId))
                        {
                            if (fov != 0)
                            {
                                CameraLCD.displays[myId].SetFov(fov);
                            }
                            if (range != 0)
                            {
                                CameraLCD.displays[myId].SetRange(range);
                            }
                            if (brightness != 0)
                            {
                                CameraLCD.displays[myId].brightness = brightness;
                            }
                            if (contrast != 0)
                            {
                                CameraLCD.displays[myId].contrast = contrast;
                            }
                            CameraLCD.displays[myId].SetSource(targetEntity);
                            return;
                        }
                    }
                    foreach (MyTerminalBlock block in entity.CubeGrid.GridSystems.TerminalSystem.Blocks)
                    {
                        //CameraLCD.log.Log("checking.beep. " + block);
                        if (block is IMyCameraController && block.DisplayNameText.Trim().ToLower() == value.Trim().ToLower())
                        {
                            CameraLCD.log.Log("found " + block + " " + block.EntityId);
                            if (CameraLCD.displays.ContainsKey(myId))
                            {
                                if (fov != 0)
                                {
                                    CameraLCD.displays[myId].SetFov(fov);
                                }
                                if (range != 0)
                                {
                                    CameraLCD.displays[myId].SetRange(range);
                                }
                                if (brightness != 0)
                                {
                                    CameraLCD.displays[myId].brightness = brightness;
                                }
                                if (contrast != 0)
                                {
                                    CameraLCD.displays[myId].contrast = contrast;
                                }
                                CameraLCD.displays[myId].SetSource(block);
                                found++;
                                break;
                            }
                        }
                    }
                }
                if (found == 0 && CameraLCD.displays.ContainsKey(myId))
                {
                    CameraLCD.displays[myId].SetSource(null);
                }
            }


        }
        public static bool Prefix_ReleaseTexture(MyRenderComponentScreenAreas __instance, int area)
        {
            MyEntity m_entity = (MyEntity)AccessTools.Field(typeof(MyRenderComponentScreenAreas), "m_entity").GetValue(__instance);
            if (m_entity == null)
            {
                CameraLCD.log.Log("null entity in releasetexture");
            }
            long id = m_entity.EntityId;
            DisplayId myId = new DisplayId(id, area);
            CameraLCD.log.Log("Releasing " + __instance + " " + area);
            if (CameraLCD.displays.ContainsKey(myId))
            {
                if (CameraLCD.displays[myId].source != null)
                {
                    ((MyTerminalBlock)CameraLCD.displays[myId].source).CustomDataChanged -= UpdateCustomData;
                }
                CameraLCD.RemoveDisplay(myId);
                CameraLCD.log.Log("removed display " + myId);

                /*MyTextPanelComponent mtpc = AccessTools.Field(typeof(MyTextPanel), "m_activePanelComponent").GetValue(m_entity) as MyTextPanelComponent;
                if(mtpc != null)
                {
                    CameraLCD.log.Log("update after sim" +mtpc);
                    mtpc.UpdateAfterSimulation(true, false);
                    mtpc.UpdateAfterSimulation(true, true);
                } else
                {
                    CameraLCD.log.Log("null mtpc in releasetexture");
                }*/
                //CameraLCD.displays.Remove(new Vector2(id, area));
            }
            return true;
        }
        //static byte[] _rgbValues = new byte[4194304];
        public static void GetBGRValues(Bitmap bmp, ref byte[] rgbValues)
        {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

            int rowBytes = bmpData.Width * Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
            int imgBytes = bmp.Height * rowBytes;
            //byte[] rgbValues = new byte[imgBytes];
            //byte[] rgbValues = new byte[4194304];

            IntPtr ptr = bmpData.Scan0;
            Marshal.Copy(ptr, rgbValues, 0, imgBytes);
            bmp.UnlockBits(bmpData);
            //return rgbValues;
        }
    }

}