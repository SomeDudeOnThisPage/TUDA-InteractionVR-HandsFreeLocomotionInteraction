using me.buhlmann.study.ARVR.util;
using UnityEngine;

namespace me.buhlmann.study.ARVR {
  [CreateAssetMenu(menuName = "InteractionARVR/RelativeHeadRotationInteractionRotation")]
  public class RelativeHeadInteractionRotation : AbstractPlayerPlugin {
    [SerializeField] private Vector3 _deadzone;
    [SerializeField] private float _speed;

    private Quaternion _enter;
    private GameObject _object;
    
    public override void UpdateDebug(Player player, DebugUtils.DebugData debug) {
    }

    public override void UpdatePlugin(Player player) {
      if (this._object == null) return;
      
      Vector3 rotation = player.GetCamera().transform.rotation.eulerAngles;
      // Factor in player rotation for y, as this is also dependant on initial player direction.
      rotation.y = Mathf.Repeat(rotation.y - player.transform.rotation.eulerAngles.y, 360.0f);
      Vector3 normalized = MathUtils.NormalizeHMDAngles(rotation, this._deadzone);

      // Invert the z- and x-axis here to make HMD pitch rotate an object around its' x-axis.
      // This matches the visual tilt direction with the direction of the rotation.
      Vector3 rotate = new Vector3(
        normalized.z,
        normalized.y,
        normalized.x
      );
      this._object.transform.Rotate(rotate * Time.deltaTime * this._speed, Space.Self);

      //this._object.transform.Rotate(rotate * Time.deltaTime * this._speed);
    }

    public override void Enter(Player player) {
      MathUtils.CenterPlayerOnViewDirection(player);
      
      this._object = player.GetStateManager().GetInteractable();
      this._enter = player.GetStateManager().GetInteractable().transform.rotation;

      if (this._object == null) {
        Debug.LogError("Interactable object is null!");
      }
    }

    public override PlayerState GetPlayerState() {
      return PlayerState.INTERACT_ROTATE;
    }
  }
}