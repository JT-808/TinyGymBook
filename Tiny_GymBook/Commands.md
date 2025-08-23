# Testen

## Linux:

dotnet clean \
&& rm -rf bin obj \
&& dotnet build

dotnet run --project Tiny_GymBook/Tiny_GymBook.csproj --framework net9.0-desktop

oder

dotnet clean
dotnet build -c Debug
dotnet run -c Debug --framework net9.0-desktop --project Tiny_GymBook/Tiny_GymBook.csproj

## Android:

### 1) SDK-Pfade setzen (Standard-Installationsort)

export ANDROID_SDK_ROOT="$HOME/Android/Sdk"
export ANDROID_HOME="$ANDROID_SDK_ROOT"
export ANDROID_AVD_HOME="$HOME/.android/avd"

### 2) Tools in den PATH packen

export PATH="$ANDROID_SDK_ROOT/emulator:$ANDROID_SDK_ROOT/platform-tools:$ANDROID_SDK_ROOT/cmdline-tools/latest/bin:$PATH"

### 3) Emulator starten (Kaltstart empfohlen)

emulator -avd Pixel_8 -no-snapshot-load

dotnet build -t:Run -f net9.0-android

---

# releases

## Linux: Binary

dotnet publish Tiny_GymBook/Tiny_GymBook.csproj -f net9.0-desktop -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true

## Android APK:

dotnet build Tiny_GymBook/Tiny_GymBook.csproj \
 -f net9.0-android -c Release -t:SignAndroidPackage \
 -p:AndroidPackageFormat=apk \
 -p:RunAOTCompilation=false \
 -p:AndroidLinkMode=None \
 -p:PublishTrimmed=false \
 -p:JavaSdkDirectory=$HOME/.dotnet/jdk/17

#### Installieren

adb install -r Tiny_GymBook/bin/Release/net9.0-android/\*-Signed.apk


## Windows:

dotnet publish Tiny_GymBook/Tiny_GymBook.csproj \
 -f net9.0-desktop -c Release -r win-x64 \
 --self-contained false -p:PublishSingleFile=false \
 --no-restore

mit Dotnet
dotnet publish Tiny_GymBook/Tiny_GymBook.csproj \
 -f net9.0-desktop -c Release -r win-x64 \
 --self-contained true \
 -p:PublishSingleFile=true \
 -p:IncludeNativeLibrariesForSelfExtract=true \
 -p:EnableCompressionInSingleFile=true \
 --no-restore

neue namen

Liftlog
RepLog
