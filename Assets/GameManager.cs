using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform gameBoard;
    [SerializeField] private Transform piecePrefab;
    [SerializeField] private int sizeGame = 3;

    private List<Transform> pieces = new List<Transform>();
    private int emptyIndex;

    void Start()
    {
        CreateGamePieces(0.01f);
    }

    private void CreateGamePieces(float gap)
    {
        float width = 1f / sizeGame;
        var interactionManager = FindObjectOfType<XRInteractionManager>();

        for (int row = 0; row < sizeGame; row++)
        {
            for (int col = 0; col < sizeGame; col++)
            {
                Transform piece = Instantiate(piecePrefab, gameBoard);
                pieces.Add(piece);

                int index = (row * sizeGame) + col;
                piece.name = index.ToString();
                piece.localPosition = new Vector3(-1 + (2 * width * col) + width, 1 - (2 * width * row) - width, 0);
                piece.localScale = ((2 * width) - gap) * Vector3.one;

                // Clone mesh to apply unique UVs
                MeshFilter mf = piece.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    Mesh mesh = Instantiate(mf.sharedMesh);
                    mf.mesh = mesh;

                    Vector2[] uv = new Vector2[4];
                    float halfGap = gap / 2;

                    uv[0] = new Vector2((width * col) + halfGap, 1 - ((width * (row + 1)) - halfGap));
                    uv[1] = new Vector2((width * (col + 1)) - halfGap, 1 - ((width * (row + 1)) - halfGap));
                    uv[2] = new Vector2((width * col) + halfGap, 1 - ((width * row) + halfGap));
                    uv[3] = new Vector2((width * (col + 1)) - halfGap, 1 - ((width * row) + halfGap));
                    mesh.uv = uv;
                }

                Rigidbody rb = piece.GetComponent<Rigidbody>() ?? piece.gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;

                BoxCollider box = piece.GetComponent<BoxCollider>() ?? piece.gameObject.AddComponent<BoxCollider>();
                box.isTrigger = false;

                var interactable = piece.GetComponent<XRSimpleInteractable>() ?? piece.gameObject.AddComponent<XRSimpleInteractable>();
                interactable.interactionManager = interactionManager;
                interactable.selectMode = InteractableSelectMode.Multiple;

                var script = piece.GetComponent<GamePieceInteraction>() ?? piece.gameObject.AddComponent<GamePieceInteraction>();
                interactable.selectEntered.AddListener(script.OnSelectEntered);
                interactable.selectExited.AddListener(script.OnSelectExited);

                if (row == sizeGame - 1 && col == sizeGame - 1)
                {
                    emptyIndex = index;
                    piece.gameObject.SetActive(false); // hide empty
                }
            }
        }
    }

    public bool CanSlide(int pieceIndex, out Vector3 slideDir)
{
    slideDir = Vector3.zero;

    int row = pieceIndex / sizeGame;
    int col = pieceIndex % sizeGame;
    int emptyRow = emptyIndex / sizeGame;
    int emptyCol = emptyIndex % sizeGame;

    if (row == emptyRow && col == emptyCol - 1) { slideDir = Vector3.right; return true; }
    if (row == emptyRow && col == emptyCol + 1) { slideDir = Vector3.left; return true; }
    if (col == emptyCol && row == emptyRow - 1) { slideDir = Vector3.down; return true; } 
    if (col == emptyCol && row == emptyRow + 1) { slideDir = Vector3.up; return true; }   

    return false;
}


   public void MovePieceTowardEmpty(int pieceIndex, Vector3 handMovement, Vector3 startPos, Vector3 slideDir)
{
    Vector3 targetPos = pieces[emptyIndex].localPosition;

    // Ensure direction alignment
    float alignment = Vector3.Dot(slideDir.normalized, handMovement.normalized);
    if (alignment > 0.8f)
    {
        // Project hand movement onto slideDir
        float handAmount = Vector3.Dot(handMovement, slideDir.normalized);
        float maxDistance = Vector3.Distance(startPos, targetPos);

        // Clamp movement strictly between 0 and full slide length
        float moveAmount = Mathf.Clamp(handAmount, 0, maxDistance);

        // Calculate precise new position along one axis
        Vector3 newPos = startPos + slideDir.normalized * moveAmount;

        pieces[pieceIndex].localPosition = new Vector3(
            Mathf.Round(newPos.x * 1000f) / 1000f,
            Mathf.Round(newPos.y * 1000f) / 1000f,
            Mathf.Round(newPos.z * 1000f) / 1000f
        );
    }
}



    public void TrySlideToEmpty(int pieceIndex, Vector3 releasePos)
{
    Vector3 target = pieces[emptyIndex].localPosition;

    float distance = Vector3.Distance(releasePos, target);

    if (distance < 0.25f) // ← loosen this a bit for real hand movement
    {
        // Perfectly snap and swap
        pieces[pieceIndex].localPosition = target;
        pieces[emptyIndex].localPosition = GetOriginalPosition(pieceIndex);

        // Swap in the list
        (pieces[pieceIndex], pieces[emptyIndex]) = (pieces[emptyIndex], pieces[pieceIndex]);

        pieces[pieceIndex].gameObject.SetActive(false);
        pieces[emptyIndex].gameObject.SetActive(true);

        emptyIndex = pieceIndex;
    }
    else
    {
        // Not close enough? Reset position
        pieces[pieceIndex].localPosition = GetOriginalPosition(pieceIndex);
    }
}

    private Vector3 GetOriginalPosition(int index)
    {
        int row = index / sizeGame;
        int col = index % sizeGame;
        float width = 1f / sizeGame;
        return new Vector3(-1 + (2 * width * col) + width, 1 - (2 * width * row) - width, 0);
    }
}
