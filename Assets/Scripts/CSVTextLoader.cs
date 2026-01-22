using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CSVTextLoader : MonoBehaviour
{
    [Header("CSV Settings")]
    [Tooltip("Name of the CSV file (place in Resources folder)")]
    public string csvFileName = "TextData";

    [Header("Text Settings")]
    [Tooltip("The ID to look up in the CSV")]
    public string textID = "RedBoxText";

    private TMP_Text tmpText;

    private Dictionary<string, string> textData = new Dictionary<string, string>();

    void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
        LoadCSV();
        SetTextByID(textID);
    }

    void LoadCSV()
    {
        // Load the CSV file from Resources folder
        TextAsset csvFile = Resources.Load<TextAsset>($"CSV/{csvFileName}");

        if (csvFile == null)
        {
            Debug.LogError($"CSV file '{csvFileName}' not found in Resources folder!");
            return;
        }

        // Split the file into lines
        string[] lines = csvFile.text.Split('\n');

        // Skip header row and parse data
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            // Skip empty lines
            if (string.IsNullOrEmpty(line))
                continue;

            // Parse CSV line properly handling commas within quoted text
            var columns = ParseCSVLine(line);

            if (columns.Count >= 2)
            {
                string id = columns[0].Trim();
                string text = columns[1].Trim();

                // Add to dictionary
                if (!textData.ContainsKey(id))
                {
                    textData.Add(id, text);
                }
                else
                {
                    Debug.LogWarning($"Duplicate ID '{id}' found in CSV. Skipping.");
                }
            }
        }

        Debug.Log($"Loaded {textData.Count} entries from CSV.");
    }

    private List<string> ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string currentField = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                // Toggle quote state
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                // End of field
                result.Add(currentField);
                currentField = "";
            }
            else
            {
                // Add character to current field
                currentField += c;
            }
        }

        // Add the last field
        result.Add(currentField);

        return result;
    }

    public void SetTextByID(string id)
    {
        if (tmpText == null)
        {
            Debug.LogError("TMP_Text component is not assigned!");
            return;
        }

        if (textData.ContainsKey(id))
        {
            tmpText.text = textData[id];
            Debug.Log($"Set text for ID '{id}': {textData[id]}");
        }
        else
        {
            Debug.LogWarning($"ID '{id}' not found in CSV data!");
            tmpText.text = $"[Text ID '{id}' not found]";
        }
    }

    // Optional: Method to get text by ID without setting it to TMP
    public string GetTextByID(string id)
    {
        if (textData.ContainsKey(id))
        {
            return textData[id];
        }
        return null;
    }
}