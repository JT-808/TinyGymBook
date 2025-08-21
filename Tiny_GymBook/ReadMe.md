# 1) SDK-Pfade setzen (Standard-Installationsort)
export ANDROID_SDK_ROOT="$HOME/Android/Sdk"
export ANDROID_HOME="$ANDROID_SDK_ROOT"
export ANDROID_AVD_HOME="$HOME/.android/avd"

# 2) Tools in den PATH packen
export PATH="$ANDROID_SDK_ROOT/emulator:$ANDROID_SDK_ROOT/platform-tools:$ANDROID_SDK_ROOT/cmdline-tools/latest/bin:$PATH"

# 3) Emulator starten (Kaltstart empfohlen)
emulator -avd Pixel_8 -no-snapshot-load


dotnet build -t:Run -f net9.0-android



neue namen

Liftlog
GymLog


dotnet clean \
&& rm -rf bin obj \
&& dotnet build


dotnet clean
                            dotnet build -c Debug
                            dotnet run -c Debug --framework net9.0-desktop --project Tiny_GymBook/Tiny_GymBook.csproj




dotnet run --project Tiny_GymBook/Tiny_GymBook.csproj --framework net9.0-desktop



