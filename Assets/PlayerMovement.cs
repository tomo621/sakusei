using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public int moveRange = 3;

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPosition = Mouse.current.position.ReadValue();
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(screenPosition);

            float snapX = Mathf.Floor(mousePos.x) + 0.5f;
            float snapY = Mathf.Floor(mousePos.y) + 0.5f;

            // クリックしたマスとの距離
            float distanceX = Mathf.Abs(snapX - transform.position.x);
            float distanceY = Mathf.Abs(snapY - transform.position.y);
            int totalDistance = Mathf.RoundToInt(distanceX + distanceY);

            if (totalDistance <= moveRange)
            {
                // ★クリックしたマスに何かあるか調べる
                Vector2 clickPoint = new Vector2(snapX, snapY);
                Collider2D hitCollider = Physics2D.OverlapPoint(clickPoint);

                if (hitCollider != null)
                {
                    // もしクリックした場所に「Enemy」がいたら攻撃！
                    Enemy enemy = hitCollider.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        Debug.Log("敵を攻撃して倒した！");
                        Destroy(enemy.gameObject); // 敵を消滅させる
                    }
                }
                else
                {
                    // 何もなければ通常通り移動する
                    transform.position = new Vector3(snapX, snapY, 0f);
                }
            }
            else
            {
                Debug.Log("そこには届きません！");
            }
        }
    }
}