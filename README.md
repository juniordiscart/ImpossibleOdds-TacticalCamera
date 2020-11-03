# ![Impossible Odds Logo][Logo] Impossible Odds - Tactical Camera

The `TacticalCamera` package provides a camera system for smoothly navigating your environments. It allows for both a top-down tactical view as well as close-up action views with some additional features:

* Move using keyboard input.
* Move using screen edge detection.
* Move to target position.
* Zooming in / out.
* Look rotations, with height-adjusted tilt limits.
* Orbit rotations around the focus point.
* Dynamic field of view, based on height.
* Height-adjusted movement speed.
* Restricted area of operation.
* Smooth collision resolve with terrain and objects.
* Extensive customization using animation curves.

It's also designed to have minimal setup and integrate well in existing projects.

[![Impossible Odds - Tactical Camera](http://img.youtube.com/vi/X4F_rTh-dkQ/0.jpg)](https://youtu.be/X4F_rTh-dkQ "Impossible Odds - Tactical Camera")

## Setting up the camera

The `TacticalCamera` Component requires to be put on your `Camera` GameObject. It will also use a `CharacterController` which is added automatically (if not there already) to resolve collisions as well as smoothly go over any terrain and obstacles.

It requires two things to operate:

* An input provider: the `TacticalCameraInputProvider` Component is a sample implementation which is perfect for testing and quick setup.
* Animation settings: create an instance of the `TacticalCameraSettings` ScriptableObject. Adjust these to define how your camera should animate and smooth out.

Assign these to their respective fields in the Unity inspector view of the `TacticalCamera`.

Optionally, you can also place some bounds on the area of operation of the camera. The `TacticalCameraBoxBounds` is a sample implementation to restrict your camera to operating in an axis-align box.

## Advanced

### Settings

The camera's settings define its behaviour regarding movement and rotation. These two are mostly driven by animation curves, value ranges and fade times.

* Movement settings: how fast the camera can move at a certain height as well the range of height values the camera is allowed to reach.
* Rotation settings: how fast the camera can look around as well as orbit around its focus point.
* Tilt settings: tilt angle ranges for both low and high altitudes, and how these ranges should interpolate.
* Field of view: should dynamic field of view be used, and how it should transition based on the camer'a height.
* World interaction settings: filter and define the raycast interaction distance for height control and move-to-target.

### Input

The camera's input provision can be integrated in your project's input system by implementing the `ITacticalCameraInputProvider` interface.

A default implementation is available for which you can assign custom input keys. Your custom implementation can be assigned to the `InputProvider` property, and can also be injected into it.

### Area of operation

The camera's area of operation can be restricted using the `ITacticalCameraBounds` interface or `AbstractTacticalCameraBounds` abstract class. This allows you to implement custom area restrictions in your project, e.g. restricting in a polygon area, not entering the fog of war, etc.

The bounds can be assigned through the Unity inspector view when being derived from `AbstractTacticalCameraBounds`, but can also be assigned to the `Bounds` property or be injected into it.

### Gotcha's

* The camera is designed to work on its own without interference on the rotation of the object from the outside, e.g. animations.
* The `z`-value of the local Euler rotation angle is always set to 0 at the end of its `LateUpdate` phase. This is to prevent drift and keeps the camera straight up.
* The operating range of the tilt angle in the `TacticalCameraSettings` objects can be no larger than [-90, 90] degrees, with 0 being level with the horizon. This range is defined to prevent flipping over. If you make a custom implementation through `ITacticalCameraSettings`, best is to keep this in mind.
* This package includes the [Impossible Odds Coding Toolkit](https://www.impossible-odds.net/unity-toolkit/). Feel free to use it!

## Unity Version

Developed and tested on Unity 2019.4 LTS.

## License

This package is provided under the [MIT][License] license.

[License]: ./LICENSE.md
[Logo]: ./ImpossibleOddsLogo.png
