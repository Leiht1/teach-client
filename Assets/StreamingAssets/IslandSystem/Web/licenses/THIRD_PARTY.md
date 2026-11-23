# Third-party components

The game workbench uses the unmodified official Scratch Blocks npm distribution 2.2.13 (Apache-2.0), Blockly 12.5.1 (Apache-2.0), @blockly/continuous-toolbox 7.0.9 (Apache-2.0), and @blockly/field-colour 6.0.12 (Apache-2.0). Their license notices and Scratch trademark policy accompany this folder. This is an independent game UI, not a Scratch Foundation product or endorsement. No Scratch logo, cat, Scratch GUI or Scratch VM is included.

Sources:
- https://github.com/scratchfoundation/scratch-blocks/tree/v2.2.13
- https://github.com/RaspberryPiFoundation/blockly/tree/blockly-v12.5.1
- https://github.com/google/blockly-samples

The adjacent Host directory contains Microsoft WebView2 SDK 1.0.3537.50 redistribution components and its original LICENSE.txt. The Microsoft Edge WebView2 Runtime is a separately installed system component.

Game-specific block definitions, theme, shell, persistence adapter and Unity bridge are in Tools/IslandScratchWeb/src and Assets/IslandSystem. Upstream rendering, dragger and connection code have not been edited. Build scripts and exact dependency versions are included for reproducibility.
