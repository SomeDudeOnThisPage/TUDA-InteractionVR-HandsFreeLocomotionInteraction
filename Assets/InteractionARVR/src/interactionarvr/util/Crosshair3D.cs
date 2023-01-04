using UnityEngine;

namespace me.buhlmann.study.ARVR.util {
  public class Crosshair3D {
    public static void FixedUpdate(Player player, GameObject crosshair) {
      const int mask = 1 << 2 /* ignore raycast layer */;
      RaycastHit hit = new RaycastHit();
      Vector3 origin = player.GetCamera().transform.position;
      Vector3 direction = player.GetCamera().transform.transform.TransformDirection(Vector3.forward);

      Physics.Raycast(origin, direction, out hit, Mathf.Infinity, ~mask);
      if (hit.transform != null && hit.transform.gameObject != null) {
        crosshair.transform.position = hit.point;
      }
    }
  }
}