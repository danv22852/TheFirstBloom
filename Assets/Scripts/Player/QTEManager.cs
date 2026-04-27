using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class QTEManager : MonoBehaviour
{
    [Header("Dependencies")]
    public CombatSystem combatSystem; 
    public GameObject qteUIPanel; 
    public Image[] arrowUIImages; 

    [Header("Arrow Sprites (0=Up, 1=Down, 2=Left, 3=Right)")]
    public Sprite[] arrowSprites; 

    [Header("QTE Settings")]
    public int sequenceLength = 6;
    public float timeLimit = 3.0f;
    
    // --- NEW: An Enum to represent the direction, independent of the key pressed! ---
    public enum QTEDir { Up, Down, Left, Right }
    
    // Internal trackers
    private List<QTEDir> currentSequence = new List<QTEDir>();
    private int currentStepIndex = 0;
    private float timer = 0f;
    private bool isQTEActive = false;

    public void StartQTE(float timeGiven)
    {
        timeLimit = timeGiven;
        currentStepIndex = 0;
        timer = timeLimit;
        currentSequence.Clear();
        
        GenerateRandomSequence();
        UpdateUI();
        
        qteUIPanel.SetActive(true);
        isQTEActive = true;
    }

    private void GenerateRandomSequence()
    {
        QTEDir[] possibleDirs = { QTEDir.Up, QTEDir.Down, QTEDir.Left, QTEDir.Right };
        
        for (int i = 0; i < sequenceLength; i++)
        {
            QTEDir randomDir = possibleDirs[Random.Range(0, possibleDirs.Length)];
            currentSequence.Add(randomDir);
        }
    }

    private void UpdateUI()
    {
        for (int i = 0; i < arrowUIImages.Length; i++)
        {
            if (i < sequenceLength)
            {
                arrowUIImages[i].gameObject.SetActive(true);
                
                // --- NEW: We cast the Enum to an integer to get the correct picture! ---
                // (Up = 0, Down = 1, Left = 2, Right = 3)
                QTEDir assignedDir = currentSequence[i];
                arrowUIImages[i].sprite = arrowSprites[(int)assignedDir];

                // Reset colors: White if waiting, Green if already pressed
                if (i < currentStepIndex)
                    arrowUIImages[i].color = Color.green; // Success color!
                else
                    arrowUIImages[i].color = Color.white; // Default color
            }
            else
            {
                arrowUIImages[i].gameObject.SetActive(false);
            }
        }
    }

    // --- NEW: A helper function that checks both WASD and Arrows! ---
    private QTEDir? GetPressedDirection()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) return QTEDir.Up;
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) return QTEDir.Down;
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) return QTEDir.Left;
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) return QTEDir.Right;
        
        return null; // Return null if they pressed some other random key (like Spacebar)
    }

    private void Update()
    {
        if (!isQTEActive) return;

        // 1. Handle the Timer
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            StartCoroutine(EndQTE(false)); // Time ran out!
            return;
        }

        // 2. Listen for Keyboard Input
        if (Input.anyKeyDown)
        {
            // Find out which movement key they pressed (if any)
            QTEDir? pressedDir = GetPressedDirection();

            if (pressedDir != null) // They pressed a valid movement key!
            {
                QTEDir expectedDir = currentSequence[currentStepIndex];

                if (pressedDir == expectedDir)
                {
                    // Correct direction!
                    currentStepIndex++;
                    UpdateUI();

                    // Did they finish the whole sequence?
                    if (currentStepIndex >= sequenceLength)
                    {
                        StartCoroutine(EndQTE(true)); // They survived!
                    }
                }
                else 
                {
                    // Wrong direction pressed! Instant failure to maximize panic!
                    StartCoroutine(EndQTE(false));
                }
            }
        }
    }

    private IEnumerator EndQTE(bool success)
    {
        isQTEActive = false;

        // Visual feedback for failure (turn the remaining arrows red)
        if (!success)
        {
            for (int i = currentStepIndex; i < sequenceLength; i++)
            {
                arrowUIImages[i].color = Color.red; 
            }
        }

        // Leave the result on screen for a split second so the player registers what happened
        yield return new WaitForSeconds(0.5f);

        qteUIPanel.SetActive(false);
        combatSystem.OnQTEFinished(success); 
    }
}