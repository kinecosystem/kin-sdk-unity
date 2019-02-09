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
		[MenuItem( "KIN/GO" )]
		static void test()
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

				proj.WriteToFile( projPath );
			}
		}
	}
}
