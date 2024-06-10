// IKP - by Hristo Ivanov (Kris Development)

using UnityEngine;

namespace IKPn
{
	public static class IKPStyle
	{
		public static Texture2D warningIcon
		{
			get
			{
				if (!iconsInitialized || !m_iconWarning)
					LoadIcons();
				return m_iconWarning;
			}
		}

		public static Texture2D logo
		{
			get
			{
				if (!iconsInitialized || !m_logo)
					LoadIcons();
				return m_logo;
			}
		}

		public static Texture2D genericIcon
		{
			get
			{
				if (!iconsInitialized || !m_genericIcon)
					LoadIcons();
				return m_genericIcon;
			}
		}

		public static Texture2D blendMachineIcon
		{
			get
			{
				if (!iconsInitialized || !m_blendMachineIcon)
					LoadIcons();
				return m_blendMachineIcon;
			}
		}

		public static Texture2D xIcon
		{
			get
			{
				if (!iconsInitialized || !m_xIcon)
					LoadIcons();
				return m_xIcon;
			}
		}

		public static Texture2D addIcon
		{
			get
			{
				if (!iconsInitialized || !m_addIcon)
					LoadIcons();
				return m_addIcon;
			}
		}

		public static Texture2D moveIcon
		{
			get
			{
				if (!iconsInitialized || !m_moveIcon)
					LoadIcons();
				return m_moveIcon;
			}
		}

		public static Texture2D refreshIcon
		{
			get
			{
				if (!iconsInitialized || !m_refreshIcon)
					LoadIcons();
				return m_refreshIcon;
			}
		}

		public static Texture2D editIcon
		{
			get
			{
				if (!iconsInitialized || !m_editIcon)
					LoadIcons();
				return m_editIcon;
			}
		}

		public static Texture2D recycleIcon
		{
			get
			{
				if (!iconsInitialized || !m_recycleIcon)
					LoadIcons();
				return m_recycleIcon;
			}
		}

		private static Texture2D
			m_iconWarning,
			m_logo,
			m_genericIcon,
			m_blendMachineIcon,
			m_xIcon,
			m_addIcon,
			m_moveIcon,
			m_refreshIcon,
			m_editIcon,
			m_recycleIcon;

		private static bool iconsInitialized = false;

		public static int
			SMALL_HEIGHT = 16,
			MEDIUM_HEIGHT = 20,
			BIG_HEIGHT = 25;

		public static float NODE_TRANSPARENCY = 0.82f;

		public static Color
			COLOR_DISABLED = new Color(0.8f, 0.5f, 0.5f),
			COLOR_ACTIVE = new Color(0.5f, 0.6f, 1f),
			COLOR_RESET = new Color(1f, 0.4f, 0.4f), //reset button
			COLOR_HIGHTLIGHT_BOX = new Color(1f, 1f, 0.3f), //info box
			COLOR_GREY = new Color(0.7f, 0.7f, 0.7f), //greyed out module 
			COLOR_TARGET = new Color(0.9f, .9f, .9f), //target properties inspector box 
			COLOR_NODE_BG = new Color(0.15f, 0.20f, 0.23f), //node background 
			COLOR_NODE_LINE = new Color(.95f, .95f, .95f), //node connecting line 
			COLOR_NODE_BGLINE = COLOR_NODE_BG * 0.5f, //node connecting line 
			COLOR_NODE_ANIM = new Color(1f, 1f, 1f, NODE_TRANSPARENCY), //animation node
			COLOR_NODE_EVENT = new Color(1f, .77f, .25f, NODE_TRANSPARENCY), //event machine node
			COLOR_NODE_SWITCH = new Color(.2f, .5f, 1f, NODE_TRANSPARENCY); //state switch node

		static void LoadIcons()
		{
			string _dir = "IKP/";
			m_logo = (Texture2D) Resources.Load(_dir + "ikp_logo");
			m_iconWarning = (Texture2D) Resources.Load(_dir + "ikp_warning_icon");
			m_genericIcon = (Texture2D) Resources.Load(_dir + "ikp_generic_icon");
			m_blendMachineIcon = (Texture2D) Resources.Load(_dir + "ikp_machine_window_icon");
			m_xIcon = (Texture2D) Resources.Load(_dir + "ikp_x_icon");
			m_addIcon = (Texture2D) Resources.Load(_dir + "ikp_add_icon");
			m_moveIcon = (Texture2D) Resources.Load(_dir + "ikp_move_icon");
			m_refreshIcon = (Texture2D) Resources.Load(_dir + "ikp_refresh_icon");
			m_editIcon = (Texture2D) Resources.Load(_dir + "ikp_edit_icon");
			m_recycleIcon = (Texture2D) Resources.Load(_dir + "ikp_recycle_icon");

			iconsInitialized = true;
		}
	}
}
