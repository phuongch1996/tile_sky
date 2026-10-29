using System;
using System.Collections.Generic;
using UnityEngine;

public class GameTextScript : MonoBehaviour
{
    public List<PoolObject> textsGOList;
    [SerializeField] string textSortingLayer = "Hovering";
    [SerializeField] int textSortingOrder = 6000;
    readonly int timeInterval = 3;
    float timer;
    int index = 0;
    public static int comboCount;
    int combo = 0;
    public static Action<int> OnComboCounter;
    public int ShowText()
    {
        if (index <= 0)
        {
            index = 1;
            timer = timeInterval;
            combo = 0;
            return 0;
        }
        combo += 1;
        comboCount += combo;
        OnComboCounter?.Invoke(comboCount);
        //int index = textsGOList.IndexOf(text);
        AudioManager.SFX sfxId = index switch
        {
            1 => AudioManager.SFX.V_GREAT,
            2 => AudioManager.SFX.V_GOOD,
            3 => AudioManager.SFX.V_EXCELLENT,
            4 => AudioManager.SFX.V_AMAZING,
            //5 => AudioManager.SFX.V_PERFECT,
            _ => AudioManager.SFX.V_AWESOME,
        };
        if (sfxId == AudioManager.SFX.V_AWESOME && index % 2 == 0)
            sfxId = AudioManager.SFX.V_PERFECT;
        PoolObject text = sfxId switch
        {
            AudioManager.SFX.V_GREAT => textsGOList[0],
            AudioManager.SFX.V_GOOD => textsGOList[1],
            AudioManager.SFX.V_EXCELLENT => textsGOList[2],
            AudioManager.SFX.V_AMAZING => textsGOList[3],
            AudioManager.SFX.V_AWESOME => textsGOList[4],
            AudioManager.SFX.V_PERFECT => textsGOList[5],
            _ => null
        };
        index++;
        timer = timeInterval;
        AudioManager.instance.PlaySFXOneShot(sfxId);
        PoolObject ob = PoolManager.instance.GetObject<PoolObject>(text, this.transform, 2);
        ob.transform.localPosition = Vector3.zero;
        BringTextToTop(ob);
        return combo;
    }
    void BringTextToTop(PoolObject textObject)
    {
        foreach (Renderer renderer in textObject.GetComponentsInChildren<Renderer>(true))
        {
            renderer.sortingLayerName = textSortingLayer;
            renderer.sortingOrder = textSortingOrder++;
        }
    }
    private void Update()
    {
        if (!GameConfig.isGameLoaded)
            return;
        if (GameController.instance.gameState != GAME_STATE.PLAYING)
            return;
        if (index != 0 && timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            index = 0;
        }
    }
}
