using UnityEngine;

namespace me.buhlmann.study.ARVR {
  public class InteractionComponent : MonoBehaviour {
    [SerializeField] private bool _simple;
    public SelectionTaskMeasure selectionTaskMeasure;

    public bool IsSimpleInteractable() {
      return this._simple;
    }

    public void SimpleInteraction() {
      if (this.gameObject.CompareTag("selectionTaskStart")) {
        // bad but idgaf
        if (!selectionTaskMeasure.isCountdown)
        {
          selectionTaskMeasure.isTaskStart = true;
          selectionTaskMeasure.StartOneTask();
        }
      }
      else if (this.gameObject.gameObject.CompareTag("done"))
      {
        selectionTaskMeasure.isTaskStart = false;
        selectionTaskMeasure.EndOneTask();
      }
    }
  }
}