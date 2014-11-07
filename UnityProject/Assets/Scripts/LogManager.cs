using UnityEngine;
using System.Collections;

public class LogManager 
{
	#region Variables

	private static LogManager _instance = null;

	private string _log = string.Empty;

	#endregion

	#region Properties

	public static LogManager Instance
	{
		get 
		{
			if(_instance != null)
				return _instance;

			_instance = new LogManager();
			return _instance; 
		}
	}

	public string Log
	{
		get { return _log; }
	}

	#endregion

	#region Methods

	public void LogMessage(string message)
	{
		_log += "\n Message: " + message;
	}

	#endregion
}
