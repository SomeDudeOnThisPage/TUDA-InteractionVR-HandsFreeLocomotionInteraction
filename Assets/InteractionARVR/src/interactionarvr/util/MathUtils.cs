using UnityEngine;

namespace me.buhlmann.study.ARVR.util {
  public class MathUtils {
    private static float NormalizeHMDAngle(float degrees, float deadzonex, float deadzoney) {
      return degrees switch {
        // normalize value between (-180, 180)
        < 180.0f when degrees > deadzonex => -(degrees - deadzonex),
        >= 180.0f when degrees < 360.0f - deadzoney => (360.0f - degrees - deadzoney),
        _ => 0
      };
    }

    public static Vector3 NormalizeHMDAngles(Vector3 degrees, Vector3? deadzone = null) {
      deadzone ??= new Vector3(0.0f, 0.0f, 0.0f);
      // Headset-Angles to our System -> HMD.x = -z, HMD.z = x
      return new Vector3(
        MathUtils.NormalizeHMDAngle(degrees.z, deadzone.Value.z, deadzone.Value.z),
        MathUtils.NormalizeHMDAngle(degrees.y, deadzone.Value.y, deadzone.Value.y),
        -MathUtils.NormalizeHMDAngle(degrees.x, deadzone.Value.x, deadzone.Value.x)
      );
    }

    public static void CenterPlayerOnViewDirection(Player player) {
      Vector3 camera = player.GetCamera().transform.forward.normalized;
      camera.y = player.transform.forward.normalized.y;

      Transform parent = player.GetOVRRig().transform.parent;
      player.GetOVRRig().transform.parent = null;
      player.transform.rotation = Quaternion.LookRotation(
        Vector3.RotateTowards(player.transform.forward, camera, 1000.0f, 0.0f)
      );
      player.GetOVRRig().transform.parent = parent;
    }
  }
}