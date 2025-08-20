1.2.8

Changed:
- Removed copy-color pass from rendering for improved performance. Instead the Opaque texture is now used (and required).

Fixed:
- Transparent material sorting breaking in Unity 2023.2 when camera stacking was in use.

1.2.7

Changed:
- Refraction now uses the Opaque texture, instead of the nearest reflection probe. Generally improves visual quality and usability.

Fixed:
- Conflicting parameter name with Buto, causing its fog density value to affect underwater fog density.

1.2.6

Added:
- Scene view gizmo to display the current water level on both the Underwater Renderer and Underwater Trigger components.

Changed:
- Underwater Trigger now constantly sets the water level/material whilst inside the trigger (avoids other triggers that spawn in to interfere)

Fixed:
- Unfogged part of the underwater surface not being affected by directional light and ambient light.

1.2.5

Changed:
- Implemented proper error handling for Unity 6.

Fixed:
- Indirect lighting and reflections being black in demo scene in Unity 2023.2+.
- "Speed" parameter on the water material not affecting the underwater caustics animation speed

1.2.4

Fixed:
- Right eye rendering black on Android VR in Unity 2022.3.21+

1.2.3

Added:
- A warning to the Underwater Renderer UI if rendering was disabled by an external script.

Fixed:
- Camera-space distortion not having any effect when using VR.

1.2.2

Fixed:
- Caustics and Distortion effects appearing to be sped up when passing in a custom time index to the WaterObject.CustomTime API.

1.2.1

Fixed:
- Unintentional inclusion of the Dynamic Effects namespace in a script file, causing a compile error

Changed:
- The water level padding now defaults to a value of 0.5.

1.2.0
Minimum required version of Stylized Water 2 is now 1.5.2.

Added: 
- Underwater Trigger component, sets the active water material/level based on Collider and Camera trigger interactions.
- Underwater Trigger example scene, with two different water bodies.
- Custom UI for render feature.

Changed:
- Moved scripts into StylizedWater2.UnderwaterRendering namespace.
- Improved auto-setup functionality for render feature, now also sets up non-default renderers.
- Renamed the "Underwater Lightshaft" shader to "Underwater Particle".

Fixed:
- Cases where water would appear not to render if the static "Enable Rendering" boolean was set to false.

1.1.0
Minimum required version of Stylized Water 2 is now 1.5.0. 

Added:
- Underwater mask node for Amplify Shader Editor
- Exposed global shader boolean called "_UnderwaterRenderingEnabled"

Changed:
- Shader Graph sub graphs are now combined into a single master node.

Fixed:
- Pre-emptive shader error fixes in Unity 2023.2+, due to API changes.

1.0.9
Minimum required version of Stylized Water 2 is now 1.4.0.

Added:
- Rendering now uses the RTHandles system, finalizing support for Unity 2022.2+. Resources are now only re-allocated when called for.
- Pre-emptive support for Unity 2023

Changed:
- Underwater rendering shaders are now also C# generated. Future integrations/extensions will be automatically added.

Fixed:
- Water shader not supporting the SRP Batcher when underwater rendering was active.
- Normalized Distortion Frequency so it appears almost the same between the two quality modes
- Directional Caustics quality option causing strong visual artefacts on Meta Quest/Pro
- The scale of the Directional Light transform affecting the caustics projection

1.0.8

Fixed:
- Materials not receiving shadows, or showing banding artefacts, when a single shadow cascade is used. (In Unity 2021.3.15+, or versions released after December 15th 2022)
- Directional Caustics appearing to warp in Unity 2021.3.15+

1.0.7
Minimum required version of Stylized Water 2 is now 1.3.0.

Changed:
- Updated scripting functionality to reflect the material parameter changes in Stylized Water 2 v1.3.0.
- Waterline now also supports animation using networked synchronized time.

1.0.6

Fixed:
- Preliminary fixes for Unity 2022.2 and 2023.1. Removal of now obsolete API's in 2022.1+ will be addressed in a future update.

1.0.5
Minimum supported Unity version is now 2020.3. 

Added:
- Support for Single Pass Instanced/Multi-view VR rendering (requires Unity 2021.2+).

Changed:
- Optimized blur to use fewer samples.
- Minor improvements to distortion. Now also renders at 1/4th resolution without a noticeable quality loss.
- Directional Caustics no longer require an intermediary rendering step.
- Values for the Water level field in the inspector are now always updated.

Fixed:
- Directional Caustics not taking effect in Unity 2020.3, unless accurate option was enabled.
- Improvement to water mask rendering, where some times a triangle would render in front of it.

1.0.4
Minimum required version of Stylized Water 2 is now 1.1.5. Verified compatibility with 2021.2.0

Added:
- Gradient ambient lighting now affects the underwater fog as well (equator color is used).
- Parameter (component/volume) to multiply the caustics strength underwater.
- Support for cascaded shadow maps

Changed:
- Refactored shading of the underwater surface and fog:
  * Surface now partially reflects the underwater color, creating a more believable appearance.
  * Fog floods the water surface as the camera goes deeper, eventually covering it completely.
  * Two new parameters are added to the "Underwater" tab on the water material, controlling aspects of the underwater surface.
  * Underwater fog now has separate control over vertical/horizontal density. With added height fog controls.
  
- Performance optimization:
  * A depth pre-pass for the water surfaces is no longer needed, cuts down on N number of draw calls.
  * In Unity 2021.2.0+, 2 frame buffer copies can be skipped, by making use of an improved feature.
  
- Waves & Caustics are now also disabled if they are on the water material.
- Shader Graph node to apply fog now only modifies the Emission value. Lighting is now incorporated into the fog color. This ensures scene lighting doesn't interfere.

Fixed:
- Fog and water surface not matching entirely, when using a skybox as the ambient light source.
- Objects underwater, in front of the water surface, also being refracted (Limited to the Advanced shading mode).
- Rendering no longer takes effect for 'preview' camera's. Otherwise affects asset thumbnails generated while underwater.
- Shader error when building to PS5.

1.0.3

Added:
- Parameters to control screen distortion strength, frequency and speed. On the Underwater Renderer component and volume settings component

Fixed:
- Caustics and subsurface scattering appearing to warp in some specific URP 7.5.x versions
- Unintentional use of an absolute file path, preventing the StylizedWater2 folder from being moved

1.0.2

Fixed:
- Rendering not taking any effect in URP 7.5.2 and 7.5.3
- Waterline/meniscus rendering behind the camera on OpenGL in Unity 2020.2+

1.0.1

Added:
- Static SetCurrentWaterLevel and SetCurrentMaterial functions to the UnderwaterRenderer class. 
  * This can be used to switch to other water bodies via triggers or other game logic
- UnderwaterRenderer.EnableRendering static boolean. Can be toggled to disable rendering in specific cases.
- Directional Caustics support for versions older than Unity 2020.2
- Option to enable high accuracy directional caustics (using either the Depth Normals prepass, or reconstructing it from the depth texture)
    
Fixed:
- Underwater surface not matching up perfectly if ambient skybox lighting is used with an intensity higher than x1
- Corrected default settings for particle system prefabs
- Enabling volume settings blending, requiring to toggle the component for it to take effect.
- Rendering also taking effect when editing a prefab

Changed:
- Render feature can longer be auto-added when in playmode

1.0.0
Initial release