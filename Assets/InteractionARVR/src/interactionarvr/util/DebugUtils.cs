using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace me.buhlmann.study.ARVR.util {
  public sealed class DebugUtils {
    private static Dictionary<String, GameObject> _cache;

    public class DebugData {
      [NotNull] public string angle;
      [NotNull] public string angle_normalized;
      [NotNull] public string speed;
      [NotNull] public string step;
      [NotNull] public string return_to_center;
    }
    
    public static void WriteDebugUI(DebugData data) {
      DebugUtils._cache ??= new Dictionary<string, GameObject>();
      
      foreach (var field in typeof(DebugData).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)) {
        if (!DebugUtils._cache.ContainsKey(field.Name)) {
          DebugUtils._cache.Add(field.Name, GameObject.Find("info-debug-" + field.Name.Replace('_', '-')));
        }

        GameObject element = DebugUtils._cache[field.Name];
        element.GetComponent<TextMeshPro>().SetText(field.GetValue(data).ToString());
      }
    }
  }
}