using UnityEngine;

public class CardSwitcher : MonoBehaviour
{
    // �؂�ւ���J�[�h�I�u�W�F�N�g�i�C���X�y�N�^�[�œo�^�j
    public GameObject[] cards;
    private int currentIndex = 0;

    // ���̃J�[�h��\��
    public void NextCard()
    {
        // ���݂̃J�[�h���\��
        cards[currentIndex].SetActive(false);

        // ���̃C���f�b�N�X��
        currentIndex = (currentIndex + 1) % cards.Length;

        // �V�����J�[�h��\��
        cards[currentIndex].SetActive(true);
    }

    // �O�̃J�[�h��\���i�K�v�Ȃ�ǉ��j
    public void PreviousCard()
    {
        cards[currentIndex].SetActive(false);
        currentIndex = (currentIndex - 1 + cards.Length) % cards.Length;
        cards[currentIndex].SetActive(true);
    }
}
