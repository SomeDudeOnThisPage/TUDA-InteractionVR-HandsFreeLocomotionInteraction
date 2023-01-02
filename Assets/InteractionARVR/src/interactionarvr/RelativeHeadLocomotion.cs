using System;

using me.buhlmann.study.ARVR.util;
using UnityEngine;

namespace me.buhlmann.study.ARVR {
  /**
   * Scriptable Object to enable quick testing/resetting to earlier values by simply swapping
   * the RelativeHeadLocomotion-Scriptable inside the Player-Object's explorer.
   */
  [CreateAssetMenu(menuName = "InteractionARVR/RelativeHeadLocomotion")]
  public class RelativeHeadLocomotion : AbstractPlayerPlugin {
    private const int FORWARD = 0;
    private const int SIDE = 1;
    private const int BACKWARD = 2;

    [System.Serializable]
    public struct IncrementStep {
      [Tooltip("Degrees additive to Deadzone-Values. I.e. 0 == deadzone + 0deg, 1 == deadzone + 1deg, etc. " +
               "Acts as the 'starting-value' for the step. If a step with a range higher than the current" +
               "step exists, and the angle of the HMD is greater than its' range value, it is used instead. " +
               "Values are like deadzone values: x = 'front', y = 'left/right', z = 'back' - degrees")]
      [SerializeField] public Vector3 range;
      
      [Tooltip("Constant speed coefficient.")]
      [SerializeField] public float constantCoefficient;
      
      [Tooltip("Dynamic speed ramp up coefficient. Based on the angle of the HMD to aggressively modify the actor " +
               "speed with higher angles. Steps with more aggressive angles should lower this value, as otherwise " +
               "this can lead to high speed ramp ups. Which, to be fair, is kind of fun, but also makes you puke." +
               "Values are like deadzone values: x = 'front', y = 'left/right', z = 'back'")]
      [SerializeField] public Vector3 dynamicCoefficient;
    }

    private struct _Coefficients {
      public Vector3 constant;
      public Vector3 dynamic;
    }
    
    [SerializeField] private bool _movement;
    [SerializeField] private bool _matchOrientationOnEnter;

    [Header("Zones")]
    [Tooltip("x = 'front', y = 'left/right', z = 'back' - degrees")] 
    [SerializeField] private Vector3 _deadzone;
    
    [SerializeField] private RelativeHeadLocomotion.IncrementStep[] _steps;
    
    [Header("Centering")]
    [SerializeField] private bool _centering;
    
    [SerializeField] private float _centeringDeadzoneAngle;
    [SerializeField] private float _constantAggressivenessCoefficient;
    [SerializeField] private float _dynamicAggressivenessCoefficient;
    
    private Vector3 _rotationPlayer;

    public override void Enter(Player player) {
      if (!this._matchOrientationOnEnter) return;
      
      Vector3 camera = player.GetCamera().transform.forward.normalized;
      camera.y = player.transform.forward.normalized.y;

      Transform parent = player.GetOVRRig().transform.parent;
      player.GetOVRRig().transform.parent = null;
      player.transform.rotation = Quaternion.LookRotation(
        Vector3.RotateTowards(player.transform.forward, camera, 1000.0f, 0.0f)
      );
      player.GetOVRRig().transform.parent = parent;
    }

    public override PlayerState GetPlayerState() {
      return PlayerState.MOVEMENT;
    }

    private void Center(Player player, RelativeHeadLocomotion._Coefficients coefficients, float angleX) {
      // First, let's define our to be edited direction as the direction of the camera
      Vector3 camera = player.GetCamera().transform.forward.normalized;
      camera.y = player.transform.forward.normalized.y;
      
#if DEBUG
      // Visualize the target direction.
      Debug.DrawRay(player.transform.position, camera * 5.0f, Color.red, 0.0f);
      Debug.DrawRay(player.transform.position, player.transform.forward.normalized * 5.0f, Color.blue, 0.0f);
#endif

      float speed = coefficients.constant[FORWARD] * this._constantAggressivenessCoefficient 
                  + coefficients.dynamic[FORWARD] * this._dynamicAggressivenessCoefficient;
      // Rotate the Player object slowly towards the camera direction.
      // This forces the user to rotate their head the other direction, if they want to stay on their target.
      if (Vector3.Angle(player.transform.forward, camera) >= this._centeringDeadzoneAngle) {
        player.transform.rotation = Quaternion.LookRotation(
          Vector3.RotateTowards(player.transform.forward, camera, speed * Time.deltaTime, 0.0f)
        );
      }
    }
    
