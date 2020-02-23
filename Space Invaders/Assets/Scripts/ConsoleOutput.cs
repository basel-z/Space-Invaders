using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class enables output to in-game onscreen console. Provides static access
/// via singleton. Text can be formatted and colored.
/// </summary>
public class ConsoleOutput : Singleton<ConsoleOutput> {

    private Text[] textFields;
    private int maxNumberOfMessages;
    private int numberOfMessages = 0;

    private void Awake()
    {
        textFields = GetComponentsInChildren<Text>();
        maxNumberOfMessages = textFields.Length;
    }

    /// <summary>
    /// Posts a message to the console output which is then displayed on the UI.
    /// </summary>
    /// <param name="message">Message text</param>
    /// <param name="color">Message color</param>
    public void PostMessage(string message, Color color)
    {
        if(numberOfMessages < maxNumberOfMessages)
        {
            numberOfMessages++;
            for (int i = numberOfMessages - 1; i > 0; i--)
            {
                textFields[i].text = textFields[i - 1].text;
                textFields[i].color = textFields[i - 1].color;
            }
            textFields[0].text = message;
            textFields[0].color = color;
        }
        else
        {
            // Circular buffer imitation
            for (int i = maxNumberOfMessages-1; i > 0; i--)
            {
                textFields[i].text = textFields[i-1].text;
                textFields[i].color = textFields[i - 1].color;
            }
            textFields[0].text = message;
            textFields[0].color = color;
        }

        UpdateTextFieldAlpha();
    }

    /// <summary>
    /// Posts a message to the console output which is then displayed on the UI.
    /// Message color is white by default.
    /// </summary>
    /// <param name="message">Message text</param>
    public void PostMessage(string message)
    {
        PostMessage(message, Color.white);
    }

    /// <summary>
    /// Updates the text field alpha channel so that the oldest message fades the most.
    /// </summary>
    private void UpdateTextFieldAlpha()
    {
        Color textColor; 

        for (int i = 0; i < maxNumberOfMessages; i++)
        {
            textColor = textFields[i].color;

            textColor.a = (maxNumberOfMessages - i + 1.0f) / (float) maxNumberOfMessages;

            textFields[i].color = textColor;
        }
    }

}
