using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Standart UI kütüphanesi

public class UndoManager : MonoBehaviour
{
    public static UndoManager Instance;

    [Header("Referanslar")]
    public GridManager gridManager;
    public PlacementManager placementManager;

    [Header("Undo Hakkı ve UI Ayarları")]
    public int baslangicHakki = 3; 
    public Text hakText; 
    private int kalanHak;

    private Stack<GameState> history = new Stack<GameState>();
    //Son objenin verilerini

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        kalanHak = baslangicHakki;
        UpdateUI();
    }

    public void SaveState()
    {
        GameState state = new GameState();//Oyunun anlık fotoğrafını alır
        state.siradakiVeri = placementManager.siradakiObjeVerisi; 
        state.sonrakiVeri = placementManager.sonrakiObjeVerisi;
        //PlacementManager scripttindeki siradakiObjeVerisi ve sonrakiObjeVerisi Undo yapıldıktan sonrada aynı kalsın diye kaydedilir

        state.gridObjects = new List<ObjectState>();//Grid üzerindeki tüm objeleri alır
        
        foreach (var cell in gridManager.GetAllCells())//Griddeki tüm hücreleri dolaşır
        {
            if (!cell.IsEmpty())//Eğer hücre boş değilse devam edilir
            {
                ObjectState objState = new ObjectState();//Bir objenin kaydı
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
                
                state.gridObjects.Add(objState);//Obje GameState'e eklenir
            }
        }
        history.Push(state);//Oyundaki son durumu stacke koyar ve artık undo yapılabilir
    }

    public void Undo()
    {
        if (history.Count == 0)
        {
            return;
        }

        // Hak bittiyse işlem yapma
        if (kalanHak <= 0)
        {
            return;
        }

        GameState lastState = history.Pop();//Stack'ten en son kaydedilen durum alınır.
        gridManager.TemizleVeYokEtPublic();//Sahnedeki tüm objeler silinir.

        foreach (var objState in lastState.gridObjects)//Kaydedilen tüm objeler tekrar geri yüklenir.
        {
            GridCell cell = gridManager.GetCell(objState.position);
            Vector3 worldPos = gridManager.grid.GetCellCenterWorld(objState.position);

            GameObject newObj = Instantiate(objState.data.objePrefab, worldPos, Quaternion.identity);
            PlaceableObject po = newObj.GetComponent<PlaceableObject>();

            po.verisi = objState.data;
            po.currentCell = cell;
            po.kilitliMi = objState.isLocked;
            po.hareketHakki = objState.hareketHakki;
            po.icindekiMalzemeler = new List<ObjeVerisi>(objState.materials);
            //Objelere tüm eski değerleri geri verilir
            
            po.transform.position = worldPos + Vector3.up * po.heightOffset;
            po.BoyutuGuncelle();
            po.SetPreviewMode(false);

            cell.currentObject = po;//Hücreye bu obje atanır
        }

        placementManager.siradakiObjeVerisi = lastState.siradakiVeri;
        placementManager.sonrakiObjeVerisi = lastState.sonrakiVeri;
        placementManager.ForceUpdatePreview(); 
        placementManager.RefreshNextObjectUI();
        //UI'daki sıradaki objeler güncellenir.

        //Hakkı azalt ve ekrana sadece sayıyı yaz
        kalanHak--;
        UpdateUI();
    }
    
    public void RemoveLastState()
    {
        if (history.Count > 0)
        {
            history.Pop();
        }
    }
    //Son kaydı siler merge gibi işlemlerden sonra kullanılır

        private void UpdateUI()
    {
        if (hakText != null)
        {
            hakText.text = kalanHak.ToString();
        }
    }
}

[System.Serializable]
public class GameState
{
    public ObjeVerisi siradakiVeri;
    public ObjeVerisi sonrakiVeri;
    public List<ObjectState> gridObjects;//Haritadaki tüm objelerin listesi
}

[System.Serializable]
public class ObjectState
{
    public Vector3Int position;//Objenin pozisyonu
    public ObjeVerisi data;//Objenin türü
    public bool isLocked;//Objenin durumu
    public int hareketHakki;//Objenin hareket hakkı
    public List<ObjeVerisi> materials;//Objenin stack durumu
}