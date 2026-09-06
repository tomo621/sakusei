using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("エネミーステータス")]
    public int maxHp = 50;
    public int currentHp = 50;
    public int atk = 20;
    public int def = 5;

    // ダメージを受ける処理
    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        if (currentHp < 0) currentHp = 0;
    }
}