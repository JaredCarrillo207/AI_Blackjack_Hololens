using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions; // To use regular expressions for parsing

public class RandomCardDisplay : MonoBehaviour
{
    // List of Image components (your 11 Image objects in the Canvas)
    public Image[] cardImages;

    // List of card sprites (52 playing card sprites)
    public Sprite[] cardSprites;

    // Reference to the TCP server script
    public SimpleTCPServer tcpServer;

    // Time interval for updating the cards
    private float changeInterval = 1.0f;

    // Flag to check if final message has been received
    private bool finalMessageReceived = false;

    void Start()
    {
        // Start both random card changes and the TCP listener
        StartCoroutine(UpdateCardsFromTCP());
        StartCoroutine(ChangeCardsRandomly());
    }

    // Coroutine to change cards randomly every second
    IEnumerator ChangeCardsRandomly()
    {
        while (!finalMessageReceived)
        {
            // Randomly assign card sprites to each card image
            for (int i = 0; i < cardImages.Length; i++)
            {
                int randomIndex = Random.Range(0, cardSprites.Length);
                cardImages[i].sprite = cardSprites[randomIndex];
            }

            // Wait for the change interval before changing cards again
            yield return new WaitForSeconds(changeInterval);
        }
    }

    // Coroutine to update cards based on received TCP message
    IEnumerator UpdateCardsFromTCP()
    {
        while (true)
        {
            // Get the latest message from the TCP server
            string tcpMessage = tcpServer.GetLastMessage().Trim(); // Trim to remove extra spaces/newlines

            // Log the raw message for debugging
            Debug.Log("Received TCP Message: " + tcpMessage);

            // Check if the message format is correct before proceeding
            if (tcpMessage.StartsWith("[") && tcpMessage.EndsWith("]"))
            {
                // If the message is in the correct format, stop random changes and update cards
                finalMessageReceived = true;
                UpdateCardImages(tcpMessage);
            }

            // Wait for the specified interval before checking again
            yield return new WaitForSeconds(changeInterval);
        }
    }

    // Method to update card images based on the received message
    void UpdateCardImages(string message)
    {
        // Parse the message to remove brackets and split into individual numbers
        message = message.Trim(new char[] { '[', ']' }); // Remove the brackets
        string[] stringIndices = message.Split(',');     // Split by commas

        // Log the parsed message for debugging
        Debug.Log("Parsed Indices String Array: " + string.Join(",", stringIndices));

        // Convert the string array to an integer array
        int[] indices = new int[stringIndices.Length];
        for (int i = 0; i < stringIndices.Length; i++)
        {
            if (int.TryParse(stringIndices[i].Trim(), out int spriteIndex)) // Ensure spaces are trimmed and parsed
            {
                indices[i] = spriteIndex;
            }
            else
            {
                Debug.LogError($"Failed to parse index: {stringIndices[i]}");
            }
        }

        // Log the parsed indices as integers for debugging
        Debug.Log("Parsed Indices Integer Array: " + string.Join(",", indices));

        // Ensure we have enough indices to match the number of cardImages
        if (indices.Length == cardImages.Length)
        {
            for (int i = 0; i < cardImages.Length; i++)
            {
                int spriteIndex = indices[i];

                // Ensure the index is within the range of available card sprites
                if (spriteIndex >= 0 && spriteIndex < cardSprites.Length)
                {
                    // Assign the corresponding sprite to the card image
                    cardImages[i].sprite = cardSprites[spriteIndex];
                    Debug.Log($"Assigned sprite {spriteIndex} to card image {i}");
                }
                else
                {
                    Debug.LogError($"Sprite index {spriteIndex} is out of range");
                }
            }
        }
        else
        {
            Debug.LogError($"Mismatch between number of indices ({indices.Length}) and card images ({cardImages.Length})");
        }
    }
}
