using me.buhlmann.study.ARVR.util;
using UnityEngine;

namespace me.buhlmann.study.ARVR {
  /**
   * Enables hooking functionality into the movement/interaction techniques without tampering with
   * already existing, (more or less) functioning systems.
   */
  public abstract class AbstractPlayerPlugin : ScriptableObject {
    
#if DEBUG
    public abstract void UpdateDebug(Player player, DebugUtils.DebugData debug);
#endif
    
    public abstract void UpdatePlugin(Player player);

    public abstract void Enter(Player player);
    
    public abstract PlayerState GetPlayerState();
    
  }
}