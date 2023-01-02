using UnityEngine;

namespace me.buhlmann.study.ARVR {
  public class PlayerStateManager {
    
    private PlayerState _current;
    private RaycastHit _raycast;

    public PlayerStateManager() {
      this._current = PlayerState.DEFAULT;
    }

    public void FixedUpdateRaycast(Player player) {
      const int mask = 1 << 8;

      Vector3 origin = player.GetCamera().transform.position;
      Vector3 direction = player.GetCamera().transform.transform.TransformDirection(Vector3.forward);

      Physics.Raycast(origin, direction, out this._raycast, Mathf.Infinity, ~mask);

      if (this._raycast.transform != null && this._raycast.transform.gameObject != null) {
        GameObject hit = this._raycast.transform.gameObject;
        if (hit.GetComponent<OutlineComponent>() != null) {
          // hit.GetComponent<OutlineComponent>().enabled = false;
          // Debug.Log(hit);
        }
      }
    }
    
    public PlayerState Get() {
      return this._current;
    }
    
    public void OnInputUp(Player player) {
    }
    
    public void OnInputDown(Player player) {
      switch (this._current) {
        case PlayerState.DEFAULT:
          this._current = PlayerState.MOVEMENT;
          player.GetActiveLocomotionPlugin().Enter(player);
          break;
        case PlayerState.MOVEMENT:
          this._current = PlayerState.DEFAULT;
          break;
      }
    }
  }
}