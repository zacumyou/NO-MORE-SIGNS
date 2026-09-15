# NO MORE SIGNS 1.4.3

Checked: 2026-09-16 (Asia/Seoul).

## Official documentation

- https://cs2.paradoxwikis.com/Modding — read the official Code Mods resource list in the browser.
- https://cs2.paradoxwikis.com/Modding_Toolchain — read C# Mod Project Template, Building the Mod, and How to publish a Code Mod. The page's verification badge is for 1.1.12 f1; checked installed tooling to resolve version drift.
- Existing mods use PublishNewVersion with the existing ModId, a new mod version, and a changelog. UpdatePublishedConfiguration changes metadata only.
- The installed PublishNewVersion.pubxml maps to ModPublisher NewVersion. Current ModPublisher Publish creates a new item, so it must not be used for this update.

## Compatibility and identity

- Existing public item: 158284, installed revision 2, assembly version 1.4.2.0.
- Existing recommended game version: 1.6.*. Corrected the repository's stale 1.0.* publishing value to 1.6.*.
- New mod version: 1.4.3; this is independent of the game version.
- Preserved FirstMapPopup assembly/namespace, PopupSettings type, NoMoreSigns settings filename/key, and all option property names.
- Only log naming and build/release metadata changed. No road visibility algorithm changes.
- No explicit game-version gate found in the mod source or assembly attributes.
- Package built against installed game 1.6.2f1 with official IL post-processing and Burst output.

## Verification

- Release build: 0 warnings, 0 errors.
- 98,304 road-scope cases passed; defaults, bulk save actions, and six locale catalogs passed.
- 17 asset-name cases and 8 decal cases passed.
- Tests use engine stubs. In-game rendering, UI, and an actual subscriber upgrade have not been exercised.
- Existing public description, forum link, external link, and access level were taken from current local Paradox metadata. The publisher requires a thumbnail, so the current published thumbnail was downloaded from the official content URL and reused unchanged. Screenshots are omitted so existing screenshots are preserved.

## Publishing result

- ModPublisher NewVersion completed with exit code 0 and reported New mod version published.
- Public page: https://mods.paradoxplaza.com/mods/158284/Windows
- Immediately after upload, the page showed pending publication, previous visible version 1.4.2, recommended game version 1.6.*, and 209 subscribers. Server processing is separate from successful upload.
- Local installation: all seven package files matched the release package by SHA-256; installed assembly version 1.4.3.0.
