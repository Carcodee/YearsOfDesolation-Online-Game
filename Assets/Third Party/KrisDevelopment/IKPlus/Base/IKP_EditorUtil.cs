// IKP - by Hristo Ivanov (Kris Development)

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using TransformData = SETUtil.Types.TransformData;

namespace IKPn
{ //standard IKP namespace

    public enum GizmoPalette
    {
        White,
        Red,
        Green,
        Blue,
        Yellow,
        Pink
    }

    /// <summary>
    /// editor drawing & visualization utilities
    /// </summary>
    public static class IKPEditorUtils
    {
        public static void PaintBone(Vector3 pointA, Vector3 pointB)
        {
#if UNITY_EDITOR
            PaintBone(pointA, pointB, GizmoPalette.White, false);
#endif
        }

        public static void PaintBone(Vector3 pointA, Vector3 pointB, GizmoPalette g)
        {
#if UNITY_EDITOR
            PaintBone(pointA, pointB, g, false);
#endif
        }

        public static void PaintBone(Vector3 pointA, Vector3 pointB, Color c)
        {
#if UNITY_EDITOR
            PaintBone(pointA, pointB, c, false);
#endif
        }

        public static void PaintBone(Vector3 pointA, Vector3 pointB, GizmoPalette g, bool drawEnd)
        {
#if UNITY_EDITOR
            PaintBone(pointA, pointB, GetGizmoPalette(g), false);
#endif
        }

        public static void PaintBone(Vector3 pointA, Vector3 pointB, Color c, bool drawEnd)
        {
#if UNITY_EDITOR
            const float _gizmoScreenSize = 0.006f;

            float _gizmoRadiusA = 0.1f;
            float _gizmoRadiusB = 0.1f;

            var _currentSceneView = SceneView.currentDrawingSceneView;
            if (_currentSceneView != null)
            {
                var _cameraDistanceA = Vector3.Distance(pointA, _currentSceneView.camera.transform.position);
                _gizmoRadiusA = _cameraDistanceA * _gizmoScreenSize;

                var _cameraDistanceB = Vector3.Distance(pointB, _currentSceneView.camera.transform.position);
                _gizmoRadiusB = _cameraDistanceB * _gizmoScreenSize;
            }


            Color _gizmoColor = c;

            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireSphere(pointA, _gizmoRadiusA);

            if (drawEnd)
            {
                Gizmos.DrawWireSphere(pointB, _gizmoRadiusB);
            }

            Gizmos.DrawLine(pointA, pointB);
            Gizmos.color = Color.white;
#endif
        }

        private static Color GetGizmoPalette(GizmoPalette gizmoPal)
        {
#if UNITY_EDITOR
            switch (gizmoPal)
            {
                case GizmoPalette.Red: return new Color(1f, 0.15f, 0.15f);
                case GizmoPalette.Green: return new Color(0.15f, 1f, 0.15f);
                case GizmoPalette.Blue: return new Color(0.1f, 0.6f, 1f);
                case GizmoPalette.Yellow: return new Color(1f, 1f, 0.15f);
                case GizmoPalette.Pink: return new Color(1f, 0.15f, 0.9f);
                case GizmoPalette.White: return new Color(.6f, .6f, .6f);
            }
#endif
            return Color.white;
        }


#if UNITY_EDITOR
        public static void DrawTargetGUI(SerializedProperty property, string label = IKPUtils.NONE_PROPERTY_NAME)
        {
            if (property == null || property.serializedObject == null)
            {
                Debug.LogError("IKPEditorUtils.DrawTargetGUI: property is null");
                return;
            }

            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();

            if (label == IKPUtils.NONE_PROPERTY_NAME)
                label = property.displayName;

            SerializedProperty
                _targ_targetMode = property.FindPropertyRelative(IKPTarget.PROPERTY_NAME_TargetMode),
                _targ_targetPos = property.FindPropertyRelative(IKPTarget.PROPERTY_NAME_TargetPos),
                _targ_relativeTo = property.FindPropertyRelative(IKPTarget.PROPERTY_NAME_RelativeTo),
                _targ_targetObj = property.FindPropertyRelative(IKPTarget.PROPERTY_NAME_TargetObj);

            GUI.color = IKPStyle.COLOR_TARGET;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_targ_targetMode);
            if (_targ_targetMode.enumValueIndex == (int)IKPTargetMode.Position)
            {
                _targ_targetPos.vector3Value = EditorGUILayout.Vector3Field("Target Position", _targ_targetPos.vector3Value);
                EditorGUILayout.PropertyField(_targ_relativeTo);
            }
            else if (_targ_targetMode.enumValueIndex == (int)IKPTargetMode.Transform)
            {
                EditorGUILayout.PropertyField(_targ_targetObj);
            }
            EditorGUILayout.EndVertical();

            property.serializedObject.ApplyModifiedProperties();
        }
#endif

        //debug utilities
        public static void DrawDebug(Vector3 pos, IKPLocalSpace lsp)
        {
            Debug.DrawRay(pos, lsp.right, Color.red);
            Debug.DrawRay(pos, lsp.up, Color.green);
            Debug.DrawRay(pos, lsp.forward, Color.blue);
        }

        public static void DrawDebug(Vector3 pos, Quaternion quat)
        {
            Debug.DrawRay(pos, quat * Vector3.right, Color.red);
            Debug.DrawRay(pos, quat * Vector3.up, Color.green);
            Debug.DrawRay(pos, quat * Vector3.forward, Color.blue);
        }

