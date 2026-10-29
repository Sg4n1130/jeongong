using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuClick : MonoBehaviour
{
    public bool isPlayButton;

    private void OnMouseDown()
    {
        if (isPlayButton)
        {
            SceneManager.LoadScene("SampleScene");
        }
        else
        {
            Debug.Log("게임 종료!");
            Application.Quit();
        }
    }
}