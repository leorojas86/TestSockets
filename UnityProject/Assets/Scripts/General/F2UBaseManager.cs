using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Project: Flash to Unity
// File: F2UManagersBase
// Created by Leonardo Rojas.
// Copyright 2014 Fair Play Labs. All rights reserved.
/// \author Leonardo Rojas
/// \date 2014
/// \copyright 2014 Fair Play Labs. All rights reserved.
/// <summary>
/// The base class for all mono behaviour managers.
/// </summary>
public class F2UBaseManager : MonoBehaviour
{	
	#region Variables

	/// <summary>
	/// Determines whether the animation has already exited.
	/// </summary>
	protected static bool _applicationHasQuitted = false;
	
	#endregion
	
	#region Methods

	protected static T InstantiateSingleton<T>() where T : F2UBaseManager
	{
		if(!_applicationHasQuitted && Application.isPlaying)//Can instantiate singleton
		{
			GameObject newGameObject = new GameObject(typeof(T).Name);
			T instance				 = newGameObject.AddComponent<T>();
			GameObject.DontDestroyOnLoad(newGameObject);

			return instance;
		}

		return null;
	}
	
	public virtual T CreateNewManagerInstance<T>() where T : MonoBehaviour
	{
		GameObject newGameObject 	   = new GameObject(typeof(T).Name);
		newGameObject.transform.parent = transform;
		
		return newGameObject.AddComponent<T>();
	}

	/// <summary>
	/// Method that executes whenever the application quits.
	/// </summary>
	protected virtual void OnApplicationQuit()
	{
		GameObject.DestroyImmediate(gameObject, true);
		_applicationHasQuitted = true;
	}
	
	#endregion
}
