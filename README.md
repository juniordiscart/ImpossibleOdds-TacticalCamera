# Impossible Odds - Tactical Camera

The `TacticalCamera` package contains a small tool to help and develop your camera system useful for tactical environment overviews for strategy games or games that require some kind of birds-eye view of a map. It provides a setup to smoothly move, zoom and rotate the camera and limit its range of action. Supported features:

* Keyboard / screen edge movement
* Move to target
* Zooming in / out
* Smooth camera angle and movement speed adjustments based on the altitude
* Pivot rotations - looking around its current position
* Orbit rotations - orbit the camera around its focus point
* Area of operation - restrict the camera to move only in a specific area
* Flexible and easy to use settings
* Easily integrates with your input classes

![alt text][TacticalCameraFeatures]

## Setting up the camera

The `TacticalCamera` package requires little setup. Hookup the `TacticalCamera` component to your camera game object and provide it with the desired settings. You can read more about these settings down below. To make the `TacticalCamera` move, we also need to provide it with an input provider. As a sample, you can add the `TacticalCameraInputProvider` component to your camera. Optionally, you can add the `TacticalCameraBoxBounds` component to restrict the area of movement. Hit play!

![alt text][TacticalCameraSetup]

The `TacticalCamera` script only manages the state of the camera's movement. It does not process inputs directly, so it requires something that processes and provides the player's desired camera input. Next to this input provider, it also requires some settings that defines its behaviour. They describe how the camera reacts to these inputs and how they should smooth out.

Optionally, you can also place some bounds on the area of operation of the camera.

## Camera settings

The `TacticalCameraSettings` describe how the camera reacts whenever it is asked to move, zoom or rotate. The settings mostly consist of curves that describe the way the camera should smooth out a certain movement. Below is a detailed description of each of the settings.

![alt text][TacticalCameraCreateSettings]

![alt text][TacticalCameraSettings]

  * Movement settings. These settings relates to moving the camera around the world.
    * The movement speed transition describes how the speed of the camera is affected by the height it is at. When at a high altitude, it may make more sense to cross a large distance faster, than it should when being low to the ground when watching nitty-gritty details. If you like it to have a consistent speed at every height, make it a flat line from left to right.
	* The movement rolloff describes how a movement value should smooth out when no input is given anylonger.
	* The movement rolloff time defines how long this smoothing should take.
	* The move to target position smoothing time defines how long the camera should smooth when it is moving to it's target destination, i.e. by double clicking on the terrain, it will move to that location.
	* The height range defines the minimum _relative_ height and the _absolute_ maximum height. The minimum value is relative to the terrain or ground objects the camera is overing over. For example, it will always keep a minimum altitude, even over some hills or mountains. The maximum value is absolute, so the camera will never go above that height.

  * Pivot settings. These settings relate to the camera rotating around its current position.
	* The max pivotal speed is the amount of degrees the camera can rotate per second.
	* The pivotal rolloff is the curve that defines the smoothing of the pivot manouvre after no input is given anymore.
	* The pivotal rolloff time is the amount of time this smoothing should happen.

  * Orbit settings. These settings relate to the camera rotating around its current point of focus. A ray is cast from the camera into the world (see the world interaction settings), and will orbit around it.
	* The max orbital speed is the distance the camera can cover per second while orbiting.
	* The orbital rolloff curve is the smoothing curve after no input is given anymore.
	* The orbital rolloff time is the amount of time this smoothing should happen.

  * Pitch settings. These settings relate to the range in which the camera can pitch or tilt around it's x axis. Based on the camera's altitude, the range in which it can tilt is modified. This allows to restrict the camera from looking too far down while near the ground and keeping the camera on the action at all times.
	* Pitch range high: the range of angles (in degrees) the camera can look down while it is up high.
	* Pitch range low: the range of angles (in degrees) the camera can look down while it is close to the ground.
	* Pitch range transition: in what way should the transition of these range of values be interpolated. The left side is considered to be up high, while the right is considered to be down low.

  * World interaction settings. These settings relate to some aspects of how you set up your game world.
	* Interaction mask: defines what layers of your game world are interesting for the camera to interact with. For example, to make the camera orbit, or to let it determine what is ground level, the camera will shoot some rays into the world. Here you can set which layers should be considered to interact with. This can also help in optimizing performance when the camera is used in densly populated worlds.
	* Max interaction ray length: to further help in getting great performance, restricitng the length of the raycasts helps in culling far away objects. Set this to a sensible length depending on how high your camera can go, and how far it can see in the distance, i.e. the far plane value of the camera component.

## Custom input

The `TacticalCameraInputProvider` is a sample implementation for providing input to the camera. It's great for demo or prototyping purposes. It impements the `ITacticalCameraInputProvider` interface and offers itself to the `TacticalCamera` script in its startup phase. In your project, it is best that you implement the `ITacticalCameraInputProvider` interface in your own input class(es) that reflects the way would like to control the camera (keyboard / mouse, gamepad, touch, ...) and offer it to the `TacticalCamera.InputProvider` property. You can, of course, look through the example code and merge it with your input structure.

## Area bounds

You can limit the `TacticalCamera`'s area of operation by assigning an `ITacticalCameraBounds` to its `Bounds` property. At the end of all its movement and rotation, if set, will let the boundary object know it is done, and should check whether it still within bounds. If not, it should put it back in. A sample implementation of limiting the camera is given by the `TacticalCameraBoxBounds`, which restricts the camera of operating within the area defined by the box.


[TacticalCameraFeatures]: ./Documentation/img/Features.gif
[TacticalCameraSetup]: ./Documentation/img/Setup.gif
[TacticalCameraCreateSettings]: ./Documentation/img/CreateSettings.png
[TacticalCameraSettings]: ./Documentation/img/Settings.png
