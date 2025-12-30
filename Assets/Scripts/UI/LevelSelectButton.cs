using UnityEngine;

public class LevelSelectButton : MonoBehaviour
{
    public LevelData levelData; // Inspector'dan atanacak

    public void LeveliBaslat()
    {
        Debug.Log("Level butonuna basıldı");

        if (levelData == null)
        {
            Debug.LogError("LevelData atanmadı");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager yok");
            return;
        }

        int index = GameManager.Instance.tumLeveller.IndexOf(levelData);

        if (index == 0)
        {
            Debug.LogError("Bu LevelData GameManager listesinde yok");
            return;
        }

        Debug.Log("Level başlatılıyor: " + levelData.levelAdi);
        GameManager.Instance.LeveliBaslat(index);
    }
}