using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelInfoScreen : MonoBehaviour
{
    public float timeForIntro = 5.0f;
    public Text levelNameText;
    public Text waveCountText;

    public bool isComplete { get; private set; }
    
    private void OnEnable()
    {
        isComplete = false;
        StartCoroutine(DoShow());
    }

    private void OnDisable()
    {
        isComplete = false;
        StopAllCoroutines();
    }

    private IEnumerator DoShow()
    {
        levelNameText.color = new Color(1, 1, 1, 0);
        waveCountText.color = new Color(1, 1, 1, 1);

        string levelName = GameManager.Instance.levelName;
        string waveCount = $"{GameManager.Instance.waves.Length.ToString()} Waves";

        levelNameText.text = levelName;
        waveCountText.text = "";

        float totalT = 0.0f;
        float charTime = 0.0f;
        float perc = 0.0f;
        float timePerChar = 1.0f / (float) waveCount.Length;
        int characterIdx = 0;
        
        while (totalT < timeForIntro / 2.0f)
        {
            perc = totalT / (timeForIntro / 2.0f);

            levelNameText.color = new Color(1, 1, 1, perc);
            
            if (waveCountText.text != waveCount)
            {
                if (charTime >= timePerChar)
                {
                    waveCountText.text += waveCount[characterIdx];
                    characterIdx = Mathf.Clamp(characterIdx + 1, 0, waveCount.Length - 1);
                    charTime = 0.0f;
                }
                
                charTime += Time.deltaTime;
            }
            
            totalT += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(3.0f);

        totalT = 0.0f;
        while (totalT < timeForIntro / 2.0f)
        {
            perc = totalT / (timeForIntro / 2.0f);
            totalT += Time.deltaTime;
            levelNameText.color = new Color(1, 1, 1, 1.0f - perc);
            waveCountText.color = new Color(1, 1, 1, 1.0f - perc);
            yield return null;
        }

        isComplete = true;
        yield return null;
    }
}
