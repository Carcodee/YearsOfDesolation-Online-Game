using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SETUtil.ResourceLoader;
using System.Net.NetworkInformation;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KrisDevelopment.DistributedInternalUtilities
{

    public static class BugReporting
	{

		private static void OpenBugReporting()
        {
			Application.OpenURL(CommonURLs.BUG_REPORT_URL);
		}

		public static void ToolbarBugReportButton()
		{
			GUIContent _bugContent;
#if UNITY_EDITOR
			_bugContent = new GUIContent(EditorTextureResource.Get("kd_bug_report_icon"));
#else
			_bugContent = new GUIContent("[!]");
#endif
			_bugContent.tooltip = "Report a bug!";

			GUIStyle _style = new GUIStyle("Button");
			
#if UNITY_EDITOR
			_style = EditorStyles.toolbarButton;
#else
			SETUtil.EditorUtil.BeginColorPocket(new Color(1, 0.5f, 0.5f, 1));
#endif
			if (GUILayout.Button(_bugContent, _style, GUILayout.ExpandWidth(false), GUILayout.Width(28)))
			{
				OpenBugReporting();
			}

#if !UNITY_EDITOR
			SETUtil.EditorUtil.EndColorPocket();
#endif
		}

		public static void SmallBugReportButton()
		{
			GUIContent _bugContent;
#if UNITY_EDITOR
			_bugContent = new GUIContent(EditorTextureResource.Get("kd_bug_report_icon"));
			_bugContent.text = " Report a bug!";
#else
			_bugContent = new GUIContent("Report a bug!");
#endif
			GUIStyle _style = new GUIStyle("Button");

#if UNITY_EDITOR
			_style = EditorStyles.miniButton;
#endif

			if (GUILayout.Button(_bugContent, _style, GUILayout.ExpandWidth(false), GUILayout.Height(24)))
			{
				OpenBugReporting();
			}
		}

		public static void StandardBugReportButton()
		{
			GUIContent _bugContent;
#if UNITY_EDITOR
			_bugContent = new GUIContent(EditorTextureResource.Get("kd_bug_report_icon"));
			_bugContent.text = " Report a bug!";
#else
			_bugContent = new GUIContent("Report a bug!");
#endif

			if (GUILayout.Button(_bugContent, GUILayout.ExpandWidth(false), GUILayout.Height(19)))
			{
				OpenBugReporting();
			}
		}
	}
}
