using UnityEngine;
using UnityEngine.SceneManagement; 

public class SceneController : MonoBehaviour
{
    // タイトル画面のStartボタンに設定する
    public void GoToMainBattle()
    {
        SceneManager.LoadScene("MainBattle");
    }

    // リザルト画面からタイトルに戻る
    public void GoToTitle()
    {
        SceneManager.LoadScene("Title");
    }
}