    /**
     * Returns the constant and dynamic ramp up coefficients for each axis based on the current rotation.
     */
    private RelativeHeadLocomotion._Coefficients GetCoefficients(Vector3 rotation) {
      RelativeHeadLocomotion._Coefficients coefficients = new RelativeHeadLocomotion._Coefficients();

      // front
      if (rotation.x < 0) {
        foreach (var step in this._steps) { // head tilted forward on x-axis
          if (!(step.range[FORWARD] <= Math.Abs(rotation.x))) continue;
          coefficients.constant[FORWARD] = step.constantCoefficient;
          // use the forward coefficient
          coefficients.dynamic[FORWARD] = step.dynamicCoefficient[FORWARD];
        }
      }
      else if (rotation.x > 0) { // head tilted back on x-axis
        foreach (var step in this._steps) {
          // coefficients := [front, sides, back]
          if (!(step.range[BACKWARD] <= Math.Abs(rotation.x))) continue;
          // we have to invert the constant coefficient based on angle
          // dynamic is always based on angle, so can be ignored!
          coefficients.constant[FORWARD] = -step.constantCoefficient;
          // use the backward coefficient
          coefficients.dynamic[FORWARD] = step.dynamicCoefficient[BACKWARD];
        }
      }
      else {
        coefficients.constant[0] = 0.0f;
        coefficients.dynamic[0] = 0.0f;
      }

      return coefficients;
    }
    
#if DEBUG
    public override void UpdateDebug(Player player, DebugUtils.DebugData debug) {
      Vector3 rotation = player.GetCamera().transform.rotation.eulerAngles;
      Vector3 normalized = MathUtils.NormalizeHMDAngles(rotation, this._deadzone);
      RelativeHeadLocomotion._Coefficients coefficients = this.GetCoefficients(normalized);
      
      debug.step = $"Coefficients: [x: [{coefficients.constant.x:n2},{coefficients.dynamic.x:n2}], " +
                   $"y: [{coefficients.constant.y:n2},{coefficients.dynamic.y:n2}], " +
                   $"z: [{coefficients.constant.z:n2},{coefficients.dynamic.z:n2}]]";
      debug.angle_normalized = $"Normalized: [x = {normalized.x:n2}, y = {normalized.y:n2}, z = {normalized.z:n2}]";

      Vector3 camera = player.GetCamera().transform.forward.normalized;
      camera.y = player.transform.forward.normalized.y;
      debug.return_to_center = $"RTC Angle: {Vector3.Angle(player.transform.forward, camera)}";
    }
#endif
    
    public override void UpdatePlugin(Player player) {
      Vector3 rotation = player.GetCamera().transform.rotation.eulerAngles;
      Vector3 normalized = MathUtils.NormalizeHMDAngles(rotation, this._deadzone);
      RelativeHeadLocomotion._Coefficients coefficients = this.GetCoefficients(normalized);
      
      Vector3 scale = new Vector3(
        normalized.z * coefficients.dynamic.z * Time.deltaTime + coefficients.constant.z * Time.deltaTime,
        0.0f,
        -normalized.x * coefficients.dynamic.x * Time.deltaTime + coefficients.constant.x * Time.deltaTime
      );

      //camera forward and right vectors:
      var forward = player.GetCamera().transform.forward;
      var right = player.GetCamera().transform.right;
 
      //project forward and right vectors on the horizontal plane (y = 0)
      forward.y = 0f;
      right.y = 0f;
      forward.Normalize();
      right.Normalize();

      var desiredMoveDirection = forward * scale.z + right * scale.x;
      desiredMoveDirection.y = -9.8f * Time.deltaTime;
      if (this._movement) {
        player.controller.Move(desiredMoveDirection);
      }

      if (this._centering) {
        this.Center(player, coefficients, normalized.z);
      }
    }
  }
}