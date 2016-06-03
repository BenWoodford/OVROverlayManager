=== Vive Overlay Helper ===

1.) Create a Render Texture for each overlay and a camera for each overlay too. Set the cameras to have a reasonable clipping plane (10?). Set the resolutions up appropriately.

2.) Create a canvas for each overlay, set it to _Screen Space - Camera_ and set the camera to the relevant camera from Step 1.

3.) Set up your canvas as you wish. For a test, just put some text on it.

4.) Set the direct children of your canvas to have a negative scale. DirectX in Unity renders textures upside down, it's a pain.

5.) (Untested) Set a path for your icon. It doesn't like Resource-based icons so you'll need to provide a real file path, not a Resources.Load path.

6.) Create an __OverlayProjector__ for each dashboard, assign the appropriate canvas and set overlay type (only Dashboard tested so far). Set the key and friendly name too. Oh, and the Render Texture

7.) If you want input, remove the StandaloneInputModule from EventSystem and add the ViveInputModule. Ignore the fields, they're just there because it inherits from StandardInputModule

It might work now, I'm not sure. This isn't a terribly good documentation.