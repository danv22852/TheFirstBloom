using UnityEngine;
using Unity.Cinemachine;
public class MapTransition : MonoBehaviour { 
    [SerializeField] private PolygonCollider2D mapBoundary;
    private CinemachineConfiner2D confiner;
    [SerializeField] Direction direction;

    [SerializeField] float moveDistance = 1f;
    enum Direction { Up, Down, Left, Right }

    private void Awake() { 
        confiner = FindFirstObjectByType<CinemachineConfiner2D>();
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) { 
            confiner.BoundingShape2D = mapBoundary;
            confiner.InvalidateBoundingShapeCache();
            UpdatePlayerPosition(collision.gameObject);
            }
        }

    private void UpdatePlayerPosition(GameObject player) { 
        Vector3 newPosition = player.transform.position;
        switch (direction) { 
            case Direction.Up:
                newPosition.y += moveDistance; 
                break;
            case Direction.Down:
                newPosition.y -= moveDistance; 
                break;
            case Direction.Left:
                newPosition.x -= moveDistance; 
                break;
            case Direction.Right:
                newPosition.x += moveDistance; 
                break;
        }
        player.transform.position = newPosition;
    }
}