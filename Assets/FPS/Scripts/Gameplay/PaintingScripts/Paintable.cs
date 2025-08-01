using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.FPS.Gameplay
{
    public class Paintable : MonoBehaviour
    {
        const int TEXTURE_SIZE = 1024;

        public float extendsIslandOffset = 1;

        RenderTexture extendIslandsRenderTexture;
        RenderTexture uvIslandsRenderTexture;
        RenderTexture maskRenderTexture;
        RenderTexture supportTexture;

        Renderer rend;

        int maskTextureID = Shader.PropertyToID("_MaskTexture");

        public RenderTexture getMask() => maskRenderTexture;
        public RenderTexture getUVIslands() => uvIslandsRenderTexture;
        public RenderTexture getExtend() => extendIslandsRenderTexture;
        public RenderTexture getSupport() => supportTexture;
        public Renderer getRenderer() => rend;

        void Start()
        {
            maskRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
            maskRenderTexture.filterMode = FilterMode.Bilinear;

            extendIslandsRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
            extendIslandsRenderTexture.filterMode = FilterMode.Bilinear;

            uvIslandsRenderTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
            uvIslandsRenderTexture.filterMode = FilterMode.Bilinear;

            supportTexture = new RenderTexture(TEXTURE_SIZE, TEXTURE_SIZE, 0);
            supportTexture.filterMode = FilterMode.Bilinear;

            rend = GetComponent<Renderer>();
            rend.material.SetTexture(maskTextureID, extendIslandsRenderTexture);

            PaintManager.instance.initTextures(this);
        }

        void OnDisable()
        {
            maskRenderTexture.Release();
            uvIslandsRenderTexture.Release();
            extendIslandsRenderTexture.Release();
            supportTexture.Release();
        }

        private void Update()
        {
            if (Keyboard.current.gKey.wasPressedThisFrame)
            {
                if (!gameObject.activeInHierarchy)
                    return;

                float percent = PaintManager.instance.CalculatePaintedPercentage(this);
                Debug.Log("Boyanma Oran�: " + percent + "%");

            }

            if (Keyboard.current.hKey.wasPressedThisFrame)
            {
                var ratios = PaintManager.instance.CalculatePaintedPercentageByColor(this);
                foreach (var kvp in ratios)
                {
                    Debug.Log($"{kvp.Key} ile boyanma oran�: {kvp.Value}%");
                }
            }
        }
    }
}

