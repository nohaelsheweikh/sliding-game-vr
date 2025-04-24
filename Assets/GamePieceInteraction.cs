using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GamePieceInteraction : MonoBehaviour
{
    private GameManager gameManager;
    private Transform handTransform;
    private bool isSliding = false;
    private Vector3 initialHandPos;
    private Vector3 initialPiecePos;

    public int PieceIndex => int.Parse(gameObject.name);

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        if (isSliding && handTransform != null)
        {
            if (gameManager.CanSlide(PieceIndex, out Vector3 slideDir))
            {
                Vector3 handMovement = handTransform.localPosition - initialHandPos;
                gameManager.MovePieceTowardEmpty(PieceIndex, handMovement, initialPiecePos, slideDir);
            }
        }
    }

    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        handTransform = args.interactorObject.transform;
        isSliding = true;
        initialHandPos = handTransform.localPosition;
        initialPiecePos = transform.localPosition;
    }

    public void OnSelectExited(SelectExitEventArgs args)
    {
        if (!isSliding) return;

        isSliding = false;
        handTransform = null;
        gameManager.TrySlideToEmpty(PieceIndex, transform.localPosition);
    }
}
