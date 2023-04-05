# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).


## [3.2.0] - 2023-04-05
### Added
- Support for Unity silicon editor.
- New menu option: `Zappar/Editor/New AR Scene &N`. Which creates a new scene with `ZapparCamera` without any skybox and flat lighting setup for the scene.
- Provided favicon.ico as part of WebGLTemplate to avoid 404 error on start in browsers.

### Changed
- Updated `ZapparFaceMesh` to use advanced mesh API for native GFX updates respective to each platform.
- Updated `ZapparBackgroundCamera` `ClearFlags` from Skybox to SolidColor, for better mobile performance.
- Zappar in-editor publishing menu now uses WebGL 2.0 graphics API on Unity version 2021 LTS and above.
- Zappar in-editor publishing menu updates WebGL `Texture Compression` to `ETC2` in favor of mobile browsers.
- Removed `Decompression Fallback` as the default option from UAR WebGL publishing setting. Support for both `Brotli` and `Gzip` compressed builds are now fully supported from `Zapworks` [dashboard](https://my.zap.works/) and `zapworks-cli` version [2.0.4](https://www.npmjs.com/package/@zappar/zapworks-cli) and above.
- Improved WebGL build caching by enabling `WebGL.nameFilesAsHashes` from the publishing menu option, and caching zbin and zpt file types on Unity 2020 and above.
- Auto link required frameworks (`Accelerate` and `OpenGLES`) in Unity built iOS Xcode project.
- Moved the menu option `Zappar/Utilities/Full Head Depth Mask` to `Zappar/Face Tracker/Face Depth Mask`.

### Known issues
- Selecting OpenGL graphics API for windows standalone crashes the Editor. The default graphics API of Direct3D11 should work fine.
- Color space mismatch between Unity and ZCV resulting in darker camera texture with Metal API on MacOS and iOS native. A temporary fix is provided in the thread [here](https://github.com/zappar-xr/universal-ar-unity/issues/11).



## [3.1.1] - 2023-03-27
### Changed
- Patch release, to fix rear camera selection in Safari. For iOS version 16.4 it seems to have been changed to use ultrawide lens instead of normal camera.


## [3.1.0] - 2022-07-28
### Added
- Cylindrical and conical image training and tracking target type.
- New Zappar camera type of gyro (3DoF tracking) which only updates the virtual camera's attitude and optional backdrop of camera feed.
- New menu options to add additional zappar packages for webgl - `Save and Share` and `Video Recorder`.
- Editor option to exclude *.zpt files from WebGL build to minimize the build size.
- Culling property for `Zappar/UnlitTextureUV` shader, which allows off/front/back culling.

### Changed
- Zappar Instant Tracking to use new world tracking implementation.
- Reduced the Zappar Camera near clipping plane to 0.01 from 0.3.

### Known issues
- No support for Unity silicon editor.


## [3.0.1] - 2022-01-04
### Fixed
- ZapparFaceMesh is not visible when kept disabled on start. This was due to delay in receiving mesh data (verts, tris, etc.) immediately after initialization.

### Known issues
- Dark camera background texture on native iOS and Mac with Gamma color space. [Track Issue](https://github.com/zappar-xr/universal-ar-unity/issues/11)

## [3.0.0] - 2021-12-20
### Added
- Multiple face tracking.
- Switch between front and rear camera at runtime using `SwitchToFrontCameraMode` and `SwitchToRearCameraMode` respectively.
- Toggle the active state of Zappar Camera to pause and unpause. Use `ToggleActiveCamera` method in ZapparCamera.cs.

### Changed
- New flow for adding Face tracking target and subsequent anchors.
- Removed zappar prefabs from the Resources folder to optimize the build size. Instead, now use Zappar editor menu to create Zappar obects in scene.
- Improved anchor placement for Instant tracker with permissible range for Z-movement before the anchor placement. Enable this behaviour by checking the flag `MoveAnchorOnZ`.
- Replaced `MirrorRearCameras` and `MirrorUserCamera` under `ZapparCamera.cs` with `MirrorCamera`.
- Moved ZapparReflectionProbe from ZaparCamera object context menu to Editor Menu.
- Zappar Menu order in Editor has been updated.

### Fixed
- Zappar tracking target to register with Zappar camera pipeline whenever set to active, rather than having to enabled it along with ZapparCamera or keeping it active at the start of the scene.


## [2.0.0] - 2021-11-19
### Added
- Support for facial landmarks in face tracking.
- UAR setting to control zappar permission UI prompt in WebGL.

### Changed
- Instant update of image preview in editor for image tracking target.
- Renamed several variables across the SDK. Updating the package will result in loosing any previously set references in editor or used in scripts.
- Updated ZapparInstantTrackingTarget to expose `UserHasPlaced` flag and additional method to place and reset virtual anchors (`PlaceTrackerAnchor`/`ResetTrackerAnchor`).

### Fixed
- Nullable IntPtr for zappar trackers and pipeline.
- Image Trainer overwrite check before starting the training process.
- UV mapping of preview texture on Safari.
- Missing Pointer_stringify in WebGL Build.
- Unity InputSystem failed to capture inputs on few mobile browsers after permissions popup.
- Missing APIs for Face Landmark in windows editor.

## [1.2.1] - 2021-10-15
### Fixed
- iPad loading issue.
- support WebGL development build. Fixed emcc error of missing graphics API.
- blurry canvas on retina and HiDPI mobile browsers

## [1.2.0] - 2021-09-22
### Added
- Zappar image training in editor.
- Renamed WebGL templates to - `Zappar` (for Unity version 2020 and above) and `Zappar2019` (for Unity version 2019.x).
- Exempt additional IP addresses ranging from 172.16.* through 172.32.* from licensing check.

### Fixed
- White screen issue after loading progress on Safari browsers.
- Fix to update the face mesh view in editor (while toggling between full head mesh).
- Fix referencing image preview when project GUID has been updated. [Note: finds reference using gameobject name and meshfilter component]
- Fix camera pause/resume when switching to other tabs and back.
- Improve HTML template CSS to better fit the full browser window.
- Issues with other/embedded browsers on iOS, including social browsers.

### Known issues
 - Sometimes the app freezes on iOS browser while switching between tabs. To workaround this make sure to check - `Run In Background` setting from `Project Settings > Player > Resolution And Presentation`.

## [1.1.1-preview.1] - 2021-07-14
### Added
- Camera-based realtime reflection

### Fixed
- WebGL runtime error for Unity2020 build due to unsupported compression setting.
- Unity WebGL template path wasn't updating to Zappar2020 on `Zappar/Editor/UpdateProjectSettingsToPublish`.
- Dialog pop-up when enabling SRP for Zappar package and Unity Universal Rendering Package is not imported in project.
- Editor view for Zappar Face Mesh.

### Known issues
 - Missing UAR APIs error when taking development build on WebGl platform.

## [1.1.0-preview.2] - 2021-07-05
### Added
- Support for Unity Scriptable Pipeline
- Capturing ZCV log in editor/file for debugging
- Universal AR project level editor settings object - Log options and image target preview in scene
- Extra sample scene for multi-image tracking

### Fixed
- Image tracker preview on windows editor
- UAR package works in Unity with Android BuildPlatform
- Reading of zpt files on windows platform
- Updated face tracker and mesh implementation

### Known issues
- Slow frame rate on Chrome Android version 91.0.xxx. Seems to have been fixed in Chrome Beta Android version 92.0.xxx


## [1.0.1-preview.1] - 2021-06-01
### Added 
- Unity SDK for ZCV libraries.
- Unity samples for running - Face Mesh, Face Tracker, Image Tracker, and Instant Tracker.
- Supported build platforms: WebGL (iOS/Android), native iOS, and native Android.
- Unity editor support - MacOS and Windows.


### Known issues:
- Image Tracker: no preview image of target zpt in editor (log message size is 0x0).
- Errors in editor with Android platform - Runtime is fine.
- Image and Instant tracking doesn't work in Editor
- Face Tracker and Face Mesh doesn't work on mobile browser. Works natively though.