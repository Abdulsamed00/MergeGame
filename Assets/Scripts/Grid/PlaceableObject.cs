using UnityEngine;

public class PlaceableObject : MonoBehaviour
{
    public GridCell currentCell;
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetPreviewMode(bool isPreview)
    {
        if (animator == null) return;

        animator.enabled = isPreview;
    }
}