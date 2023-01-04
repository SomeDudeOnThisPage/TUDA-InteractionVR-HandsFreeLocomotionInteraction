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
    private enum _Direction : byte {
      FORWARD = 0x00,
      SIDE = 0x01,
      BACKWARD = 0x02
    }

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
      public float constantZ;
      public float dynamicZ;
      
      public float constantX;
      public float dynamicX;
    }
    
    [SerializeField] private bool _movement;
    [SerializeField] private bool _matchOrientationOnEnter;

    [Header("Zones")]
    [Tooltip("x = 'front', y = 'left/right', z = 'back' - degrees")] 
    [SerializeField] private Vector3 _deadzone;
    
    [SerializeField] private RelativeHeadLocomotion.IncrementStep[] _steps;
    
    [Header("Centering")]
    [SerializeField] private bool _centering;
    [SerializeField] private bool _centeringStationary;

    [SerializeField] private float _centeringDeadzoneAngle;
    [SerializeField] private float _constantAggressivenessCoefficient;
    [SerializeField] private float _dynamicAggressivenessCoefficient;
    
    private Vector3 _rotationPlayer;
    private float _speed;
    
    public override void Enter(Player player) {
      if (this._matchOrientationOnEnter) {
        MathUtils.CenterPlayerOnViewDirection(player);
      }
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

      float speed = coefficients.constantZ * this._constantAggressivenessCoefficient
                    + coefficients.dynamicZ * this._dynamicAggressivenessCoefficient;

      if (speed == 0.0f && this._centeringStationary) {
        speed = this._constantAggressivenessCoefficient;
      }
      
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
      RelativeHeadLocomotion._Direction index = rotation.z > 0
        ? RelativeHeadLocomotion._Direction.FORWARD
        : RelativeHeadLocomotion._Direction.BACKWARD;
      
      foreach (var step in this._steps) {
        // coefficients := [front, sides, back]
        if (!(step.range[(byte) index] <= Math.Abs(rotation.z))) continue;
        coefficients.constantZ = step.constantCoefficient * Math.Sign(rotation.z);
        coefficients.dynamicZ = step.dynamicCoefficient[(byte) index];
      }
      
      foreach (var step in this._steps) { // head tilted on the x-Axis
        if (!(step.range[(byte) RelativeHeadLocomotion._Direction.SIDE] <= Math.Abs(rotation.x))) continue;
        coefficients.constantX = step.constantCoefficient * Math.Sign(rotation.x);
        coefficients.dynamicX = step.dynamicCoefficient[(byte) RelativeHeadLocomotion._Direction.SIDE];
      }

      return coefficients;
    }
    
#if DEBUG
    public override void UpdateDebug(Player player, DebugUtils.DebugData debug) {
      Vector3 rotation = player.GetCamera().transform.rotation.eulerAngles;
      Vector3 normalized = MathUtils.NormalizeHMDAngles(rotation, this._deadzone);
      RelativeHeadLocomotion._Coefficients coefficients = this.GetCoefficients(normalized);

      debug.step = $"Coefficients: [z (fwd/bck): [{coefficients.constantZ:n2},{coefficients.dynamicZ:n2}], " +
                   $"x (sides): [{coefficients.constantX:n2},{coefficients.dynamicX:n2}]";
      debug.angle_normalized = $"Normalized: [x = {normalized.x:n2}, y = {normalized.y:n2}, z = {normalized.z:n2}]";

      Vector3 scale = new Vector3(
        normalized.x * coefficients.dynamicX + coefficients.constantX,
        0.0f,
        normalized.z * coefficients.dynamicZ + coefficients.constantZ
      );

      //camera forward and right vectors:
      var forward = player.GetCamera().transform.forward;
      var right = player.GetCamera().transform.right;
 
      //project forward and right vectors on the horizontal plane (y = 0)
      forward.y = 0f;
      // right.y = 0f;
      forward.Normalize();
      right.Normalize();

      var move = forward * scale.z + right * scale.x;
      move.y = 0.0f;
      debug.speed = $"Speed: [x: {move.x}, y: {move.y}, z: {move.z}, total: {move.magnitude}] -> /dt";
      
      Vector3 camera = player.GetCamera().transform.forward.normalized;
      camera.y = player.transform.forward.normalized.y;
      debug.return_to_center = $"RTC Angle: {Vector3.Angle(player.transform.forward, camera)}";
      
      // Deadzone FWD
      Debug.DrawRay(player.transform.position, Quaternion.AngleAxis(-this._deadzone.y, Vector3.up) * camera * 5.0f, Color.green, 0.0f);
      Debug.DrawRay(player.transform.position, Quaternion.AngleAxis(this._deadzone.y, Vector3.up) * camera * 5.0f, Color.green, 0.0f);
      
      // FWD
      Debug.DrawRay(player.transform.position, player.transform.forward.normalized * 5.0f, Color.blue, 0.0f);

      Vector3 cameral = -player.GetCamera().transform.right.normalized;
      // Deadzone LEFT
      Debug.DrawRay(player.transform.position, Quaternion.AngleAxis(-this._deadzone.x, Vector3.forward) * cameral * 5.0f, Color.green, 0.0f);
      Debug.DrawRay(player.transform.position, Quaternion.AngleAxis(this._deadzone.x, Vector3.forward) * cameral * 5.0f, Color.green, 0.0f);
      Debug.DrawRay(player.transform.position, -player.transform.right.normalized * 5.0f, Color.blue, 0.0f);
    }
#endif
    
    public override void UpdatePlugin(Player player) {
      Vector3 rotation = player.GetCamera().transform.rotation.eulerAngles;
      Vector3 normalized = MathUtils.NormalizeHMDAngles(rotation, this._deadzone);
      RelativeHeadLocomotion._Coefficients coefficients = this.GetCoefficients(normalized);
      
      Vector3 scale = new Vector3(
        normalized.x * coefficients.dynamicX * Time.deltaTime + coefficients.constantX * Time.deltaTime,
        0.0f,
        normalized.z * coefficients.dynamicZ * Time.deltaTime + coefficients.constantZ * Time.deltaTime
      );

      //camera forward and right vectors:
      var forward = player.GetCamera().transform.forward;
      var right = player.GetCamera().transform.right;
 
      //project forward and right vectors on the horizontal plane (y = 0)
      forward.y = 0f;
      // right.y = 0f;
      forward.Normalize();
      right.Normalize();

      var move = forward * scale.z + right * scale.x;
      move.y = -9.8f * Time.deltaTime;
      if (this._movement) {
        player.controller.Move(move);

        // set speed value without gravity constant
        move.y = 0.0f;
        this._speed = move.magnitude;
      }

      if (this._centering) {
        this.Center(player, coefficients, normalized.z);
      }
    }
  }
}