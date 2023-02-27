using HarmonyLib;
using Kitchen;
using Kitchen.Components;
using KitchenPurrBliss.Utils;
using KitchenPurrBliss.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

namespace KitchenPurrBliss.Patches
{
    [HarmonyPatch]
    internal static class LocalViewRouter_Patch
    {

        const string VFX_NAME = "Hearts";
        static string[] audioClipNames = new string[] { 
            "purr1.ogg",
            "purr2.ogg",
            "purr3.ogg",
            "purr4.ogg",
            "purr5.ogg",
            "purr6.ogg",
            "purr7.ogg",
            "purr8.ogg" };

        [HarmonyPatch(typeof(LocalViewRouter), "GetPrefab", new Type[] { typeof (ViewType) })]
        [HarmonyPostfix]
        static void GetPrefab_Postfix(ref GameObject __result, ViewType view_type)
        {
            if (view_type == ViewType.CustomerCat && __result.GetComponent<VisualEffectWhilePettingView>() == null)
            {
                GameObject vfxGameObject = new GameObject($"Petting VFX - {VFX_NAME}");
                VisualEffectAsset asset = Resources.FindObjectsOfTypeAll<VisualEffectAsset>().Where(vfx => vfx.name == VFX_NAME).FirstOrDefault();
                if (asset != default)
                {

                    VisualEffect vfx = vfxGameObject.GetComponent<VisualEffect>();
                    if (vfx == null)
                    {
                        vfx = vfxGameObject.AddComponent<VisualEffect>();
                    }
                    vfx.visualEffectAsset = asset;

                    vfxGameObject.transform.parent = __result.transform;
                    vfxGameObject.transform.localScale = Vector3.one;
                    vfxGameObject.transform.localPosition = Vector3.zero;
                    vfxGameObject.transform.rotation = Quaternion.identity;
                    vfxGameObject.SetActive(false);

                    Main.LogInfo($"Added VFX - {VFX_NAME}");

                    VisualEffectWhilePettingView view = __result.AddComponent<VisualEffectWhilePettingView>();
                    view.VfxGameObject = vfxGameObject;

                    Main.LogInfo($"Added View - VisualEffectWhilePettingView");

                    if (Main.Bundle != null)
                    {
                        SoundSource soundSource = __result.AddComponent<SoundSource>();
                        soundSource.TransitionTime = 0.1f;
                        soundSource.ShouldLoop = false;
                        view.SoundSource = soundSource;

                        List<AudioClip> clips = new List<AudioClip>();
                        foreach (string clipName in audioClipNames)
                        {
                            AudioClip clip = AudioUtils.LoadWavFromAssetBundle(Main.Bundle, clipName);
                            if (clip != null)
                            {
                                clips.Add(clip);
                                Main.LogInfo($"Added AudioClip - {clipName}");
                                continue;
                            }
                            Main.LogWarning($"Could not find AudioClip - {clipName}");
                        }
                        view.AudioClips = clips;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(LocalViewRouter), "HandleUpdate")]
        [HarmonyPatch]
        static void HandleUpdate_Prefix()
        {

        }
    }
}
