using TMPro;
using UnityEngine;

public class CoinsDisplay : MonoBehaviour {
   [field: SerializeField] public GameObject Panel { get; private set; }

   [SerializeField] private TextMeshProUGUI _textMesh;

   public void SetCoins(int count)
      => _textMesh.text = count.ToString();

   public int GetCoins()
      => int.Parse(_textMesh.text);
}
