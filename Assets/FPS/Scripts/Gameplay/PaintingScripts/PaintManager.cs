using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.FPS.Gameplay
{
    public class PaintManager : Singleton<PaintManager>
    {

        public Shader texturePaint;
        public Shader extendIslands;

        int prepareUVID = Shader.PropertyToID("_PrepareUV");
        int positionID = Shader.PropertyToID("_PainterPosition");
        int hardnessID = Shader.PropertyToID("_Hardness");
        int strengthID = Shader.PropertyToID("_Strength");
        int radiusID = Shader.PropertyToID("_Radius");
        int blendOpID = Shader.PropertyToID("_BlendOp");
        int colorID = Shader.PropertyToID("_PainterColor");
        int textureID = Shader.PropertyToID("_MainTex");
        int uvOffsetID = Shader.PropertyToID("_OffsetUV");
        int uvIslandsID = Shader.PropertyToID("_UVIslands");

        Material paintMaterial;
        Material extendMaterial;

        CommandBuffer command;

        public override void Awake()
        {
            base.Awake();

            paintMaterial = new Material(texturePaint);
            extendMaterial = new Material(extendIslands);
            command = new CommandBuffer();
            command.name = "CommmandBuffer - " + gameObject.name;
        }


        public float CalculatePaintedPercentage(Paintable paintable)
        {
            RenderTexture uvIsland = paintable.getUVIslands();
           
            RenderTexture activeRT = RenderTexture.active;

            RenderTexture mask = paintable.getMask();

            Texture2D maskTex = new Texture2D(mask.width, mask.height, TextureFormat.RFloat, false);
            Texture2D uvTex = new Texture2D(uvIsland.width, uvIsland.height, TextureFormat.RFloat, false);

            RenderTexture.active = mask;
            maskTex.ReadPixels(new Rect(0, 0, mask.width, mask.height), 0, 0);
            maskTex.Apply();

            RenderTexture.active = uvIsland;
            uvTex.ReadPixels(new Rect(0, 0, uvIsland.width, uvIsland.height), 0, 0);
            uvTex.Apply();

            RenderTexture.active = activeRT;

            int paintedPixels = 0;
            int totalPaintablePixels = 0;

            for (int y = 0; y < mask.height; y++)
            {
                for (int x = 0; x < mask.width; x++)
                {
                    float uvIslandPixel = uvTex.GetPixel(x, y).r;
                    if (uvIslandPixel > 0.1f)
                    {
                        totalPaintablePixels++;
                        float maskPixel = maskTex.GetPixel(x, y).r;
                        if (maskPixel > 0.1f)
                        {
                            paintedPixels++;
                        }
                    }
                }
            }
    
            float percent = (float)paintedPixels / totalPaintablePixels * 100f;
            return percent;
        }


        public void initTextures(Paintable paintable)
        {
            RenderTexture mask = paintable.getMask();
            RenderTexture uvIslands = paintable.getUVIslands();
            RenderTexture extend = paintable.getExtend();
            RenderTexture support = paintable.getSupport();
            Renderer rend = paintable.getRenderer();

            command.SetRenderTarget(mask);
            command.SetRenderTarget(extend);
            command.SetRenderTarget(support);

            paintMaterial.SetFloat(prepareUVID, 1);
            command.SetRenderTarget(uvIslands);
            command.DrawRenderer(rend, paintMaterial, 0);

            Graphics.ExecuteCommandBuffer(command);
            command.Clear();
        }


        public void paint(Paintable paintable, Vector3 pos, float radius = 1f, float hardness = .5f, float strength = .5f, Color? color = null)
        {
            RenderTexture mask = paintable.getMask();
            RenderTexture uvIslands = paintable.getUVIslands();
            RenderTexture extend = paintable.getExtend();
            RenderTexture support = paintable.getSupport();
            Renderer rend = paintable.getRenderer();

            paintMaterial.SetFloat(prepareUVID, 0);
            paintMaterial.SetVector(positionID, pos);
            paintMaterial.SetFloat(hardnessID, hardness);
            paintMaterial.SetFloat(strengthID, strength);
            paintMaterial.SetFloat(radiusID, radius);
            paintMaterial.SetTexture(textureID, support);
            paintMaterial.SetColor(colorID, color ?? Color.red);
            extendMaterial.SetFloat(uvOffsetID, paintable.extendsIslandOffset);
            extendMaterial.SetTexture(uvIslandsID, uvIslands);

            command.SetRenderTarget(mask);
            command.DrawRenderer(rend, paintMaterial, 0);

            command.SetRenderTarget(support);
            command.Blit(mask, support);

            command.SetRenderTarget(extend);
            command.Blit(mask, extend, extendMaterial);

            Graphics.ExecuteCommandBuffer(command);
            command.Clear();
        }

        public Dictionary<string, float> CalculatePaintedPercentageByColor(Paintable paintable)
        {
            RenderTexture support = paintable.getSupport();
            RenderTexture activeRT = RenderTexture.active;

            Texture2D tex = new Texture2D(support.width, support.height, TextureFormat.RGB24, false);
            RenderTexture.active = support;
            tex.ReadPixels(new Rect(0, 0, support.width, support.height), 0, 0);
            tex.Apply();
            RenderTexture.active = activeRT;

            Color[] pixels = tex.GetPixels();

            int totalPainted = 0;
            int red = 0, blue = 0, green = 0, yellow = 0;

            foreach (Color pixel in pixels)
            {
                if (pixel.maxColorComponent < 0.1f) continue; // Siyah veya boyanmam��sa atla

                totalPainted++;

                if (IsApproximately(pixel, Color.red)) red++;
                else if (IsApproximately(pixel, Color.blue)) blue++;
                else if (IsApproximately(pixel, Color.green)) green++;
                else if (IsApproximately(pixel, Color.yellow)) yellow++;
            }

            Dictionary<string, float> colorRatios = new Dictionary<string, float>();
            if (totalPainted > 0)
            {
                colorRatios["Red"] = (float)red / totalPainted * 100f;
                colorRatios["Blue"] = (float)blue / totalPainted * 100f;
                colorRatios["Green"] = (float)green / totalPainted * 100f;
                colorRatios["Yellow"] = (float)yellow / totalPainted * 100f;
            }

            return colorRatios;
        }

        bool IsApproximately(Color a, Color b, float tolerance = 0.1f)
        {
            return Mathf.Abs(a.r - b.r) < tolerance &&
                   Mathf.Abs(a.g - b.g) < tolerance &&
                   Mathf.Abs(a.b - b.b) < tolerance;
        }

    }
}
