using UnityEngine;

public class CardSwitcher : MonoBehaviour
{
    // 切り替えるカードオブジェクト（インスペクターで登録）
    public GameObject[] cards;
    private int currentIndex = 0;

    // 次のカードを表示
    public void NextCard()
    {
        // 現在のカードを非表示
        cards[currentIndex].SetActive(false);

        // 次のインデックスに
        currentIndex = (currentIndex + 1) % cards.Length;

        // 新しいカードを表示
        cards[currentIndex].SetActive(true);
    }

    // 前のカードを表示（必要なら追加）
    public void PreviousCard()
    {
        cards[currentIndex].SetActive(false);
        currentIndex = (currentIndex - 1 + cards.Length) % cards.Length;
        cards[currentIndex].SetActive(true);
    }
}
