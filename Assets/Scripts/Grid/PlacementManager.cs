using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public Grid grid;
    public GridManager gridManager;
    public GameObject placeablePrefab;

    private GameObject previewObject;
    private GridCell selectedCell;
//---------------------------------------------------------------------------
//Drag & Drop için tanımlanan değişkenler
    private bool isDragging = false;
    private Vector3Int lastHoveredCellPos = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
    private bool dragMoved = false;
    private Vector2 startScreenPos;
    private const float DRAG_THRESHOLD_PX = 10f;
//---------------------------------------------------------------------------
    void Start()
    {
        previewObject = Instantiate(placeablePrefab);//Yerleştirilecek objenin previewını spawn eder.

        foreach (var col in previewObject.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }
        //Preview'ın raycast ile çarpışmasını engellemek için colliderları devredışı bırakır.

        previewObject.GetComponent<PlaceableObject>().SetPreviewMode(true);//Preview animasyon kodu

        GridCell firstEmpty = gridManager.GetFirstEmptyCell();//Grid üzerindeki ilk boş hücreyi alır
        if (firstEmpty != null)
        {
            SelectCell(firstEmpty);
        }
        //Eğer boş hücre varsa preview oraya taşınır
        
    }

    void Update()
    {

        //-----------------------------------------------------------
        //Drag & Drop için eklenen kodlar
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            HandleTouch(t.position, t.phase);
            return;
        }
        //-----------------------------------------------------------
        //Mouse ile mobil touch taklidi. (Sadece editör ve PC üzerinde çalışacak.)
        #if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
            HandleTouch(Input.mousePosition, TouchPhase.Began);

        if (Input.GetMouseButton(0))
            HandleTouch(Input.mousePosition, TouchPhase.Moved);

        if (Input.GetMouseButtonUp(0))
            HandleTouch(Input.mousePosition, TouchPhase.Ended);
        #endif
        //-----------------------------------------------------------
    }

    void HandleTouch(Vector3 position, TouchPhase phase)
    {
        Ray ray = Camera.main.ScreenPointToRay(position);

        if (!Physics.Raycast(ray, out RaycastHit hit))
        {
            //-----------------------------------------------------------
            //Drag & Drop için eklenen kodlar (Dokunma bitse bile preview kaybolmayacak.)
            if (phase == TouchPhase.Ended || phase == TouchPhase.Canceled)
                isDragging = false;
            //-----------------------------------------------------------
            return;            
        }

        Vector3Int cellPos = grid.WorldToCell(hit.point);//Raycastin çarptığı dünya pozisyonunu grid koordinatlarına çevirir.
        GridCell cell = gridManager.GetCell(cellPos);//Grid pozisyondaki hücre alınır

        if (cell == null)
        {
            //-----------------------------------------------------------
            //Drag & Drop için eklenen kodlar
            if (phase == TouchPhase.Ended || phase == TouchPhase.Canceled)
                isDragging = false;
            //-----------------------------------------------------------
            return;
        }
        //-----------------------------------------------------------
        //Drag & Drop için eklenen kodlar
        //1)Seçim ve ikinci dokunuşta place
        if (phase == TouchPhase.Began)
        {
            isDragging = true;
            dragMoved = false;
            startScreenPos = position;
            lastHoveredCellPos = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

            //Dolu hücreye dokunursan hiçbir şey yapma
            if (!cell.IsEmpty())
            {
                return;
            }

            //İlk seçim preview aç + yerleştir
            if (selectedCell == null)
            {
                SelectCell(cell);
                return;
            }

            if (cell != selectedCell)
            {
                SelectCell(cell);
            }

            return;
        }
        //Henüz bir hücre seçili değilse tıklanan hücre seçilir ve preview oraya taşınır

        //2)Preview sürükleme ve sadece boş hücrelere yerleştirme
        if (phase == TouchPhase.Moved || phase == TouchPhase.Stationary)
        {
            //Preview aktif değilse sürükleme yok
            if (!isDragging || selectedCell == null)
            {
                return;
            }

            //Parmak/mouse basmak yerine sürüklendi mi?
            if (!dragMoved && Vector2.Distance(position, startScreenPos) >= DRAG_THRESHOLD_PX)
            {
                dragMoved = true;
            }


            //Aynı hücredeysek boş yere SelectCell çağırma
            if (cellPos == lastHoveredCellPos)
            {
                return;
            }

            lastHoveredCellPos = cellPos;

            //Yalnızca boş hücrelere yerleştir
            if (!cell.IsEmpty())
            {
                return;
            }

            //Hücre değiştiyse seçimi güncelle
            if (cell != selectedCell)
            {
                SelectCell(cell);
            }

            return;
        }

        //3)Sadece drag biter, yerleştirme yok, seçim korunur
        if (phase == TouchPhase.Ended || phase == TouchPhase.Canceled)
        {
            //Sadece basıldığında yerleştir
            if (phase == TouchPhase.Ended && !dragMoved && selectedCell != null)
            {
                //Parmağı kaldırdığında hala seçili hücredeysen yerleştir
                if (cell == selectedCell)
                    Place();
            }

            isDragging = false;
            //selectedCell ve previewObject korunur.
            return;
        }
        //-----------------------------------------------------------
    }

    void SelectCell(GridCell cell)
    {
        selectedCell = cell;//Yeni hücre seçili olarak atanır.
        previewObject.SetActive(true);

        float offset = previewObject.GetComponent<PlaceableObject>().heightOffset;//Objeye offset vermek için

        previewObject.transform.position = grid.GetCellCenterWorld(cell.cellPosition) + Vector3.up * offset;//Hücrenin merkezine y koodinatında objeye offset vererek yerleştirir.
    }

    void Place()//Preview objeye dönüştrmek için yazılan methodtur
    {
        if (selectedCell == null || !selectedCell.IsEmpty())
        {
            return;
        }
        //Seçili hücre yoksa yerleştirme yapılmaz return döner.

        PlaceableObject prefabPO = placeablePrefab.GetComponent<PlaceableObject>();//Prefab üzerineki ayarlar çekilir

        Vector3 spawnPos = grid.GetCellCenterWorld(selectedCell.cellPosition) + Vector3.up * prefabPO.heightOffset;
        //Hücrenin merkezine y koodinatında objeye offset vererek yerleştirir. Üstekinden farkı burada obje spawn edilir, üstekinde ise preview olarak gözükür.

        GameObject obj = Instantiate(placeablePrefab, spawnPos, Quaternion.identity);

        PlaceableObject po = obj.GetComponent<PlaceableObject>();
        po.SetPreviewMode(false);
        //Animasyon kapatılır ve bu obje artık preview değildir.

        po.currentCell = selectedCell;
        selectedCell.currentObject = po;
        //Hücre artık dolu kabul edilir.

        selectedCell = null;
        previewObject.SetActive(false);
        //Yerleştirdikten sonra preview sıfırlanır.
        
        GridCell nextEmpty = gridManager.GetFirstEmptyCell();//Grid üzerindeki bir sonraki boş hücre bulur.
        if (nextEmpty != null)
        {
            SelectCell(nextEmpty);
        }
        //Preview otomatik olarak yeni boş hücreye taşınır.
    }
}
