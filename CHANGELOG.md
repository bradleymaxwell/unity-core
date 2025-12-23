## 0.1.0 - First Developmental Release
### Features
- **Assets** - Asynchronous loading and unloading of assets utilizing Addressables.
- **Dependencies** - Locator pattern that manages singleton dependencies which any object at runtime can access.
- **Domain Events** - Pub/sub event system that allows de-coupled delegates for when objects want to react to certain game events but does not want a hard dependency on where it happened.
- **Input** - Custom layer on top of Unity's input system that acts as a global manager of how the player's input is handled across different parts of the game.
- **Logs** - Modular logger that improves upon the built-in unity logging which provides more information about where logs come from and control over what logs are displayed for more efficient debugging.
- **Pools** - Generic object pool system which allows pools to have their objects pre-instantiated.
- **Scenes** - Management of scenes by each scene having its bootstrapper that signals when the scene is ready before transition begins.
- **Storage** - Automatic saving and loading of custom data structure instances to and from the player's local files.