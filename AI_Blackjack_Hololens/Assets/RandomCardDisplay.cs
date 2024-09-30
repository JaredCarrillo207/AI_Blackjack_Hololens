using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomCardDisplay : MonoBehaviour
{
    // List of Image components (your 4 Image objects in the Canvas)
    public Image[] cardImages;

    // List of card sprites (52 playing card sprites)
    public Sprite[] cardSprites;

    // Change the displayed card every second
    private float changeInterval = 1.0f;

    void Start()
    {
        // Start the card selection coroutine
        StartCoroutine(ChangeCardImages());
    }

    IEnumerator ChangeCardImages()
    {
        while (true)
        {
            // For each image component, randomly assign a card sprite
            foreach (Image cardImage in cardImages)
            {
                int randomIndex = Random.Range(0, cardSprites.Length);
                cardImage.sprite = cardSprites[randomIndex];
            }

            // Wait for the specified interval before changing cards again
            yield return new WaitForSeconds(changeInterval);
        }
    }
}
