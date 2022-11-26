1.0.5
Minimum supported Unity version is now 2020.3. Removal of now obsolete API's in 2022.1 will be addressed in a future update.

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