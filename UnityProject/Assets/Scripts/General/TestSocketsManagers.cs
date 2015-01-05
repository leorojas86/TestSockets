public class TestSocketsManagers : F2UBaseManager
{	
	#region Variables
	
	/// <summary>
	/// <see cref="F2UManagers" />'s singleton instance.
	/// </summary>
	private static TestSocketsManagers _instance = null;
	
	#endregion
	
	#region Properties
	
	/// <summary>
	/// Gets the current <see cref="F2UManagers" />'s instance.
	/// </summary>
	public static TestSocketsManagers Instance
	{
		get
		{
			if(_instance != null)
				return _instance;
			
			_instance = InstantiateSingleton<TestSocketsManagers>();
			return _instance;
		}
	}
	
	#endregion
}