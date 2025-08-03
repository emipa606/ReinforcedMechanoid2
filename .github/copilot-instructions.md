# GitHub Copilot Instructions for Reinforced Mechanoid 2 (Continued)

## Mod Overview and Purpose

**Reinforced Mechanoid 2 (Continued)** is a continuation and enhancement of the previously known "HALO: Rimworld Heretic Mechanoids." This mod expands on "Reinforced Mechanoids: Tyrikan-Line" and the vanilla game of RimWorld by introducing new threats, weapons, buildings, factions, and gameplay mechanics. Targeted at maintaining a seamless integration with the base game, this mod enhances the game experience without breaking the vanilla aesthetic and balance.

## Key Features and Systems

1. **New Mechanoids:** Introduces a variety of mechanoids with unique abilities and behaviors, providing fresh challenges. Some belong to an ancient faction distinct from the traditional enemies.
   
2. **Mechanoid Construction:** Allows players to construct mechanoids and use them in combat.
   
3. **Mechanoid Power Source:** A new power production method revamps the prior mod "Mechanoid Power."
   
4. **Gestalt Engine:** Enables control of mechanoids without a mechanitor, expanding tactical options.
   
5. **DLC Integration:** Utilizes Royalty DLC assets, providing control over the weather controller and climate adjuster.
   
## Coding Patterns and Conventions

- **Namespaces & Classes:** Use descriptive class names reflecting their functionality or tied features.
- **Members & Methods:** Follow PascalCase naming for public members and camelCase for private fields.
- **Documentation:** Methods should have XML comments describing their purpose, input parameters, and return values.
- **Separation of Concerns:** Encourage distinct handling of gameplay logic, UI, and data through modular methods and classes.

## XML Integration

- **DefModExtensions:** Utilize XML for game definitions that can be extended with custom classes (e.g., `DefModExtension`).
- **Patches & Overrides:** Support alterations directly via XML against vanilla game entities.
- **Data Loading:** Classes like `ThingDefRecord` and `PawnKindRecord` demonstrate XML data parsing for custom mechanics.

## Harmony Patching

- **Patch Management:** Centralize patches within organized static classes, such as `HarmonyPatches`.
- **Prefix & Postfix Methods:** Employ Harmony's Prefix and Postfix to maintain vanilla integrity while enhancing functionality.
- **Targeted Patching:** Use specific patches to change behaviors (e.g., `JobDriver_Flee_MakeNewToils_Patch_CanEmitFleeMote`) without disturbing unrelated processes.

## Suggestions for Copilot

1. **Class Skeletons:** Generate base class definitions with constructors and a defined interface, implementing required methods.
   
2. **Harmony Patches:** Auto-generate Harmony patch templates with placeholder functionalities for prefix and postfix integration.
   
3. **Method Stubs:** Create method stubs describing parameters and return types, ready to be filled with the relevant logic.
   
4. **XML Definition Generation:** Provide templates for creating new XML definitions extended with mod-specific logic using DefModExtension.
   
5. **Error Handling Patterns:** Offer patterns for robust error handling and logging via 'try-catch' blocks where gameplay or computation logic might fail.

This instruction file aims to serve as a reference to maintain consistent development practices and guidance for utilizing GitHub Copilot effectively for continued development within the **Reinforced Mechanoid 2 (Continued)** mod project.
