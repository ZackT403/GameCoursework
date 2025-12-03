using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float panSpeed = 20f;
    public float zoomSpeed = 2f;

    [Header("Limits")]
    public Tilemap mapRenderer;
    private float minX, maxX, minY, maxY;

    [Header("Zoom Limits")]
    public float minZoom = 5f;
    public float maxZoom = 20f;
    
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        // calc map bounds
        Vector3 mapSize = mapRenderer.localBounds.extents; 
        Vector3 mapCenter = mapRenderer.localBounds.center;

        minX = mapCenter.x - mapSize.x;
        maxX = mapCenter.x + mapSize.x;
        minY = mapCenter.y - mapSize.y;
        maxY = mapCenter.y + mapSize.y;
    }

    void Update()
    {
        // handle camera movemnet every frame
        MoveCamera();
        ZoomCamera();
    }

    void MoveCamera()
    {
        float xMove = 0f;
        float yMove = 0f;

        
        if (Keyboard.current == null){
            return; 
        }
        // if a or left arrow key is presesed move in that direction
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed){
            xMove = -1f;
        }
        // d or right arrow
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed){
            xMove = 1f;
        }   
        // s or down arrow
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed){
            yMove = -1f;
        } 
        // w or up arrow
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed){
            yMove = 1f;
        } 
        // move the camera based on input and speed
        Vector3 pos = transform.position;
        pos.x += xMove * panSpeed * Time.deltaTime;
        pos.y += yMove * panSpeed * Time.deltaTime;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;
    }

    void ZoomCamera()
    {
        // dont zoom if pointer is on ui
        if (EventSystem.current.IsPointerOverGameObject()){
          return;  
        }

        float scroll = 0f;
        // if sccroll wheel moved adjust zoom
        if (Mouse.current != null)
        {
            scroll = Mouse.current.scroll.ReadValue().y;

            if (Mathf.Abs(scroll) > 0){
                scroll = Mathf.Sign(scroll);
            } 
        }
        else
        {
            scroll = Input.GetAxis("Mouse ScrollWheel");
        }

        if (scroll != 0)
        {
            // change orthographic size based on scroll input
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }
}