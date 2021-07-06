# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).


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