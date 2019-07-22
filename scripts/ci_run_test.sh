#!/usr/bin/env bash
set -x

TEST_RESULT_FILE="/root/project/test_results.xml"

# copy unity license file
mkdir /root/.local/share/unity3d/Unity
cp /root/project/Unity_license.ulf /root/.local/share/unity3d/Unity/Unity_lic.ulf

# login to gmsaas (genymotion tool), and create device instance for tests
gmsaas auth login $GENY_USERNAME "$GENY_PASSWORD"
instance_id=$(gmsaas instances start 107d757e-463a-4a18-8667-b8dec6e4c87e ci_test)
gmsaas instances adbconnect $instance_id

# extract the port adb is connected to the remote device 
adb_port=$(adb devices |grep localhost: | grep -o -E '[0-9]+')
# use it 
adb -s "localhost:$adb_port" forward "tcp:34999" "localabstract:Unity-com.UnityTestRunner.UnityTestRunner"

# run unity test with remote device (use xvfb to create required virtual screen)
${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' /opt/Unity/Editor/Unity} \
-projectPath /root/project/Kin\ Unity/ \
-runTests -buildTarget Android -testPlatform android -logfile \
-batchmode -testResults $TEST_RESULT_FILE
UNITY_EXIT_CODE=$?

if [ $UNITY_EXIT_CODE -eq 0 ]; then
  echo "Run succeeded, no failures occurred";
elif [ $UNITY_EXIT_CODE -eq 2 ]; then
  echo "Run succeeded, some tests failed";
  cat $TEST_RESULT_FILE
elif [ $UNITY_EXIT_CODE -eq 3 ]; then
  echo "Run failure (other failure)";
  cat $TEST_RESULT_FILE
else
  echo "Unexpected exit code $UNITY_EXIT_CODE";
  cat $TEST_RESULT_FILE
fi

cat $TEST_RESULT_FILE |grep test-run | grep "result="
gmsaas instances stop $instance_id
exit $UNITY_EXIT_CODE