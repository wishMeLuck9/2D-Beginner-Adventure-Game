using UnityEngine;

public sealed class DialogueActor : MonoBehaviour, IInteractable
{
    [TextArea]
    [SerializeField] private string message = "The repair bots are loose. Fix them with your projectiles.";

    public void Interact(PlayerController2D player)
    {
        if (HudDocumentController.Instance != null)
        {
            HudDocumentController.Instance.ShowDialogue(message);
        }

        ToneAudio.PlayTone(transform.position, 420f, 0.12f, 0.13f);
    }
}
