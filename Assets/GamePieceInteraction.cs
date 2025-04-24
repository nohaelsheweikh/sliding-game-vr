
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GamePieceInteraction : MonoBehaviour
{
    private GameManager gameManager;
    private Transform handTransform;
    private int pieceIndex;
    private bool isSliding = false;
    private Vector3 initialHandPos;
    private Vector3 initialPiecePos;
    private Vector3 slideDir;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        pieceIndex = int.Parse(gameObject.name);
    }

    void Update()
    {
        if (isSliding && handTransform != null)
        {
            Vector3 handMovement = handTransform.localPosition - initialHandPos;
            gameManager.MovePieceTowardEmpty(pieceIndex, handMovement, initialPiecePos, slideDir);
        }
    }

    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (!gameManager.CanSlide(pieceIndex, out slideDir)) return;

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
        gameManager.TrySlideToEmpty(pieceIndex, transform.localPosition);
    }
}
