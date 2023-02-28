using me.buhlmann.study.ARVR.util;
using Unity.Collections;
using UnityEngine;

namespace me.buhlmann.study.ARVR {
  public abstract class PlayerStateManager : ScriptableObject {
    [ReadOnly] [SerializeField] protected PlayerState _current;
    protected Player _player;

    public abstract GameObject GetInteractable();
    public virtual void Update(OVRInput.Controller[] controllers) {}
    public virtual void FixedUpdate() {}


#if DEBUG
    public virtual void UpdateDebug(DebugUtils.DebugData debug) {}
#endif
    
    public void SetPlayer(Player player) {
      this._player = player;
    }
    
    public PlayerState Get() {
      return this._current;
    }
  }
}