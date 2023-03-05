using UnityEngine;

public class FinishPortal : MonoBehaviour {
   public void OnTriggerEnter2D(Collider2D collision) {
      if (collision.gameObject.layer != 3)
         return;

      GameManager.Instance.StartNewRun();
   }
}
