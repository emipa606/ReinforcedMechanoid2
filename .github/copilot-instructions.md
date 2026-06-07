# GitHub Copilot Instructions for Reinforced Mechanoid 2 (Continued)

## Mod Overview and Purpose
The "Reinforced Mechanoid 2 (Continued)" mod is an expansion pack for RimWorld that enhances and expands upon the vanilla mechanoid experience. Originally known as "HALO: Rimworld Heretic Mechanoids," this mod introduces new mechanoid units, weapons, buildings, factions, and gameplay mechanics, all of which are designed to integrate seamlessly with the base game. The mod also features unique systems that innovate on existing game mechanics, offering a richer and more varied gameplay experience.

## Key Features and Systems
- **New Mechanoid Variants**: Introduces mechanoids with unique abilities and behaviors, beyond mere aesthetic changes, providing new combat challenges.
  
- **Ancient Faction Integration**: Features mechanoids that belong to a new ancient faction on the world map, expanding the narrative and strategic elements of the game.

- **Player-Created Mechanoids**: Allows players to construct and deploy their own mechanoid units, adding depth to colony management and combat strategies.

- **New Power Production Method**: Builds upon the "Mechanoid Power" mod to provide a novel way to generate power with balanced mechanics.

- **Royalty DLC Integration**: Empowers players to utilize weather controllers and climate adjusters, significantly modifying environmental gameplay.

- **Gestalt Engine**: Provides a mechanism for controlling mechanoids independently, without necessitating a mechanitor, thereby offering players greater tactical flexibility.

- **Vanilla-Friendly Design**: All additions are balanced and crafted to match the vanilla art style and lore, offering a cohesive expansion experience.

## Coding Patterns and Conventions
- **C# Code Structure**: Follow a clear and organized file structure, with distinct C# files for different mod components like the `GestaltEngine` and its patches.

- **Naming Conventions**: Use PascalCase for class names and methods, and camelCase for method parameters. Maintain clarity and context in naming to enhance code readability.

- **Modularity**: Separate concerns by organizing functionality into distinct classes and methods. This includes the use of prefixes and postfixes to apply patches via Harmony.

## XML Integration
- **Defs Configuration**: XML files are used to define new game entities, such as body parts (`BodyPartDef`), cultures (`CultureDef`), and structural layouts (`KCSG.StructureLayoutDef`).

- **Path Organization**: Maintain a structured directory for XML definitions under version-specific folders to ensure compatibility and ease of updating.

- **Vanilla Compatibility**: Ensure that new XML defs integrate smoothly with existing game mechanics by adhering to existing data structures and naming conventions.

## Harmony Patching
- **Adding Patches**: Use the Harmony library for method patching, allowing for alterations or extensions of vanilla game methods without direct modifications.

- **Prefix/Postfix Usage**: Apply prefixes and postfixes judiciously to adjust or augment game logic. Prefix methods can alter or prevent the execution of original methods, while postfixes can execute additional logic after the original method completes.

- **Example Implementations**: The `CameraJumper_TryJumpAndSelect_Patch` uses a `Prefix` to modify camera behavior, while patches in `CompGestaltEngine` manage mechanoid control logic.

## Suggestions for Copilot
- **Contextual Predictions**: Copilot can aid in suggesting XML tags and attributes based on existing mod definitions. Ensure suggestions follow the pattern established in similar def files.
  
- **Method Stubs**: When creating new methods or patches, Copilot can propose method signatures and basic logic that align with the mod's programming style and structure.

- **Bug Fixing**: Use Copilot to suggest fixes for common coding issues, specifically in Harmony patches or XML errors.

- **Documentation Assistance**: Leverage Copilot to auto-generate detailed method and class comments based on code logic and intents, enhancing code maintainability and readability.

This guide ensures that contributors to the "Reinforced Mechanoid 2 (Continued)" mod project maintain coding quality and consistency across development stages.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).

