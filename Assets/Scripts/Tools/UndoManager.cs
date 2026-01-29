using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class UndoManager : MonoBehaviour
{
    public static UndoManager Instance;

    [Header("Referanslar")]
    public GridManager gridManager;
    public PlacementManager placementManager;

    [Header("Undo Hakkı ve UI Ayarları")]
    public int baslangicHakki = 3; 
    public Text hakText; 
    
    public int KalanHak { get; private set; }

    [Header("Undo Particle")]
    public GameObject undoParticlePrefab;

    private Stack<GameState> history = new Stack<GameState>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (KalanHak == 0) KalanHak = baslangicHakki;
        UpdateUI();
    }

    public void LoadRights(int rights)
    {
        KalanHak = rights;
        UpdateUI();
    }

    public void ResetHistory()
    {
        history.Clear();
        KalanHak = baslangicHakki; 
        UpdateUI();
    }

    public void SaveState(GridCell targetCell = null)
    {
        GameState state = new GameState();
        
        if (targetCell != null)
        {
            state.lastActionPos = targetCell.cellPosition;
            state.hasActionPos = true;
        }

        state.siradakiVeri = placementManager.siradakiObjeVerisi; 
        state.sonrakiVeri = placementManager.sonrakiObjeVerisi;

        state.gridObjects = new List<ObjectState>();
        
        foreach (var cell in gridManager.GetAllCells())
        {
            if (!cell.IsEmpty() && cell.currentObject != null)
            {
                ObjectState objState = new ObjectState();
                objState.position = cell.cellPosition;
                objState.data = cell.currentObject.verisi;
                objState.isLocked = cell.currentObject.kilitliMi;
                objState.hareketHakki = cell.currentObject.hareketHakki;

                if (cell.currentObject.icindekiMalzemeler != null)
                {
                    objState.materials = new List<ObjeVerisi>(cell.currentObject.icindekiMalzemeler);
                }
                else
                {
                    objState.materials = new List<ObjeVerisi>();
                }

                state.gridObjects.Add(objState);
            }
        }
        history.Push(state);
    }

    public void Undo()
    {
        if (history.Count == 0 || KalanHak <= 0)
            return;

        GameState lastState = history.Pop();

        gridManager.TemizleVeYokEtPublic();

        foreach (var objState in lastState.gridObjects)
        {
            GridCell cell = gridManager.GetCell(objState.position);
            Vector3 worldPos = gridManager.grid.GetCellCenterWorld(objState.position);

            GameObject newObj = Instantiate(
                objState.data.objePrefab,
                worldPos,
                objState.data.objePrefab.transform.rotation
            );

            PlaceableObject po = newObj.GetComponent<PlaceableObject>();

            po.verisi = objState.data;
            po.currentCell = cell;
            po.kilitliMi = objState.isLocked;
            po.hareketHakki = objState.hareketHakki;
            po.icindekiMalzemeler = new List<ObjeVerisi>(objState.materials);

            po.transform.position = worldPos + Vector3.up * po.heightOffset;
            po.BoyutuGuncelle();
            po.SetPreviewMode(false);

            cell.currentObject = po;
        }

        if (undoParticlePrefab != null && lastState.hasActionPos)
        {
            Vector3 targetWorldPos = gridManager.grid.GetCellCenterWorld(lastState.lastActionPos);
            Vector3 particlePos = targetWorldPos + Vector3.up * 0.6f;
            Instantiate(undoParticlePrefab, particlePos, Quaternion.identity);
        }

        placementManager.LoadSpawnState(
            lastState.siradakiVeri,
            lastState.sonrakiVeri
        );

        KalanHak--;
        UpdateUI();
    }

    public void RemoveLastState()
    {
        if (history.Count > 0)
            history.Pop();
    }

    private void UpdateUI()
    {
        if (hakText != null)
            hakText.text = KalanHak.ToString();
    }
}


[System.Serializable]
public class GameState
{
    public Vector3Int lastActionPos;
    public bool hasActionPos = false;

    public ObjeVerisi siradakiVeri;
    public ObjeVerisi sonrakiVeri;
    public List<ObjectState> gridObjects;
}

[System.Serializable]
public class ObjectState
{
    public Vector3Int position;
    public ObjeVerisi data;
    public bool isLocked;
    public int hareketHakki;
    public List<ObjeVerisi> materials;
}