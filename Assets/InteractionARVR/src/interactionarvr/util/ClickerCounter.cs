using UnityEngine;

namespace me.buhlmann.study.ARVR.util {
  public class ClickerCounter {
    private readonly float _delay;
    
    private uint _clicked;
    private uint _last;
    private float _time;

    public ClickerCounter(float delay) {
      this._clicked = 0u;
      this._last = 0u;
      this._time = 0.0f;
      this._delay = delay;
    }

    public void Update() {
      if (this._time + this._delay < Time.time && this._clicked > 0u) {
        Debug.Log("reset timer " + this._clicked);
        this._last = this._clicked;
        this._clicked = 0u;
      }
    }
    
    public void OnInput() {
      this._clicked++;
      this._time = Time.time;
    }

    public uint Get() {
      uint result = this._last;
      this._last = 0u;
      return result;
    }
  }
}