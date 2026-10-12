using UnityEngine;

public class SkyFollow : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private Vector2 position;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Update()
    {
        position = new Vector2(player.transform.position.x, player.transform.position.y);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = position;
    }
}
