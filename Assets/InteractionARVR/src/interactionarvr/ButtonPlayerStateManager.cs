using me.buhlmann.study.ARVR.util;
using UnityEngine;

namespace me.buhlmann.study.ARVR {
  [CreateAssetMenu(menuName = "InteractionARVR/ButtonPlayerStateManager")]
  public class ButtonPlayerStateManager : PlayerStateManager {

    [SerializeField] private float _interactionRange;
    
    private RaycastHit _raycast;
    private ClickerCounter _counter;
    private GameObject _interactable;

    public ButtonPlayerStateManager() {
      this._current = PlayerState.DEFAULT;
      this._counter = new ClickerCounter(0.3f);
    }

#if DEBUG
    public override void UpdateDebug(DebugUtils.DebugData debug) {
      debug.state = this._current.ToString();
    }
#endif

    public override void Update(OVRInput.Controller[] controllers) {
      this._counter.Update();
      bool lb = OVRInput.GetDown(OVRInput.Button.One);
      bool rb = OVRInput.GetDown(OVRInput.Button.One);
      if (Input.GetMouseButtonDown(0) || lb || rb) {
        this._counter.OnInput();
      }

      uint count = this._counter.Get();
      if (count == 0u) {
        return;
      }
      
      switch (this._current) {
        case PlayerState.DEFAULT:
          if (count == 1u) {
            this._current = PlayerState.MOVEMENT;
            this._player.GetActiveLocomotionPlugin().Enter(this._player);
          }
          break;
        case PlayerState.DEFAULT_CAN_INTERACT:
          if (this._interactable.GetComponent<InteractionComponent>().IsSimpleInteractable()) {
            this._interactable.GetComponent<InteractionComponent>().SimpleInteraction();
            return;
          }
          if (count == 1u) {
            this._current = PlayerState.INTERACT_TRANSLATE;
            this._player.GetActiveTranslationPlugin().Enter(this._player);
          } else if (count == 2u) {
            this._current = PlayerState.INTERACT_ROTATE;
            this._player.GetActiveInteractionRotationPlugin().Enter(this._player);
          }
          break;
        case PlayerState.MOVEMENT:
          this._current = PlayerState.DEFAULT;
          break;
        case PlayerState.INTERACT_TRANSLATE:
          if (count == 1u) {
            this._current = PlayerState.INTERACT_ROTATE;
            this._player.GetActiveInteractionRotationPlugin().Enter(this._player);
          } else if (count == 2u) {
            this._current = PlayerState.DEFAULT;
          }
          break;
        case PlayerState.INTERACT_ROTATE:
          this._current = count switch {
            1 => PlayerState.DEFAULT,
            2 => PlayerState.DEFAULT,
            _ => this._current
          };
          break;
      }
    }

    public bool CanInteract() {
      return this._current == PlayerState.DEFAULT_CAN_INTERACT;
    }

    public override GameObject GetInteractable() {
      return this._interactable;
    }
    
    public override void FixedUpdate() {
      const int mask = 1 << 2;

      Vector3 origin = this._player.GetCamera().transform.position;
      Vector3 direction = this._player.GetCamera().transform.transform.TransformDirection(Vector3.forward);

      Physics.Raycast(origin, direction, out this._raycast, this._interactionRange, ~mask);

      if (this._raycast.transform != null && this._raycast.transform.gameObject != null) {
        GameObject hit = this._raycast.transform.gameObject;
        if (hit.GetComponent<InteractionComponent>() != null) {
          if (this._current is PlayerState.DEFAULT or PlayerState.DEFAULT_CAN_INTERACT) {
            this._current = PlayerState.DEFAULT_CAN_INTERACT;
            this._interactable = hit;
          }
        } else if (this._current is PlayerState.DEFAULT or PlayerState.DEFAULT_CAN_INTERACT) {
          this._current = PlayerState.DEFAULT;
          this._interactable = null;
        }
      } else if (this._current == PlayerState.DEFAULT_CAN_INTERACT) {
        this._current = PlayerState.DEFAULT;
        this._interactable = null;
      }
    }
  }
}