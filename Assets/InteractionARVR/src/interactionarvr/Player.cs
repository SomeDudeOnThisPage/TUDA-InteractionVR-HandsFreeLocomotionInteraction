using me.buhlmann.study.ARVR.util;
using UnityEngine;

namespace me.buhlmann.study.ARVR {
  public class Player : MonoBehaviour {
    
    [SerializeField] private AbstractPlayerPlugin _locomotion;
    [SerializeField] private AbstractPlayerPlugin _translation;
    [SerializeField] private AbstractPlayerPlugin _rotation;

    [SerializeField] private PlayerStateManager _manager;

    [SerializeField] private OVRInput.Controller[] _controllers;

    [SerializeField] private GameObject _camera;
    
    [SerializeField] private GameObject _rig;

    [SerializeField] private GameObject _debug;
    [SerializeField] private GameObject _crosshair;

    [SerializeField] public CharacterController controller;
    
    public GameObject GetOVRRig() {
      return this._rig;
    }
    
    public GameObject GetCamera() {
      return this._camera;
    }

    public AbstractPlayerPlugin GetActiveLocomotionPlugin() {
      return this._locomotion;
    }
    
    public AbstractPlayerPlugin GetActiveTranslationPlugin() {
      return this._translation;
    }

    public AbstractPlayerPlugin GetActiveInteractionRotationPlugin() {
      return this._rotation;
    }
    
    public PlayerStateManager GetStateManager() {
      return this._manager;
    }

    public void FixedUpdate() {
      this._manager.FixedUpdate();
      Crosshair3D.FixedUpdate(this, this._crosshair);
    }
    
    public void Update() {
      this._manager.Update();
      // bool lb = OVRInput.Get(OVRInput.Button.Any, this._controllers[0]); 
      // bool rb = OVRInput.Get(OVRInput.Button.Any, this._controllers[0]);
      
      if (this._locomotion.GetPlayerState() == this._manager.Get()) {
        this._locomotion.UpdatePlugin(this);
      }
      
      if (this._translation.GetPlayerState() == this._manager.Get()) {
        this._translation.UpdatePlugin(this);
      }
      
      if (this._rotation.GetPlayerState() == this._manager.Get()) {
        this._rotation.UpdatePlugin(this);
      }

#if DEBUG
      Vector3 rotation = this._camera.transform.rotation.eulerAngles;
      DebugUtils.DebugData debug = new DebugUtils.DebugData() {
        angle = $"Angle: [x: {rotation.x:n2}, y: {rotation.y:n2}, z: {rotation.z:n2}]",
        angle_normalized = "",
        speed = "Speed: 0.0f",
        return_to_center = "",
        step = ""
      };
      this._manager.UpdateDebug(debug);
      this._locomotion.UpdateDebug(this, debug);
      this._rotation.UpdateDebug(this, debug);
      DebugUtils.WriteDebugUI(debug);
#endif
    }

    public void Start() {
      this._manager.SetPlayer(this);
    }
    
    [SerializeField] public ParkourCounter parkourCounter;
    [SerializeField] public string stage;
    [SerializeField] public SelectionTaskMeasure selectionTaskMeasure;
    
    void OnTriggerEnter(Collider other)
    {
      Debug.Log(other);

      // These are for the game mechanism.
      if (other.CompareTag("banner"))
      {
        stage = other.gameObject.name;
        parkourCounter.isStageChange = true;
      }
      else if (other.CompareTag("objectInteractionTask"))
      {
        selectionTaskMeasure.isTaskStart = true;
        selectionTaskMeasure.scoreText.text = "";
        selectionTaskMeasure.partSumErr = 0f;
        selectionTaskMeasure.partSumTime = 0f;
        // rotation: facing the user's entering direction
        float tempValueY = other.transform.position.y > 0 ? 12 : 0;
        Vector3 tmpTarget = new Vector3(this.transform.position.x, tempValueY, this.transform.position.z);
        selectionTaskMeasure.taskUI.transform.LookAt(tmpTarget);
        selectionTaskMeasure.taskUI.transform.Rotate(new Vector3(0, 180f, 0));
        selectionTaskMeasure.taskStartPanel.SetActive(true);
      }
      else if (other.CompareTag("coin"))
      {
        parkourCounter.coinCount += 1;
        //this.GetComponent<AudioSource>().Play();
        other.gameObject.SetActive(false);
      }
      // These are for the game mechanism.
    }
  }
}

