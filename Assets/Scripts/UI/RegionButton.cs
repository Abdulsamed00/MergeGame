using UnityEngine;

public enum RegionType
{
    Turkey,
    Brazil,
    Japan,
    Egypt
}

public class RegionButton : MonoBehaviour
{
    public RegionType region;

    public void OnClick()
    {
        switch (region)
        {
            case RegionType.Turkey:
                AudioManager.Instance.PlayTurkeyMusic();
                break;
            case RegionType.Brazil:
                AudioManager.Instance.PlayBrazilMusic();
                break;
            case RegionType.Japan:
                AudioManager.Instance.PlayJapanMusic();
                break;
            case RegionType.Egypt:
                AudioManager.Instance.PlayEgyptMusic();
                break;
        }
    }
}