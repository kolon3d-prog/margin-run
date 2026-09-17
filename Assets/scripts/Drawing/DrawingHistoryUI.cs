using UnityEngine;
using UIButton = UnityEngine.UI.Button;

public class DrawingHistoryUI : MonoBehaviour
{
    [SerializeField] private PaperDrawer paperDrawer;

    [SerializeField] private UIButton undoButton;
    [SerializeField] private UIButton redoButton;
    [SerializeField] private UIButton clearButton;

    private void Start()
    {
        undoButton.onClick.AddListener(Undo);
        redoButton.onClick.AddListener(Redo);
        clearButton.onClick.AddListener(Clear);

        RefreshButtons();
    }

    private void Update()
    {
        RefreshButtons();
    }

    private void Undo()
    {
        paperDrawer.UndoLastLine();
        RefreshButtons();
    }

    private void Redo()
    {
        paperDrawer.RedoLastLine();
        RefreshButtons();
    }
    
    private void Clear()
    {
        paperDrawer.ClearAllLines();
        RefreshButtons();
    }

    private void RefreshButtons()
    {
        if(paperDrawer == null)
            return;
        
        undoButton.interactable = paperDrawer.CanUndo;
        redoButton.interactable = paperDrawer.CanRedo;

        clearButton.interactable = paperDrawer.CanUndo || paperDrawer.CanRedo;
    }

    private void OnDestroy()
    {
        if (undoButton != null)
            undoButton.onClick.RemoveListener(Undo);
        if (redoButton != null)
            redoButton.onClick.RemoveListener(Redo);
        if (clearButton != null)
            clearButton.onClick.RemoveListener(Clear);
    }
}
