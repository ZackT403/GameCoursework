using UnityEngine;

public class RandomColor : MonoBehaviour
{
    void Start()
    {
        // picks a random color
        float r = Random.Range(0.7f, 1f);
        float g = Random.Range(0.7f, 1f);
        float b = Random.Range(0.7f, 1f);
        GetComponent<SpriteRenderer>().color = new Color(r, g, b);
    }
}