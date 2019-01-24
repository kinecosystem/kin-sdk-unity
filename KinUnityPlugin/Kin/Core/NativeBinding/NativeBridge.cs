namespace Kin
{
	/// <summary>
	/// convenience helper class to deal with returning a proper INativeBridge for each supported platform
	/// </summary>
	static class NativeBridge
	{
		/// <summary>
		/// fetches the platform specific INativeBridge via it's Instance property. This prevents intantiating multiple objects
		/// when we really need only a single INativeBrige.
		/// </summary>
		/// <returns>The get.</returns>
		static internal INativeBridge Get()
		{
#if UNITY_ANDROID && !UNITY_EDITOR
			return NativeBridgeAndroid.Instance;
#elif UNITY_IOS && !UNITY_EDITOR
			return NativeBridgeIos.Instance;
#else
			return NativeBridgeEditor.Instance;
#endif
		}
	}
}