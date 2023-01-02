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
      return new Vector3(
        MathUtils.NormalizeHMDAngle(degrees.x, deadzone.Value.x, deadzone.Value.z),
        MathUtils.NormalizeHMDAngle(degrees.y, deadzone.Value.y, deadzone.Value.y),
        // RelativeHeadLocomotion.Normalize(degrees.z, deadzone.Value.z)
        0.0f
      );
    }
  }
}