        public static void DrawDebug(TransformData tr)
        {
            Debug.DrawRay(tr.position, tr.right, Color.red);
            Debug.DrawRay(tr.position, tr.up, Color.green);
            Debug.DrawRay(tr.position, tr.forward, Color.blue);
        }

        public static void DrawDebug(Transform tr)
        {
            Debug.DrawRay(tr.position, tr.right, Color.red);
            Debug.DrawRay(tr.position, tr.up, Color.green);
            Debug.DrawRay(tr.position, tr.forward, Color.blue);
        }

        public static void DrawDebugCube(Vector3 pivotPoint)
        {
            DrawDebugCube(pivotPoint, Color.magenta);
        }

        public static void DrawDebugCube(Vector3 pivotPoint, Color _color, float time = 0.1f)
        {
            Debug.DrawLine(pivotPoint + new Vector3(-0.5f, -0.5f, -0.5f), pivotPoint + new Vector3(-0.5f, -0.5f, 0.5f), _color, time, false);
            Debug.DrawLine(pivotPoint + new Vector3(-0.5f, -0.5f, 0.5f), pivotPoint + new Vector3(0.5f, -0.5f, 0.5f), _color, time, false);
            Debug.DrawLine(pivotPoint + new Vector3(0.5f, -0.5f, 0.5f), pivotPoint + new Vector3(0.5f, -0.5f, -0.5f), _color, time, false);
            Debug.DrawLine(pivotPoint + new Vector3(0.5f, -0.5f, -0.5f), pivotPoint + new Vector3(-0.5f, -0.5f, -0.5f), _color, time, false);
            Debug.DrawLine(pivotPoint + new Vector3(-0.5f, 0.5f, -0.5f), pivotPoint + new Vector3(-0.5f, 0.5f, 0.5f), _color, time, false);
            Debug.DrawLine(pivotPoint + new Vector3(-0.5f, 0.5f, 0.5f), pivotPoint + new Vector3(0.5f, 0.5f, 0.5f), _color, time, false);
            Debug.DrawLine(pivotPoint + new Vector3(0.5f, 0.5f, 0.5f), pivotPoint + new Vector3(0.5f, 0.5f, -0.5f), _color, time, false);
            Debug.DrawLine(pivotPoint + new Vector3(0.5f, 0.5f, -0.5f), pivotPoint + new Vector3(-0.5f, 0.5f, -0.5f), _color, time, false);
            Debug.DrawLine(pivotPoint + new Vector3(-0.5f, -0.5f, -0.5f), pivotPoint + new Vector3(-0.5f, 0.5f, -0.5f), _color, time, false);
            Debug.DrawLine(pivotPoint + new Vector3(-0.5f, -0.5f, 0.5f), pivotPoint + new Vector3(-0.5f, 0.5f, 0.5f), _color, time, false);
            Debug.DrawLine(pivotPoint + new Vector3(0.5f, -0.5f, 0.5f), pivotPoint + new Vector3(0.5f, 0.5f, 0.5f), _color, time, false);
            Debug.DrawLine(pivotPoint + new Vector3(0.5f, -0.5f, -0.5f), pivotPoint + new Vector3(0.5f, 0.5f, -0.5f), _color, time, false);
        }


#if UNITY_EDITOR
        public static bool CanEditGameObjectState(GameObject gameObject, out string error)
        {
            return SETUtil.SceneUtil.CanEditGameObjectState(gameObject, out error);
        }

        internal static void DrawBonesArraySerializedProperty(SerializedProperty array, bool alwaysExpanded = false)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (!alwaysExpanded)
                {
                    array.isExpanded = EditorGUILayout.Toggle(array.isExpanded, EditorStyles.foldout, GUILayout.Width(14));
                }

                GUILayout.Label(new GUIContent(string.Format("{0} ({1})", array.displayName, array.arraySize)));

                if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                {
                    array.arraySize++;
                }

                EditorGUI.BeginDisabledGroup(array.arraySize <= 0);
                {
                    if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                    {
                        array.arraySize--;
                    }
                }
                EditorGUI.EndDisabledGroup();

                array.arraySize = EditorGUILayout.DelayedIntField(array.arraySize, GUILayout.Width(80));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            const int PROPERTY_LIMIT = 200;

            if (array.isExpanded || alwaysExpanded)
            {
                for (int i = 0; i < array.arraySize && i < PROPERTY_LIMIT; i++)
                {
                    GUILayout.BeginHorizontal();
                    {
                        DrawBoneSerializedProperty(array.GetArrayElementAtIndex(i), i);
                        if (GUILayout.Button(new GUIContent("X", "Remove"), EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                        {
                            array.DeleteArrayElementAtIndex(i);
                            EditorGUIUtility.ExitGUI();
                        }

                    }
                    GUILayout.EndHorizontal();
                }
            }
            EditorGUI.indentLevel--;
        }

        private static void DrawBoneSerializedProperty(SerializedProperty serializedProperty, int index)
        {
            const string _transformPropertyName = Bone.TRANSFORM_PROPERTY_NAME;
            EditorGUILayout.PropertyField(serializedProperty.FindPropertyRelative(_transformPropertyName), new GUIContent($"Bone {index}"));
        }
#endif
    }
}
