using me.buhlmann.study.ARVR.util;
using UnityEngine;

namespace me.buhlmann.study.ARVR {
  /**
   * "Go-Go"-esque approach to object translation via HMD angles.
   */
  [CreateAssetMenu(menuName = "InteractionARVR/RelativeHeadRotationInteractionTranslation")]
  public class RelativeHeadInteractionTranslation : AbstractPlayerPlugin {
    [SerializeField] private Vector3 _deadzone;

    private GameObject _object;
    private float _distance;

    public override void UpdateDebug(Player player, DebugUtils.DebugData debug) {
      // throw new System.NotImplementedException();
    }

    public override void UpdatePlugin(Player player) {
      Vector3 rotation = player.GetCamera().transform.rotation.eulerAngles;
      Vector3 normalized = MathUtils.NormalizeHMDAngles(rotation, this._deadzone);

      // Modify _distance based on HMD roll.
      // Roll, because that is the only value we don't need for positioning.
      this._distance += normalized.x * Time.deltaTime;
      
      // Position object along forward vector with distance _distance.
      Vector3 forward = player.GetCamera().transform.forward;
      this._object.transform.position = player.transform.position + forward.normalized * this._distance;
    }

    public override void Enter(Player player) {
      MathUtils.CenterPlayerOnViewDirection(player);
      this._object = player.GetStateManager().GetInteractable();
      this._distance = Vector3.Distance(player.transform.position, this._object.transform.position);
      
      if (this._object == null) {
        Debug.LogError("Interactable object is null!");
      }
    }

    public override PlayerState GetPlayerState() {
      return PlayerState.INTERACT_TRANSLATE;
    }
  }
}