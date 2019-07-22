FROM gableroux/unity3d:2018.3.14f1-android
# install android sdk
RUN touch /root/.android/repositories.cfg \
    && /opt/android-sdk-linux/tools/bin/sdkmanager --install tools "platforms;android-28" "build-tools;28.0.3" "extras;android;m2repository" "platform-tools"
RUN export PATH="/opt/android-sdk-linux/platform-tools/:$PATH"
# install genymotion cloud tool gmsaas
# first install pip3
RUN apt-get update -y && apt-get install -y python3-pip
# install gmsaas
RUN pip3 install gmsaas
RUN gmsaas config set android-sdk-path /opt/android-sdk-linux/