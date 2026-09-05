using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public int moveRange = 3;
    public TextMeshProUGUI statusText;
    public Transform enemyTransform;

    private bool isPlayerTurn = true;

    void Start()
    {
        // 1. マス目の中央に生成
        transform.position = SnapToGrid(transform.position);
        if (enemyTransform != null)
        {
            enemyTransform.position = SnapToGrid(enemyTransform.position);
        }

        UpdateUI("プレイヤーのターン\n行動を選択");
    }

    void Update()
    {
        // 敵のターン中は操作不可
        if (!isPlayerTurn) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPosition = Mouse.current.position.ReadValue();
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(screenPosition);

            // クリックした位置をマス目単位に変換
            float snapX = Mathf.Round(mousePos.x);
            float snapY = Mathf.Round(mousePos.y);

            // 移動距離の計算
            float distanceX = Mathf.Abs(snapX - transform.position.x);
            float distanceY = Mathf.Abs(snapY - transform.position.y);
            int totalDistance = Mathf.RoundToInt(distanceX + distanceY);

            if (totalDistance <= moveRange)
            {
                Vector2 clickPoint = new Vector2(snapX, snapY);
                Collider2D hitCollider = Physics2D.OverlapPoint(clickPoint);

                if (hitCollider != null)
                {
                    // 敵がいた場合は撃破
                    if (hitCollider.GetComponent<Enemy>() != null)
                    {
                        Destroy(hitCollider.gameObject);
                        UpdateUI("敵を撃破しました！\nSTAGE CLEAR");
                        isPlayerTurn = false;
                    }
                }
                else
                {
                    // 移動して敵のターンへ
                    transform.position = new Vector3(snapX, snapY, 0f);
                    StartCoroutine(EnemyTurnRoutine());
                }
            }
        }
    }

    private Vector3 SnapToGrid(Vector3 rawPos)
    {
        // 座標を四捨五入してマスの中央に合わせる
        return new Vector3(Mathf.Round(rawPos.x), Mathf.Round(rawPos.y), 0f);
    }

    private void UpdateUI(string message)
    {
        if (statusText != null) statusText.text = message;
    }

    //  敵のターン処理と行動AI
    private IEnumerator EnemyTurnRoutine()
    {
        isPlayerTurn = false;
        UpdateUI("エネミーのターン...");

        yield return new WaitForSeconds(1.0f); 

        // 敵の行動プレイヤーに向かって1マス移動する
        if (enemyTransform != null)
        {
            float diffX = transform.position.x - enemyTransform.position.x;
            float diffY = transform.position.y - enemyTransform.position.y;

            Vector3 newEnemyPos = enemyTransform.position;

            // XとY、より遠い方の軸に向かって1マス進む
            if (Mathf.Abs(diffX) > Mathf.Abs(diffY))
            {
                newEnemyPos.x += Mathf.Sign(diffX);
            }
            else
            {
                newEnemyPos.y += Mathf.Sign(diffY);
            }

            // 移動を反映
            enemyTransform.position = newEnemyPos;
        }

        yield return new WaitForSeconds(0.5f); 

        isPlayerTurn = true;
        UpdateUI("プレイヤーのターン\n行動を選択");
    }

    // 移動範囲を視覚的に水色で表示する処理
    void OnDrawGizmos()
    {
        // プレイヤーのターンの時だけ表示
        if (!Application.isPlaying || !isPlayerTurn) return;

        // 色の設定水色で半透明
        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);

        // 移動力の範囲内でマス目をチェック
        for (int x = -moveRange; x <= moveRange; x++)
        {
            for (int y = -moveRange; y <= moveRange; y++)
            {
                // 現在地からの距離が移動力以下なら描画
                if (Mathf.Abs(x) + Mathf.Abs(y) <= moveRange)
                {
                    Vector3 tilePos = new Vector3(transform.position.x + x, transform.position.y + y, 0);
                    // 1x1のサイズのキューブを描画してマスを塗る
                    Gizmos.DrawCube(tilePos, new Vector3(1, 1, 0.1f));
                }
            }
        }
    }
}