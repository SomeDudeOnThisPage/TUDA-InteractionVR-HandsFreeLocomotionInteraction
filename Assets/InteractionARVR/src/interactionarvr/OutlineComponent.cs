
namespace me.buhlmann.study.ARVR {
  public class OutlineComponent : Outline {
    private bool _enabledFrame;

    public void EnableOutlineForFrame() {
      this._enabledFrame = true;
    }
    
    public void Update() {
      //this.enabled = this._enabledFrame;
      this.OutlineWidth = this._enabledFrame ? 50.0f : 0.0f;
      this._enabledFrame = false;
    }
    
    public void Start() {
      this._enabledFrame = true;
    }
  }
}