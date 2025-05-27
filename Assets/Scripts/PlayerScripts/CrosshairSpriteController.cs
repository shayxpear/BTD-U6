using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairSpriteController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer crosshairRenderer;
    [SerializeField] private Sprite[] redCrosshairSprites;
    [SerializeField] private Sprite[] blueCrosshairSprites;
    [SerializeField] private Sprite[] whiteCrosshairSprites;
    [SerializeField] private PlayerUI playerUI;
    [SerializeField] private TrackHolder trackHolder;
    [SerializeField] private GameManager gameManager;

    // Call this to start the crosshair animation
    public void StartCrosshairCoroutine(List<double> leftNoteTimes, List<double> rightNoteTimes, System.Func<double> getAudioSourceTime)
    {
        StopAllCoroutines();
        StartCoroutine(CrosshairSprite(leftNoteTimes, rightNoteTimes, getAudioSourceTime));
    }

    private IEnumerator CrosshairSprite(List<double> leftNoteTimes, List<double> rightNoteTimes, System.Func<double> getAudioSourceTime)
    {
        // Merge & sort note times
        var sortedNoteTimes = new List<double>(leftNoteTimes);
        sortedNoteTimes.AddRange(rightNoteTimes);
        sortedNoteTimes.Sort();
        sortedNoteTimes.Insert(0, 0.0);

        Sprite[] currentSpriteSheet = null;
        int currentSprite = 0;

        for (int i = 0; i < sortedNoteTimes.Count - 1; i++)
        {
            double currentNoteTime = sortedNoteTimes[i];
            double nextNoteTime = sortedNoteTimes[i + 1];

            // Wait until the audio reaches the current note time
            yield return new WaitUntil(() => getAudioSourceTime() >= currentNoteTime || !trackHolder.guitarRiff.isPlaying);

            // Wait while paused
            while (gameManager != null && gameManager.isPaused)
                yield return null;

            // If the song ends, reset to white
            if (!trackHolder.guitarRiff.isPlaying && (gameManager == null || !gameManager.isPaused))
            {
                ResetToWhiteCrosshair();
                yield break;
            }

            // Use the note time of the next note to decide the color
            if (leftNoteTimes.Contains(nextNoteTime))
            {
                currentSpriteSheet = redCrosshairSprites;
            }
            else if (rightNoteTimes.Contains(nextNoteTime))
            {
                currentSpriteSheet = blueCrosshairSprites;
            }
            else
            {
                currentSpriteSheet = playerUI.crosshairSprites;
            }

            int totalSprites = currentSpriteSheet.Length;
            currentSprite = 0;

            // Update sprite during the segment
            while (getAudioSourceTime() < nextNoteTime && trackHolder.guitarRiff.isPlaying)
            {
                // Wait while paused
                while (gameManager != null && gameManager.isPaused)
                    yield return null;

                float progress = (float)((getAudioSourceTime() - currentNoteTime) / (nextNoteTime - currentNoteTime));
                int newSprite = Mathf.Clamp(Mathf.FloorToInt(progress * totalSprites), 0, totalSprites - 1);

                if (newSprite != currentSprite)
                {
                    currentSprite = newSprite;
                    crosshairRenderer.sprite = currentSpriteSheet[currentSprite];
                }
                yield return null;
            }

            // If the song ends during the segment, reset to white
            if (!trackHolder.guitarRiff.isPlaying && (gameManager == null || !gameManager.isPaused))
            {
                ResetToWhiteCrosshair();
                yield break;
            }
        }

        // Wait until the audio completes playing before resetting to white
        yield return new WaitUntil(() => !trackHolder.guitarRiff.isPlaying);

        ResetToWhiteCrosshair();
    }

    private void ResetToWhiteCrosshair()
    {
        if (whiteCrosshairSprites != null && whiteCrosshairSprites.Length > 0)
        {
            crosshairRenderer.sprite = whiteCrosshairSprites[0];
        }
    }
}