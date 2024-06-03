using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Knife.HDRPOutline.Core
{
    [VolumeComponentEditor(typeof(HDRPOutline))]
    public class HDRPOutlineEditor : VolumeComponentEditor
    {
        private SerializedDataParameter mode;
        private SerializedDataParameter width;
        private SerializedDataParameter fillAmount;

        private SerializedDataParameter patternTexture;
        private SerializedDataParameter patternTile;
        private SerializedDataParameter patternFillAmount;

        private SerializedDataParameter iterations;
        private SerializedDataParameter blurRadius;
        private SerializedDataParameter blurIterations;
        private SerializedDataParameter overglow;
        private SerializedDataParameter softnessEnabled;
        private SerializedDataParameter softness;

        private SerializedDataParameter singlePassInstanced;

        private HDRPOutline outline;

        private static Texture2D logo;

        public static Texture2D GetLogo()
        {
            if (logo == null)
            {
                logo = Resources.Load<Texture2D>("Textures/Knife-HDRP Outline/header");
            }

            return logo;
        }

        public override void OnEnable()
        {
            outline = target as HDRPOutline;
            var o = new PropertyFetcher<HDRPOutline>(serializedObject);

            mode = Unpack(o.Find(x => x.mode));
            width = Unpack(o.Find(x => x.width));
            fillAmount = Unpack(o.Find(x => x.fillAmount));

            patternTexture = Unpack(o.Find(x => x.patternTexture));
            patternTile = Unpack(o.Find(x => x.patternTile));
            patternFillAmount = Unpack(o.Find(x => x.patternFillAmount));

            iterations = Unpack(o.Find(x => x.iterations));
            blurRadius = Unpack(o.Find(x => x.blurRadius));
            blurIterations = Unpack(o.Find(x => x.blurIterations));
            overglow = Unpack(o.Find(x => x.overglow));
            softnessEnabled = Unpack(o.Find(x => x.softnessEnabled));
            softness = Unpack(o.Find(x => x.softness));
            singlePassInstanced = Unpack(o.Find(x => singlePassInstanced));
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label(GetLogo());
            PropertyField(mode);

            var outlineMode = (OutlineMode)mode.value.enumValueIndex;

            if (outlineMode != OutlineMode.Disabled)
            {
                PropertyField(fillAmount);
                PropertyField(patternTexture);
                if (outline.patternTexture.overrideState)
                {
                    PropertyField(patternTile);
                    PropertyField(patternFillAmount);
                }
            }
            switch (outlineMode)
            {
                case OutlineMode.Hard:
                    PropertyField(width);
                    PropertyField(iterations);
                    break;
                case OutlineMode.Soft:
                    PropertyField(blurRadius);
                    if(!Mathf.Approximately(blurRadius.value.floatValue, 0))
                        PropertyField(blurIterations);
                    PropertyField(overglow);
                    PropertyField(softnessEnabled);
                    if(softnessEnabled.value.boolValue)
                        PropertyField(softness);
                    break;
            }

            PropertyField(singlePassInstanced);
        }
    }
}
