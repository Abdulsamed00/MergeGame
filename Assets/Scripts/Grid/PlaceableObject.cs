using UnityEngine;

public class PlaceableObject : MonoBehaviour
{
    public GridCell currentCell;
    public float heightOffset = 0.5f;
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetPreviewMode(bool isPreview)
    {
        if (animator != null)
        {
            animator.enabled = isPreview;
        }
    }
}