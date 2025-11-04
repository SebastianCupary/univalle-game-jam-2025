using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

// Ensures TextCore FontAssets are initialized on the main thread before any UI text jobs run
public static class TextCoreFontWarmup
{
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
 private static void Warmup()
 {
 // Try to gather font assets from any available TextSettings assets
 var fonts = new List<FontAsset>();
 try
 {
 var settingsAssets = Resources.FindObjectsOfTypeAll<TextSettings>();
 for (int s =0; s < settingsAssets.Length; s++)
 {
 var st = settingsAssets[s];
 if (st == null) continue;
 if (st.defaultFontAsset != null) fonts.Add(st.defaultFontAsset);
 if (st.fallbackFontAssets != null) fonts.AddRange(st.fallbackFontAssets);
 }
 }
 catch { /* ignore if API/assets not available */ }

 // Also include any FontAssets already loaded in memory (e.g., from scenes/resources)
 try
 {
 var loadedFonts = Resources.FindObjectsOfTypeAll<FontAsset>();
 if (loadedFonts != null && loadedFonts.Length >0)
 {
 for (int i =0; i < loadedFonts.Length; i++)
 {
 var fa = loadedFonts[i];
 if (fa != null && !fonts.Contains(fa)) fonts.Add(fa);
 }
 }
 }
 catch { /* ignored */ }

 // Touch members that cause lazy init on the main thread
 for (int i =0; i < fonts.Count; i++)
 {
 var fa = fonts[i];
 if (fa == null) continue;
 try
 {
 var _ = fa.faceInfo; // triggers metadata access
 var __ = fa.atlasTexture; // ensures atlas/material are ready
 var ___ = fa.characterLookupTable; // ensures character tables are built
 }
 catch { /* best-effort warmup */ }
 }
 }
}
