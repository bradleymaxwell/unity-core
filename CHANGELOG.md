## [0.6.0] - 02-01-2026
### Features
- **Scenes** - SceneUtil to separate responsibility of gathering SceneData. Also finding all the scene's lights and passing the scene data through to the lifecycle hooks if a scene needs any information such as what lights to turn on and off.

## [0.5.2] - 02-01-2026
### Bugfixes
- **Scenes** - Fixed missing line setting the scene that is being shown as active via the unity scene manager.

## [0.5.1] - 02-01-2026
### Bugfixes
- **Scenes** - When showing a scene part of the navigation flow, we need to wait for the active scene to finish being hidden before we complete the show new scene coroutine, so that there are no race conditions with transition overlaps and when trying to unload the previous active scene immediately after.

## [0.5.0] - 02-01-2026
### Features
- **Scenes** - Showing scenes that are part of the scene service's navigation subsystem automatically hide the previously actively shown scene, maintaining a consistent one visible scene at a time rule. 

## [0.4.0] - 02-01-2026
### Features
- **Scenes** - Introduced SceneLifecycle to allow better control over visually hiding and showing scenes, as well as clear separation between load, show, hide, and show scene operations with support for automatic pre-loading a scene before being shown.

## [0.3.0] - 29-12-2025
### Features
- **Scenes** - Loaded scenes can now be automatically and manually made the actively displayed scene, as well as independent scene unloading as before it was only supported while loading another scene. 

## [0.2.1] - 28-12-2025
### Bugfixes
- **Scenes** - Infinite scene load fixed.

## [0.2.0] - 24-12-2025
### Features
- **Config** - Centralized point for setting up and managing references to custom config data containers defined using scriptable objects, set up during the initial bootstrap using parameters that are editable in the inspector.

### Removals
- **Scenes** - Start scene bootstrapper was removed because it is specific to whatever the game needs when the initial core flow is complete, so it is not needed in this package.

## [0.1.0] - 23-12-2025
### Features
- **Assets** - Asynchronous loading and unloading of assets utilizing Addressables.
- **Dependencies** - Locator pattern that manages singleton dependencies which any object at runtime can access.
- **Domain Events** - Pub/sub event system that allows de-coupled delegates for when objects want to react to certain game events but does not want a hard dependency on where it happened.
- **Input** - Custom layer on top of Unity's input system that acts as a global manager of how the player's input is handled across different parts of the game.
- **Logs** - Modular logger that improves upon the built-in unity logging which provides more information about where logs come from and control over what logs are displayed for more efficient debugging.
- **Pools** - Generic object pool system which allows pools to have their objects pre-instantiated.
- **Scenes** - Management of scenes by each scene having its bootstrapper that signals when the scene is ready before transition begins.
- **Storage** - Automatic saving and loading of custom data structure instances to and from the player's local files.