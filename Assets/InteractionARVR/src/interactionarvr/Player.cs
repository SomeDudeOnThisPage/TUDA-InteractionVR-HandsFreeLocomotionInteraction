using me.buhlmann.study.ARVR.util;
using TMPro;
using UnityEngine;

namespace me.buhlmann.study.ARVR {
  public class Player : MonoBehaviour {
    
    [SerializeField] private AbstractPlayerPlugin _locomotion;
    // [SerializeField] private readonly InteractionTechnique _interaction;
    
    [SerializeField] private OVRInput.Controller[] _controllers;

    [SerializeField] private GameObject _camera;
    
    [SerializeField] private GameObject _rig;

    [SerializeField] private GameObject _debug;

    [SerializeField] public CharacterController controller;

    private PlayerStateManager _manager;
    
    public GameObject GetOVRRig() {
      return this._rig;
    }
    
    public GameObject GetCamera() {
      return this._camera;
    }

    public void FixedUpdate() {
      this._manager.FixedUpdateRaycast(this);
    }

    public AbstractPlayerPlugin GetActiveLocomotionPlugin() {
      return this._locomotion;
    }
    
    public void Update() {
      bool lb = OVRInput.Get(OVRInput.Button.Any, this._controllers[0]); 
      bool rb = OVRInput.Get(OVRInput.Button.Any, this._controllers[0]); 

      if (lb || rb || Input.GetMouseButtonDown(0)) {
        this._manager.OnInputDown(this);
      } else {
        this._manager.OnInputUp(this);
      }
      
      Vector3 rotation = this._camera.transform.rotation.eulerAngles;

      if (this._locomotion.GetPlayerState() == this._manager.Get()) {
        this._locomotion.UpdatePlugin(this);
      }

#if DEBUG
      DebugUtils.DebugData debug = new DebugUtils.DebugData() {
        angle = $"Angle: [x: {rotation.x:n2}, y: {rotation.y:n2}, z: {rotation.z:n2}]",
        angle_normalized = "",
        speed = "Speed: 0.0f",
        return_to_center = "",
        step = ""
      };
      this._locomotion.UpdateDebug(this, debug);
      DebugUtils.WriteDebugUI(debug);
#endif
    }

    public void Start() {
      Debug.Log("Player Script Initialized");
      this._manager = new PlayerStateManager();
    }
  }
}

