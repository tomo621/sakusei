using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    public enum Phase { Wait, MoveSelect, AttackSelect, EnemyTurn }
    public Phase currentPhase = Phase.MoveSelect;

    [Header("パラメータ設定")]
    public int moveRange = 3;
    public int attackRange = 1;

    [Header("プレイヤーステータス")]
    public int maxHp = 100;
    public int currentHp = 100;
    public int atk = 30;
    public int def = 10;

    [Header("UI・参照")]
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI statusText;

    [Header("エネミー出現設定")]
    public Enemy enemyPrefab;       
    public int enemySpawnCount = 3; 
    public int spawnAreaX = 8;      
    public int spawnAreaY = 4;      

    // マップ上にいる全ての敵のリストに変更
    private List<Enemy> activeEnemies = new List<Enemy>();

    void Start()
    {
        currentHp = maxHp;
        transform.position = SnapToGrid(transform.position);

        // ゲーム開始時、まず敵をランダムに自動配置する
        SpawnEnemiesRandomly();

        // その後にシーン内の敵をリストに登録
        RefreshEnemyList();

        UpdateStatusUI();
        UpdateTurnUI("【移動フェーズ】\n移動するマスを選択");
    }

    // 敵をランダムな空きマスに生成する関数
    void SpawnEnemiesRandomly()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("Enemy Prefabが設定されていません！");
            return;
        }

        for (int i = 0; i < enemySpawnCount; i++)
        {
            // 重なりを防ぐため最大10回まで空きマスを探す
            for (int attempt = 0; attempt < 10; attempt++)
            {
                int randX = Random.Range(-spawnAreaX, spawnAreaX + 1);
                int randY = Random.Range(-spawnAreaY, spawnAreaY + 1);
                Vector3 spawnPos = new Vector3(randX, randY, 0f);

                // プレイヤーの真上ならやり直し
                if (spawnPos == transform.position) continue;

                // そのマスに既に他のキャラクターがいないかチェック
                Collider2D hit = Physics2D.OverlapPoint(spawnPos);
                if (hit == null)
                {
                    // 何もなければ敵を生成して次の敵の生成へ
                    Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                    break;
                }
            }
        }
    }

    // 敵が倒されるたびにリストを更新する
    void RefreshEnemyList()
    {
        activeEnemies.Clear();
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy e in allEnemies)
        {
            e.transform.position = SnapToGrid(e.transform.position);
            activeEnemies.Add(e);
        }
    }

    void Update()
    {
        // 敵のターン中や待機中は入力を無視
        if (currentPhase == Phase.EnemyTurn || currentPhase == Phase.Wait) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPosition = Mouse.current.position.ReadValue();
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(screenPosition);

            float snapX = Mathf.Round(mousePos.x);
            float snapY = Mathf.Round(mousePos.y);
            Vector2 clickPoint = new Vector2(snapX, snapY);

            // プレイヤーからクリック地点までの距離を計算
            int totalDistance = GetDistance(transform.position, clickPoint);

            //  移動フェーズの処理
            if (currentPhase == Phase.MoveSelect)
            {
                if (totalDistance <= moveRange)
                {
                    Collider2D hitCollider = Physics2D.OverlapPoint(clickPoint);
                    // 敵がいない場所なら移動して攻撃フェーズへ
                    if (hitCollider == null || hitCollider.gameObject == gameObject)
                    {
                        transform.position = new Vector3(snapX, snapY, 0f);
                        currentPhase = Phase.AttackSelect;
                        UpdateTurnUI("【攻撃フェーズ】\n攻撃対象を選択\n(何もない場所で待機)");
                    }
                }
            }
            //  攻撃フェーズの処理
            else if (currentPhase == Phase.AttackSelect)
            {
                if (totalDistance <= attackRange)
                {
                    Collider2D hitCollider = Physics2D.OverlapPoint(clickPoint);
                    // クリックしたマスに敵がいればダメージ計算
                    if (hitCollider != null && hitCollider.GetComponent<Enemy>() != null)
                    {
                        // ダメージ計算: 自分の攻撃力 - 敵の防御力 
                        Enemy targetEnemy = hitCollider.GetComponent<Enemy>();
                        int damage = Mathf.Max(1, atk - targetEnemy.def);
                        targetEnemy.TakeDamage(damage);

                        if (targetEnemy.currentHp <= 0)
                        {
                            Destroy(targetEnemy.gameObject);
                            UpdateTurnUI("敵を撃破しました！\nSTAGE CLEAR");
                            currentPhase = Phase.Wait;
                            Invoke("RefreshEnemyList", 0.1f);
                            Invoke("GoToResult", 1.5f);
                            return;
                        }
                    }
                }

                // 攻撃したまたは何もないマスをクリックして待機した場合は敵のターンへ
                StartCoroutine(EnemyTurnRoutine());
            }
        }
    }

    // 距離を計算する関数
    int GetDistance(Vector3 a, Vector2 b)
    {
        return Mathf.RoundToInt(Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y));
    }

    private Vector3 SnapToGrid(Vector3 rawPos)
    {
        return new Vector3(Mathf.Round(rawPos.x), Mathf.Round(rawPos.y), 0f);
    }

    private void UpdateTurnUI(string message)
    {
        if (turnText != null) turnText.text = message;
    }

    // ステータスの数字を変数と連動させる
    private void UpdateStatusUI()
    {
        if (statusText != null)
        {
            statusText.text = $"ユニット：自軍\nHP：{currentHp} / {maxHp}\nATK: {atk}  DEF: {def}\n移動力：{moveRange}";
        }
    }

    // 敵のターンを管理する
    private IEnumerator EnemyTurnRoutine()
    {
        currentPhase = Phase.EnemyTurn;
        UpdateTurnUI("エネミーのターン...");

        yield return new WaitForSeconds(1.0f);

        RefreshEnemyList();

        foreach (Enemy enemy in activeEnemies)
        {
            if (enemy == null) continue;

            int distToPlayer = GetDistance(enemy.transform.position, transform.position);

            // 隣接していなければ1マス近づく
            if (distToPlayer > 1)
            {
                float diffX = transform.position.x - enemy.transform.position.x;
                float diffY = transform.position.y - enemy.transform.position.y;
                Vector3 newEnemyPos = enemy.transform.position;

                if (Mathf.Abs(diffX) > Mathf.Abs(diffY))
                {
                    newEnemyPos.x += Mathf.Sign(diffX);
                }
                else
                {
                    newEnemyPos.y += Mathf.Sign(diffY);
                }

                Collider2D hit = Physics2D.OverlapPoint(newEnemyPos);
                if (hit == null || hit.gameObject == gameObject)
                {
                    enemy.transform.position = newEnemyPos;
                }

                // 移動後に距離を再計算
                distToPlayer = GetDistance(enemy.transform.position, transform.position);
                yield return new WaitForSeconds(0.5f);
            }

            // 距離がならプレイヤーを攻撃する
            if (distToPlayer <= 1)
            {
                int damage = Mathf.Max(1, enemy.atk - def);
                currentHp -= damage;
                if (currentHp < 0) currentHp = 0;

                // 減ったHPを画面に反映
                UpdateStatusUI();
                if (currentHp <= 0)
                {
                    UpdateTurnUI("GAME OVER...");
                    yield break;
                }
            }
        }

        RefreshEnemyList();
        if (activeEnemies.Count == 0)
        {
            UpdateTurnUI("すべての敵を撃破！\nSTAGE CLEAR");
            Invoke("GoToResult", 1.5f);
            yield break;
        }

        yield return new WaitForSeconds(0.5f);

        // プレイヤーのターンに戻す
        currentPhase = Phase.MoveSelect;
        UpdateTurnUI("【移動フェーズ】\n移動するマスを選択");
    }

    private void GoToResult()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Result");
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // フェーズによって色と描画範囲を変える
        if (currentPhase == Phase.MoveSelect)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
            DrawGridArea(moveRange);
        }
        else if (currentPhase == Phase.AttackSelect)
        {
            // 赤色（攻撃範囲）
            Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
            DrawGridArea(attackRange);
        }
    }

    // 範囲を描画する
    void DrawGridArea(int range)
    {
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) <= range)
                {
                    Vector3 tilePos = new Vector3(transform.position.x + x, transform.position.y + y, 0);
                    Gizmos.DrawCube(tilePos, new Vector3(1, 1, 0.1f));
                }
            }
        }
    }
}