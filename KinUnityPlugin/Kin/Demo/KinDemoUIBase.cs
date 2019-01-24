using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Kin
{
	/// <summary>
	/// internal helper class to make Unity GUI look a bit nicer and to clean up the KinDemoUI scene to make it more legible
	/// </summary>
	public abstract class KinDemoUIBase : MonoBehaviour
	{
		/// <summary>
		/// toggles log viewer mode
		/// </summary>
		protected bool _isInLogViewerMode;

		/// <summary>
		/// indicates an async request is in flight, displaying a modal for the duration of the request
		/// </summary>
		bool _isRequestInFlight;

		/// <summary>
		/// status text for the current in flight request
		/// </summary>
		string _currentRequest;

		/// <summary>
		/// the scroll position of the log viewer
		/// </summary>
		Vector2 _logScrollPosition;

		/// <summary>
		/// caches all logs for the log viewer
		/// </summary>
		/// <returns></returns>
		StringBuilder _logBuilder = new StringBuilder();

		/// <summary>
		/// in flight request window background
		/// </summary>
		Texture2D _progressWindowBackground;

		/// <summary>
		/// background texture for buttons
		/// </summary>
		Texture2D _normalBackground;

		/// <summary>
		/// background texture for active (depressed) buttons
		/// </summary>
		Texture2D _activeBackground;

		/// <summary>
		/// background texture for bottom aligned buttons
		/// </summary>
		Texture2D _bottomButtonBackground;

		/// <summary>
		/// is this device a retina device?
		/// </summary>
		bool _isRetina;

		/// <summary>
		/// is this a really big retina device (most likely a tablet)?
		/// </summary>
		bool _isRetinaIpad;

		/// <summary>
		/// height of a button. Varies based on dpi.
		/// </summary>
		float _buttonHeight = 30;

		/// <summary>
		/// font size for buttons. Varies based on dpi.
		/// </summary>
		int _buttonFontSize = 15;

		/// <summary>
		/// width of a column
		/// </summary>
		float _columnWidth;

		/// <summary>
		/// padding applied around the edges of the screen and around button t4ext
		/// </summary>
		const float _guiPadding = 10;

		/// <summary>
		/// gap between columns
		/// </summary>
		const float _guiColumnGap = 15;


		#region MonoBehaviour

		/// <summary>
		/// setup our textures and precalculate some sizing info
		/// </summary>
		public virtual void Awake()
		{
			CreateGuiTextures();
			Application.logMessageReceived += handleLog;

			_isRetina = Screen.width >= 960 || Screen.height >= 960;
			_isRetinaIpad = Screen.height >= 2048 || Screen.width >= 2048;

			if( _isRetina )
			{
				if( _isRetinaIpad )
				{
					_buttonHeight = 140;
					_buttonFontSize = 40;
				}
				else
				{
					_buttonHeight = 70;
					_buttonFontSize = 25;
				}
			}
		}


		/// <summary>
		/// cleans up our log handler
		/// </summary>
		void OnDestroy()
		{
			Application.logMessageReceived -= handleLog;
		}


		/// <summary>
		/// sets up GUI areas and handles displaying regular and edit modes
		/// </summary>
		void OnGUI()
		{
			PrepGuiSkin();

			GUILayout.BeginArea( new Rect( _guiPadding, GetNotchOffset() + _guiPadding, _columnWidth, Screen.height ) );
			GUILayout.BeginVertical();

			if( _isInLogViewerMode )
				OnLeftColumnEditGUI();
			else
				OnLeftColumnGUI();

			GUILayout.EndVertical();
			GUILayout.EndArea();


			GUILayout.BeginArea( new Rect( Screen.width - _columnWidth - _guiPadding, GetNotchOffset() + _guiPadding, _columnWidth, Screen.height ) );
			GUILayout.BeginVertical();

			if( _isInLogViewerMode )
				OnRightColumnEditGUI();
			else
				OnRightColumnGUI();

			GUILayout.EndVertical();
			GUILayout.EndArea();

			if( _isRequestInFlight )
				GUI.ModalWindow( 2, new Rect( 20, 20, Screen.width - 40, Screen.height - 40 ), PaintProgressWindow, "Async Request In Flight" );
		}

		#endregion


		#region Overridable onGUI methods

		protected virtual void OnLeftColumnGUI()
		{ }


		protected virtual void OnRightColumnGUI()
		{ }


		protected virtual void OnLeftColumnEditGUI()
		{
			var logWindowTitle = "Log Console";
			GUILayout.Window( 1, new Rect( 0, GetNotchOffset(), Screen.width, Screen.height - _buttonHeight - 15 ), PaintLogWindow, logWindowTitle );
		}


		protected virtual void OnRightColumnEditGUI()
		{
			if( BottomButton( "Hide Logs" ) )
			{
				ToggleEditMode();
			}
		}

		#endregion


		#region Log Handler and GUI

		void handleLog( string logString, string stackTrace, LogType type )
		{
			_logBuilder.AppendFormat( "{0}\n", logString );
		}

		void PaintLogWindow( int id )
		{
			GUI.skin.label.alignment = TextAnchor.UpperLeft;
			GUI.skin.label.fontSize = _buttonFontSize;

			_logScrollPosition = GUILayout.BeginScrollView( _logScrollPosition );

			// add a clear button
			if( GUILayout.Button( "Clear Console" ) )
				_logBuilder.Remove( 0, _logBuilder.Length );

			GUILayout.Label( _logBuilder.ToString() );

			GUILayout.EndScrollView();
		}

		#endregion


		/// <summary>
		/// toggles edit mode, allowing the editing of ad units and app ids
		/// </summary>
		protected void ToggleEditMode()
		{
			_isInLogViewerMode = !_isInLogViewerMode;
		}


		/// <summary>
		/// creates a button on the bottom-right of the screen
		/// </summary>
		/// <param name="text"></param>
		/// <param name="useBottomButtonStyle"></param>
		/// <returns></returns>
		protected bool BottomButton( string text, bool useBottomButtonStyle = true )
		{
			if( useBottomButtonStyle )
			{
				GUI.skin.button.hover.background = _bottomButtonBackground;
				GUI.skin.button.normal.background = _bottomButtonBackground;
			}

			var retVal = GUI.Button( new Rect( 0, Screen.height - GetNotchOffset() - _buttonHeight - _guiPadding * 2, _columnWidth, _buttonHeight ), text );

			if( useBottomButtonStyle )
			{
				GUI.skin.button.hover.background = _normalBackground;
				GUI.skin.button.normal.background = _normalBackground;
			}

			return retVal;
		}


		/// <summary>
		/// creates some 1x1 pixel Textures that we use to style the buttons with
		/// </summary>
		void CreateGuiTextures()
		{
			Color normalColor;
			ColorUtility.TryParseHtmlString( "#a2b2e1", out normalColor );

			_normalBackground = new Texture2D( 1, 1 );
			_normalBackground.SetPixel( 0, 0, normalColor );
			_normalBackground.Apply();

			_activeBackground = new Texture2D( 1, 1 );
			_activeBackground.SetPixel( 0, 0, Color.yellow );
			_activeBackground.Apply();

			_bottomButtonBackground = new Texture2D( 1, 1 );
			_bottomButtonBackground.SetPixel( 0, 0, Color.Lerp( Color.gray, Color.black, 0.5f ) );
			_bottomButtonBackground.Apply();

			_progressWindowBackground = new Texture2D( 1, 1 );
			_progressWindowBackground.SetPixel( 0, 0, new Color( 0, 0, 0, 0.9f ) );
			_progressWindowBackground.Apply();
		}



		/// <summary>
		/// does a best effort to see if we've got an iphone X with a notch
		/// </summary>
		/// <returns></returns>
		int GetNotchOffset()
		{
#if UNITY_IOS
			var isProbablyPortrait = Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.FaceUp;
			if( isProbablyPortrait && UnityEngine.iOS.Device.generation.ToString().ToLower().Contains( "iphonex" ) )
				return 80;
#endif

			return 0;
		}


		#region GUI Skin Config

		/// <summary>
		/// this method just makes the Unity default GUI look a little bit nicer
		/// </summary>
		protected void PrepGuiSkin()
		{
			_columnWidth = ( Screen.width / 2 ) - 15;

			GUI.skin.button.fontSize = _buttonFontSize;
			GUI.skin.button.margin = new RectOffset( 0, 0, (int)_guiPadding, 0 );
			GUI.skin.button.stretchWidth = true;
			GUI.skin.button.fixedHeight = _buttonHeight;
			GUI.skin.button.wordWrap = false;
			GUI.skin.button.hover.background = _normalBackground;
			GUI.skin.button.normal.background = _normalBackground;
			GUI.skin.button.active.background = _activeBackground;
			GUI.skin.button.normal.textColor = Color.white;
			GUI.skin.button.active.textColor = Color.black;

			GUI.skin.label.normal.textColor = Color.white;
			GUI.skin.label.fontSize = _buttonFontSize;

			GUI.skin.textField.fontSize = _buttonFontSize + 20;
			GUI.skin.textField.fixedHeight = _buttonFontSize + 20 + _guiPadding;

			GUI.skin.window.normal.background = _progressWindowBackground;
		}

		#endregion


		#region Modal Window

		protected void ShowProgressWindow( string text )
		{
			_isRequestInFlight = true;
			_currentRequest = text + " request in progress...";
		}


		protected void HideProgressWindow()
		{
			_isRequestInFlight = false;
		}


		void PaintProgressWindow( int id )
		{
			var centeredStyle = GUI.skin.GetStyle( "Label" );
			centeredStyle.alignment = TextAnchor.UpperCenter;
			centeredStyle.fontSize = 30;
			GUI.Label( new Rect( 0, Screen.height / 2 - 25, Screen.width, 50 ), _currentRequest, centeredStyle );
		}

		#endregion
	}
}