Builds against Unity 2019.4.18f1

# Step 1 - build kin android 
- With Gradle 6.8 - run gradle at the root of kin android to buid artifacts.  The build process puts output into.
  `'../../Kin Unity/Assets/Kin/Editor/NativeCode/Android'` as an AAR.

# Step 2 - Copy Kun Unity Into Project
- Copy `project settings`

# Step 3 - Import assets and package manifests
- From inside unity, add the kin assets folder to the project.
- From inside unity, add the packages folder contents into packages.

# Step 4 - (Optional) Import the tutorial scripts
- If you plan to follow the tutorial, you can follow tutorial article 3, but use the scripts in this repo.
  They are slightly modified to accomadate unity package changes.

# Step 5 - In Unity Settings, Player Settings -> Publish Settings
- Enable: Custom Main Gradle Template
- Enable: Custom Launcher Gradle Template
- Enable: Custom Base Gradle Teplate 
- Enable: Custom Gradle Properties Template

- Once You enable these settings, save the project, close unity and re-open 

# Step 6 - Cross check your build files
- The unity build files are crucial, and unity is a bit finicky with overwriting them upgrading them, etc
- One you enable all the settings, restart unity, etc -
    Cross check all the files from inside Unity in `Assets->Plugins->Android`
    - baseProjectTemplate.gradle
    - gradleTemplate.properties
    - launcherTemplate.gradle
    - mainTemplate.gradle
