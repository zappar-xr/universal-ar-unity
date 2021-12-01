# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

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