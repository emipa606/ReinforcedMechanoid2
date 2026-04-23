# .github/copilot-instructions.md for Reinforced Mechanoid 2 (Continued)

## Mod Overview and Purpose
The "Reinforced Mechanoid 2 (Continued)" mod is an enhancement of the original RimWorld mechanoid system, integrating new gameplay mechanics, threats, weapons, buildings, and factions that align seamlessly with the vanilla RimWorld experience. Originally named "HALO: Rimworld Heretic Mechanoids," this mod builds upon the foundation laid by "Reinforced Mechanoids: Tyrikan-Line."

The mod leverages the enhanced support for Asset Bundles provided by RimWorld 1.6 to streamline the mod's size and performance. It incorporates the functionality of the Gestalt Engine, eliminating the need for this additional mod, making mechanoid management more intuitive and efficient.

## Key Features and Systems
- **New Mechanoids**: Introduces a variety of new mechanoids, each with unique abilities, behaviors, and roles. These mechanoids are part of a new ancient faction on the world map, offering fresh challenges and storytelling opportunities.
- **Mechanoid Construction**: Players can construct and utilize mechanoids in combat, allowing for strategic innovation.
- **Mechanoid Power**: Implements a new power production method, inspired by the "Mechanoid Power" mod, with new code, textures, and finely tuned balance.
- **DLC Integration**: Owners of the Royalty DLC can now utilize the weather controller and climate adjuster for their own colonies.
- **Gestalt Engine**: Control mechanoids without a mechanitor, providing a streamlined and autonomous control mechanism.
- **Vanilla-Friendly Integration**: Ensures all new assets, textures, and balance adjustments feel native to the RimWorld environment.

## Coding Patterns and Conventions
- **Naming Conventions**: Classes and methods follow PascalCase, which is standard for C# development.
- **Class Structure**: Utility classes are marked appropriately (e.g., `internal static class Utils`), and core functionalities are encapsulated in specific classes such as `CompGestaltEngine` for managing Gestalt Engine capabilities.
- **Comments and Documentation**: Use XML comments (`///`) for code documentation to facilitate IntelliSense support and enhance comprehension for collaborators and users.

## XML Integration
- XML files are used to define game components such as mechanoid behaviors and item definitions, ensuring easy integration and modification.
- Use of DefModExtensions for dynamic data loading from XML, as seen in classes like `BeamExtension` and `EquipmentDrawPositionOffsetExtension`.
- When defining new entities or modifying existing ones, ensure XML tags and data are consistent with RimWorld standards to avoid conflicts and errors.

## Harmony Patching
- **Harmony Library**: Utilized extensively for modifying existing game functions without altering core game files directly.
- All patches are organized within a single class, `HarmonyPatches`, which initializes and applies the necessary patches when the mod is loaded.
- **Patch Examples**: Patches like `CameraJumper_TryJumpAndSelect_Patch` and `Pawn_MechanitorTracker_CanControlMechs_Patch` demonstrate targeted enhancements to existing game logic without disrupting base game mechanics.

## Suggestions for Copilot
- Write efficient code by leveraging C# features such as LINQ, async/await for asynchronous operations, and try-catch blocks for error handling.
- Consider creating helper functions for repetitive tasks to promote DRY (Don't Repeat Yourself) principles.
- Utilize Copilot’s ability to autocomplete by starting with clear intents or method signatures to improve predictability of the suggestions.
- Leverage XML comment templates to maintain coherent and standardized documentation across codebase components.
- When creating new functionalities, consider potential interactions and dependencies within the RimWorld ecosystem to ensure smooth gameplay and mod compatibility.

This file provides an overarching guide for leveraging GitHub Copilot in the development of the "Reinforced Mechanoid 2 (Continued)" project, enhancing productivity and code quality while maintaining the mod's unique identity and seamless integration with the base game.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.
