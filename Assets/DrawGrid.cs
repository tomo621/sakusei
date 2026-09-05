using UnityEngine;

public class DrawGrid : MonoBehaviour
{
    public int width = 20;
    public int height = 15;

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 0, 0.3f); 

        // ècê¸
        for (int i = -width; i <= width; i++)
        {
            Gizmos.DrawLine(new Vector3(i - 0.5f, -height - 0.5f, 0), new Vector3(i - 0.5f, height - 0.5f, 0));
        }
        // â°ê¸
        for (int i = -height; i <= height; i++)
        {
            Gizmos.DrawLine(new Vector3(-width - 0.5f, i - 0.5f, 0), new Vector3(width - 0.5f, i - 0.5f, 0));
        }
    }
}