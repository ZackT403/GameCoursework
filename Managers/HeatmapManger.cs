using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.AI;
using System.Collections.Generic;

public class HeatmapManager : MonoBehaviour
{
    public static HeatmapManager Instance;

    [Header("References")]
    public Tilemap groundTilemap;   
    public Tilemap overlayTilemap; 
    
    [Header("Heatmap Colors")]
    public Color coldColor = new Color(0f, 0f, 0.5f, 0.8f); 
    public Color hotColor = new Color(1f, 0f, 0f, 0.8f);  
    
    [Header("Sensitivity Settings")]
    public float heatGainPerSecond = 0.5f; // how long it takes to go from hot to cold
    public float heatDecayPerSecond = 0f;  
    public float displayThreshold = 0.1f; 
    [Header("Accessibility Settings")]
    public Color accessWalkable = new Color(0f, 1f, 1f, 0.5f); 
    public Color accessBlocked = new Color(0f, 0f, 0f, 0.8f); 

    private enum ViewMode { Normal, Heatmap, Accessibility }
    private ViewMode currentMode = ViewMode.Normal;

    private Dictionary<Vector3Int, float> tileHeatData = new Dictionary<Vector3Int, float>();
    private bool hasResetForNight = false; 

    void Awake()
    {
        Instance = this;
        ResetMapVisuals();
    }

    void Update()
    {
        if (GameManager.Instance != null)
        {
            
            if (GameManager.Instance.isLive && !hasResetForNight)
            {
                // clear heatmap data at start of day
                ClearData(); 
                hasResetForNight = true;
            }
            
            // allow for reset next night
            if (!GameManager.Instance.isLive)
            {
                hasResetForNight = false;
            }
            // record traffic during event 
            if (GameManager.Instance.isLive)
            {
                RecordTraffic();
            }
        }
        // draw oto the heatmap if in heatmap mode
        if (currentMode == ViewMode.Heatmap)
        {
            DrawHeatmap();
        }
    }

    public void ToggleHeatmap()
    {
        // determine if player can see hearmap or not
        if (currentMode == ViewMode.Heatmap){
            SetMode(ViewMode.Normal);
        }
        else{
            SetMode(ViewMode.Heatmap);
        }
    }

    public void ToggleAccessView()
    {
        // show the access map
        // access map shows walkable and blocked areas
        if (currentMode == ViewMode.Accessibility){
            SetMode(ViewMode.Normal);
        }
        else{
            SetMode(ViewMode.Accessibility);
        }
    }

    void SetMode(ViewMode newMode)
    {
        ResetMapVisuals(); 
        currentMode = newMode;

        if (currentMode == ViewMode.Accessibility)
        {
            DrawAccessMap(); 
        }
    }

    void RecordTraffic()
    {
        GameObject[] people = GameObject.FindGameObjectsWithTag("Attendee");
        // keep track of heat per tile based on if attendees are on it
        foreach (GameObject person in people)
        {
            Vector3Int cellPos = groundTilemap.WorldToCell(person.transform.position);
            
            if (!tileHeatData.ContainsKey(cellPos)){
                tileHeatData[cellPos] = 0f;
            }

            tileHeatData[cellPos] += heatGainPerSecond * Time.deltaTime;
            
            if (tileHeatData[cellPos] > 1.0f){
                tileHeatData[cellPos] = 1.0f;
            }
        }
    }

    void DrawHeatmap()
    {
        foreach (var kvp in tileHeatData)
        {
            Vector3Int pos = kvp.Key;
            float heat = kvp.Value;
            // determine colors based on heat vlaue and paint tiles
            if (overlayTilemap.HasTile(pos))
            {
                Color displayColor;

                if (heat <= displayThreshold)
                {
                    displayColor = new Color(0, 0, 0, 0); 
                }
                else
                {
                    displayColor = Color.Lerp(coldColor, hotColor, heat);
                }

                overlayTilemap.SetTileFlags(pos, TileFlags.None);
                overlayTilemap.SetColor(pos, displayColor);
            }
        }
    }

    void DrawAccessMap()
    {
        // color tiles if tehy are walkable or not
        foreach (var pos in overlayTilemap.cellBounds.allPositionsWithin)
        {
            if (overlayTilemap.HasTile(pos))
            {
                Vector3 worldPos = overlayTilemap.CellToWorld(pos) + new Vector3(0.5f, 0.5f, 0);
                NavMeshHit hit;
                bool isWalkable = NavMesh.SamplePosition(worldPos, out hit, 0.2f, NavMesh.AllAreas);

                overlayTilemap.SetTileFlags(pos, TileFlags.None);
                overlayTilemap.SetColor(pos, isWalkable ? accessWalkable : accessBlocked);
            }
        }
    }

    void ResetMapVisuals()
    {
        // reset tiles
        foreach (var pos in overlayTilemap.cellBounds.allPositionsWithin)
        {
            if (overlayTilemap.HasTile(pos))
            {
                overlayTilemap.SetTileFlags(pos, TileFlags.None);
                overlayTilemap.SetColor(pos, new Color(0, 0, 0, 0));
            }
        }
    }
    
    public void ClearData()
    {
        tileHeatData.Clear();
        ResetMapVisuals();
    }
}