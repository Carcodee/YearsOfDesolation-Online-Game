﻿////////////////////////////////////////
//    Shared Editor Tool Utilities    //
//    by Kris Development             //
////////////////////////////////////////

//License: MIT
//GitLab: https://gitlab.com/KrisDevelopment/SETUtil

using System;
using System.Collections;
using System.Collections.Generic;
using U = UnityEngine;
using Gl = UnityEngine.GUILayout;

#if UNITY_EDITOR
using E = UnityEditor;
using EGl = UnityEditor.EditorGUILayout;

namespace SETUtil.EditorOnly
{
	///<summary> Intended to provide some form of asynchronous task support in the Unity Editor </summary>
	public class EditorCoroutine 
	{
		private static List<EditorCoroutine> coroutines = new List<EditorCoroutine>();

		private static List<EditorCoroutine> removeQueue = new List<EditorCoroutine>();

		private Stack<IEnumerator> stack = new Stack<IEnumerator>();
		private bool paused = false;
		private IEnumerator current = null;
		
		private EditorCoroutine (IEnumerator enumerator)
		{
			this.stack.Push(enumerator);
			this.paused = false;
		}

		private void Update ()
		{
			if(paused){
				return;
			}
			
			if(current == null)
            {
				current = stack.Pop();
			}

			var _canMoveNext = current.MoveNext();

			// check if the coroutine yields a nested coroutine
			if (_canMoveNext)
			{
				if (current.Current is IEnumerator child)
				{
					stack.Push(current);
					current = child;
				}
            }
            else
            {
				current = null;
            }
			
			if (!_canMoveNext && stack.Count == 0){
				removeQueue.Add(this);
            }
		}

		public void Pause ()
		{
			paused = true;
		}

		public void Resume ()
		{
			paused = false;
			Update();
		}

		public static EditorCoroutine Start (IEnumerator enumerator)
		{
			var _coroutine = new EditorCoroutine(enumerator);
			coroutines.Add(_coroutine);

			E.EditorApplication.update -= UpdateCoroutines;
			E.EditorApplication.update += UpdateCoroutines;

			UpdateCoroutines();

			return _coroutine;
		}

		public static void Stop (EditorCoroutine coroutine)
		{
			removeQueue.Add(coroutine);
			UpdateCoroutines();
		}

		private static void UpdateCoroutines ()
		{
			// Remove finished coroutines
			foreach(var toRemove in removeQueue){
				coroutines.Remove(toRemove);
			}
			removeQueue.Clear();

			// Update all active coroutines
			foreach(var coroutine in coroutines){
				coroutine.Update();
			}

			// Unsubscribe if there is nothing to do
			if(coroutines.Count == 0){
				E.EditorApplication.update -= UpdateCoroutines;
			}
		}

		public static IEnumerator Wait (float seconds)
        {
			var _startWait = DateTime.Now;
            while (DateTime.Now - _startWait < TimeSpan.FromSeconds(seconds))
            {
				yield return null;
            }
        }
	}
}

#endif