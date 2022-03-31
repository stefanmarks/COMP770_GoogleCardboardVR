# COMP770 Google Cardboard VR

Starter repository for COMP770 Google Cardboard VR development

This is a pre-packaged Unity project with:

- Google Cardboard SDK
- Resonance Audio SDK
- SentienceLab script selection

Version: 2022, Semester 1

## Minimum Android SDK Problem

If your Android phone is older than Android "Nougat 7.1" (SDK v25), you can switch to an older version of the GVR XR plugin (1.9) which uses the v18 SDK. That should make the project work on Phones with mimum Android "Kitkat 4.1".

- Edit `Packages/manifest.json` and change `com.google.xr.cardboard": "file:../LocalPackages/COMP770_Cardboard_XR_Plugin_1.12.0` to `com.google.xr.cardboard": "file:../LocalPackages/COMP770_Cardboard_XR_Plugin_1.9.0`
- In `Project Settings/Other Settings`, change the minimum API to 19.
