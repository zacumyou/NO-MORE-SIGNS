# NO MORE SIGNS

A Cities: Skylines II code mod that hides selected roadside objects and decals. Version **1.4.0**.

[한국어 안내](README.ko.md)

## Features

Regular roads and highways have independent settings. **ON means hidden; OFF means shown normally.**

| Category | Default for both road types |
| --- | --- |
| Signs, including stop, yield and speed limit signs | ON |
| Traffic lights | OFF |
| Street lights, including their light sources | OFF |
| Other road props, including hydrants and electrical boxes | ON |
| Speed limit pavement markings | OFF |
| Lane direction arrows | OFF |

Options include Restore defaults, Enable all and Disable all buttons, persistent settings, and the running mod version. Supported languages: English, Korean, Japanese, Simplified Chinese, Traditional Chinese and Spanish.

## Scope and save behavior

Highways are identified by `RoadData.m_Flags` and `RoadFlags.UseHighwayRules`, rather than road names or speed limits. Shared objects at a junction between both road types are hidden only when the corresponding setting is enabled for both types.

The mod filters transient rendering data. It does not delete road objects, change prefab generation candidates, alter traffic rules, or add custom city-save components. Street-light intensity is temporarily filtered for light culling, then restored. Settings are stored separately from city saves.

Buildings, independently placed objects, trees, bridge structures and unrelated decals are outside the intended scope. Composite or custom assets may not always be classified correctly. Save/reload/removal behavior, large-city performance and all DLC/custom-road combinations still require broader in-game testing.

## Build

Install the game's official [Modding Toolchain](https://cs2.paradoxwikis.com/Modding_Toolchain) and complete its setup first. The project uses the locally installed game assemblies and the toolchain's `CSII_TOOLPATH` user environment variable; those dependencies are not included here.

From the repository root, with the game closed:

```powershell
dotnet build src/FirstMapPopup/FirstMapPopup.csproj -c Release
```

The official build targets deploy the mod to the local Mods folder. `FirstMapPopup` remains the internal project, assembly and installation identity for compatibility; the displayed name is NO MORE SIGNS.

The files under `Properties` are the toolchain's publishing scaffold. Its thumbnail, game-version field and publishing metadata are not finalized. A normal build is for local development; this repository does not represent a Paradox Mods release.

## Logic checks

The standalone checks require .NET 9:

```powershell
dotnet run --project tests/NameTests/NameTests.csproj
dotnet run --project tests/OptionTests/OptionTests.csproj
```

They cover prop-name/decal classification, 98,304 visibility combinations, defaults, bulk actions, save calls and translation-key consistency. Settings tests use game API stubs: they do not run the actual game UI or renderer.

## License and attribution

MIT. See [LICENSE](LICENSE). The rendering approach adapts work from [NoSpeedLimitMarkings](https://github.com/anonymousprime2020/NoSpeedLimitMarkings); its copyright and MIT notice are preserved in [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt). Game and toolchain dependencies retain their own licenses.
