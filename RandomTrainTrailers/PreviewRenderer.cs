using ColossalFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RandomTrainTrailers
{
    public struct VehicleRenderInfo
    {
        public VehicleInfo VehicleInfo { get; set; }
        public bool Inverted { get; set; }

        public VehicleRenderInfo(VehicleInfo vehicleInfo, bool inverted)
        {
            VehicleInfo = vehicleInfo ?? throw new ArgumentNullException(nameof(vehicleInfo));
            Inverted = inverted;
        }
    }

    public class PreviewRenderer : MonoBehaviour
    {
        private Camera m_camera;
        private float m_rotation = 120f;
        private float m_zoom = 3f;

        public PreviewRenderer()
        {
            m_camera = new GameObject("Camera").AddComponent<Camera>();
            m_camera.transform.SetParent(transform);
            m_camera.backgroundColor = new Color(0, 0, 0, 0);
            m_camera.fieldOfView = 30f;
            m_camera.nearClipPlane = 1f;
            m_camera.farClipPlane = 1000f;
            m_camera.allowHDR = true;
            m_camera.enabled = false;
            m_camera.targetTexture = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32);
            m_camera.pixelRect = new Rect(0f, 0f, 512, 512);
            m_camera.clearFlags = CameraClearFlags.Color;
        }

        public Vector2 size
        {
            get { return new Vector2(m_camera.targetTexture.width, m_camera.targetTexture.height); }
            set
            {
                if (size != value)
                {
                    m_camera.targetTexture = new RenderTexture((int)value.x, (int)value.y, 24, RenderTextureFormat.ARGB32);
                    m_camera.pixelRect = new Rect(0f, 0f, value.x, value.y);
                }
            }
        }

        public RenderTexture texture
        {
            get { return m_camera.targetTexture; }
        }

        public float cameraRotation
        {
            get { return m_rotation; }
            set { m_rotation = value % 360f; }
        }

        public float zoom
        {
            get { return m_zoom; }
            set
            {
                m_zoom = Mathf.Clamp(value, 0.5f, 5f);
            }
        }

        public void RenderVehicle(VehicleInfo info)
        {
            RenderVehicle(new VehicleRenderInfo[] { new VehicleRenderInfo { VehicleInfo = info } }, info.m_color0, false);
        }

        public void RenderVehicle(IList<VehicleRenderInfo> infos)
        {
            RenderVehicle(infos, infos[0].VehicleInfo.m_color0, false);
        }

        public void RenderVehicle(IList<VehicleRenderInfo> vehicles, Color color, bool useColor = true)
        {
            InfoManager infoManager = Singleton<InfoManager>.instance;
            InfoManager.InfoMode currentMod = infoManager.CurrentMode;
            InfoManager.SubInfoMode currentSubMod = infoManager.CurrentSubMode; ;
            infoManager.SetCurrentMode(InfoManager.InfoMode.None, InfoManager.SubInfoMode.Default);
            infoManager.UpdateInfoMode();

            Light sunLight = DayNightProperties.instance.sunLightSource;
            float lightIntensity = sunLight.intensity;
            Color lightColor = sunLight.color;
            Vector3 lightAngles = sunLight.transform.eulerAngles;

            sunLight.intensity = 2f;
            sunLight.color = Color.white;
            sunLight.transform.eulerAngles = new Vector3(50, 180, 70);

            Light mainLight = RenderManager.instance.MainLight;
            RenderManager.instance.MainLight = sunLight;

            if(mainLight == DayNightProperties.instance.moonLightSource)
            {
                DayNightProperties.instance.sunLightSource.enabled = true;
                DayNightProperties.instance.moonLightSource.enabled = false;
            }

            // Calculate the positions of each vehicle
            var positions = new List<Vector3>(vehicles.Count);
            var totalLength = 0f;
            var totalWidth = 0f;
            var totalHeight = 0f;
            var prevPos = Vector3.zero;
            var prevOffset = 0f;

            foreach (var vehicle in vehicles)
            {
                var size = vehicle.VehicleInfo.m_generatedInfo.m_size;
                var offset = vehicle.Inverted ? -vehicle.VehicleInfo.m_attachOffsetBack : -vehicle.VehicleInfo.m_attachOffsetFront;
                offset += size.z * 0.5f;
                var newPos = prevPos + Vector3.back * prevOffset + Vector3.back * offset;
                positions.Add(newPos);

                prevPos = newPos;
                prevOffset = vehicle.Inverted ? -vehicle.VehicleInfo.m_attachOffsetFront : -vehicle.VehicleInfo.m_attachOffsetBack;
                prevOffset += size.z * 0.5f;

                // Keep track of total vehicle size
                totalLength += size.z;
                if (size.y > totalHeight)
                    totalHeight = size.y;
                if (size.x > totalWidth)
                    totalWidth = size.x;
            }

            // Shift all positions so we are centered
            var centerOffset = new Vector3(0, -totalHeight / 2, totalLength / 2);

            // Set up the camera
            float magnitude = new Vector3(totalWidth, totalHeight, totalLength).magnitude / 2;
            float num = magnitude + 16f;
            float num2 = magnitude * m_zoom;
            m_camera.transform.position = Vector3.forward * num2;
            m_camera.transform.rotation = Quaternion.AngleAxis(180, Vector3.up);
            m_camera.nearClipPlane = Mathf.Max(num2 - num * 1.5f, 0.01f);
            m_camera.farClipPlane = num2 + num * 1.5f;

            // Render the vehicles
            var shader = vehicles[0].VehicleInfo.m_material.shader;
            for (var i = 0; i < vehicles.Count; i++)
            {
                var info = vehicles[i].VehicleInfo;
                var inverted = vehicles[i].Inverted;

                Vector3 one = Vector3.one;
                Quaternion rotation = Quaternion.Euler(20f, 0f, 0f) * Quaternion.Euler(0f, m_rotation, 0f);
                Vector3 position = rotation * (positions[i] + centerOffset);

                VehicleManager instance = Singleton<VehicleManager>.instance;
                if (inverted)
                    rotation = rotation * Quaternion.Euler(0, 180f, 0);
                Matrix4x4 matrixBody = Matrix4x4.TRS(position, rotation, Vector3.one);
                Matrix4x4 matrixTyre = info.m_vehicleAI.CalculateTyreMatrix(Vehicle.Flags.Created, ref position, ref rotation, ref one, ref matrixBody);

                MaterialPropertyBlock materialBlock = instance.m_materialBlock;
                materialBlock.Clear();
                materialBlock.SetMatrix(instance.ID_TyreMatrix, matrixTyre);
                materialBlock.SetVector(instance.ID_TyrePosition, Vector3.zero);
                materialBlock.SetVector(instance.ID_LightState, Vector3.zero);
                if (useColor) materialBlock.SetColor(instance.ID_Color, color);

                instance.m_drawCallData.m_defaultCalls = instance.m_drawCallData.m_defaultCalls + 1;

                info.m_material.SetVectorArray(instance.ID_TyreLocation, info.m_generatedInfo.m_tyres);
                Graphics.DrawMesh(info.m_mesh, matrixBody, info.m_material, 0, m_camera, 0, materialBlock, true, true);
            }

            m_camera.RenderWithShader(shader, "");

            sunLight.intensity = lightIntensity;
            sunLight.color = lightColor;
            sunLight.transform.eulerAngles = lightAngles;

            RenderManager.instance.MainLight = mainLight;

            if (mainLight == DayNightProperties.instance.moonLightSource)
            {
                DayNightProperties.instance.sunLightSource.enabled = false;
                DayNightProperties.instance.moonLightSource.enabled = true;
            }

            infoManager.SetCurrentMode(currentMod, currentSubMod);
            infoManager.UpdateInfoMode();
        }
    }
}