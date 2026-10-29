using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopBarScript : MonoBehaviour
{
    public Canvas canvas;
    public GameObject goldsCounter;
    public TextMeshProUGUI txGolds;
    public PoolObject goldGO;
    public Button btShop;
    //public GameObject starsCounter;
    //public TextMeshProUGUI txStars;
    //public GameObject starGO;
    //public bool hideStars;
    public bool goldsUpdating;
    private void OnEnable()
    {
        GameConfig.OnGoldChange += OnGoldChange;
    }
    private void OnDisable()
    {
        GameConfig.OnGoldChange -= OnGoldChange;
    }
    void OnGoldChange(int gold, bool increase)
    {
        if (goldsUpdating)
            return;
        txGolds.text = gold.ToString();
        txGolds.DOKill();
        txGolds.color = Color.white;
        txGolds.DOColor(increase ? Color.green : Color.red, 0.25f).SetLoops(2, LoopType.Yoyo);
    }
    private void Start()
    {
        if (txGolds)
            txGolds.text = GameConfig.GetGold().ToString();
    }
    public IEnumerator UpdateGolds(Transform pos, int goldsGain, int mod, Action callback)
    {
        goldsUpdating = true;
        int module = goldsGain % mod;
        int count = 0;
        GameConfig.AddGold(goldsGain);
        yield return null;

        for (int i = 0; i <= ((module == 0) ? ((goldsGain / mod) - 1) : (goldsGain / mod)); i++)
        {
            //GameObject coin = Instantiate(coinGO, this.transform);
            var coin = PoolManager.instance.GetObject<PoolObject>(goldGO, this.transform);

            float offsetX = UnityEngine.Random.Range(-.5f, .5f);
            float offsetY = UnityEngine.Random.Range(-.5f, .5f);

            coin.transform.position = pos.position + new Vector3(offsetX, offsetY);
            coin.transform.DOMove(txGolds.transform.position, 1f).SetEase(Ease.InBack).OnComplete(() =>
            {
                int add = 0;
                if (count == goldsGain / mod)
                {
                    add = module;
                }
                else
                {
                    add = mod;
                }
                Debug.Log($"{count} {add}");
                count++;
                PoolManager.instance.ReleaseObject(coin);
                txGolds.text = (int.Parse(txGolds.text) + add).ToString();
                txGolds.DOKill();
                txGolds.color = Color.white;
                txGolds.DOColor(Color.green, 0.25f).SetLoops(2, LoopType.Yoyo);
            });
            yield return null;
        }
        yield return new WaitForSeconds(1);
        goldsUpdating = false;
        callback?.Invoke();
        txGolds.text = GameConfig.GetGold().ToString();
    }
    public void Top()
    {
        canvas.sortingOrder = 3;
        btShop.gameObject.SetActive(false);
    }
    public void Bottom()
    {
        canvas.sortingOrder = 1;
        btShop.gameObject.SetActive(true);
    }
}
