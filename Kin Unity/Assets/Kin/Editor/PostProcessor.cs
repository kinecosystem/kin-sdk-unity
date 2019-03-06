using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.Collections;
using System.IO;
using System.Linq;


namespace Kin
{
	static class PostProcessor
	{
		static void ManuallyRunPostProcessor()
		{
			var buildPath = Path.Combine( Directory.GetCurrentDirectory(), "Xcode" );
			OnPostProcessBuild( BuildTarget.iOS, buildPath );
		}


		[PostProcessBuild]
		public static void OnPostProcessBuild( BuildTarget buildTarget, string buildPath )
		{
			if( buildTarget == BuildTarget.iOS )
			{
				var projPath = Path.Combine( buildPath, "Unity-iPhone.xcodeproj/project.pbxproj" );
				var proj = new PBXProject();
				proj.ReadFromFile( projPath );

				var targetGuid = proj.ProjectGuid();

				// copy our bridging header into the root of the project so Xcode will find it
				var file = Directory.GetFiles( Application.dataPath, "KinPlugin-Bridging-Header.h", SearchOption.AllDirectories ).First();
				var dest = Path.Combine( buildPath, Path.GetFileName( file ) );
				if(!File.Exists(dest))
					File.Copy( file, dest );

				//proj.SetBuildProperty( targetGuid, "ENABLE_BITCODE", "NO" );
				proj.SetBuildProperty( targetGuid, "SWIFT_OBJC_BRIDGING_HEADER", "KinPlugin-Bridging-Header.h" );
				proj.SetBuildProperty( targetGuid, "SWIFT_OBJC_INTERFACE_HEADER_NAME", "KinPlugin-Generated-Swift.h" );
				proj.SetBuildProperty( targetGuid, "SWIFT_VERSION", "4.2" );
				proj.SetBuildProperty( targetGuid, "CLANG_ENABLE_MODULES", "YES" );
				proj.AddBuildProperty( targetGuid, "LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks $(inherited)" );
				proj.AddBuildProperty( targetGuid, "FRAMERWORK_SEARCH_PATHS", "$(inherited) $(PROJECT_DIR) $(PROJECT_DIR)/Frameworks" );
				proj.AddBuildProperty( targetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES" );

				proj.WriteToFile( projPath );
			}
		}
	}
}
