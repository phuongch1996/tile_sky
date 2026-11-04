using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupMap : Popup
{
    public Button btClose;
    public GameObject buttonTabsGO;
    public List<MapObject> maps;
    public MapObject mapTemplate;
    public Transform mapParentTfm;
    public override void SetupTweenCallback()
    {
        SetupTweenCallback(() =>
        {
            btClose.onClick.AddListener(() => Close());
            for (int i = 0; i < maps.Count; i++)
            {
                MapButton(maps[i], i);
            }
        });
    }
    public override void Open()
    {
        HomeAlignedBackButton.Configure(btClose, HomeScript.instance ? HomeScript.instance.btSettings : null);
        //fadeIgnore = new();
        maps = new();
        //if (GameConfig.bgTypeDefault == 0)
        //{

        //}
        buttonTabsGO.SetActive(false);
        foreach (var bgSprite in HomeScript.instance.canvasBG.bgSpritesSO.bgSprites)
        {
            MapObject map = PoolManager.instance.GetObject<MapObject>(mapTemplate, mapParentTfm);
            map.img.sprite = bgSprite.bg;
            maps.Add(map);
            //fadeIgnore.Add(map.gameObject);
        }
        var selectableBg = GameConfig.GetSelectableBackground();
        int unlockCount = 0;
        for (int i = 0; i < maps.Count; i++)
        {
            maps[i].Unlock(selectableBg.Contains(i));
            maps[i].bt.onClick.RemoveAllListeners();
            if (selectableBg.Contains(i))
                maps[i].transform.SetSiblingIndex(unlockCount++);
        }
        btClose.onClick.RemoveAllListeners();
        mapParentTfm.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        SelectMap();
        base.Open();
    }
    public override void Close()
    {
        base.Close();
        foreach (var map in maps)
            PoolManager.instance.ReleaseObject(map);
        maps.Clear();
        //fadeIgnore.Clear();
    }
    void MapButton(MapObject map, int bgIndex, int bgType = 0)
    {
        map.bt.onClick.AddListener(() =>
        {
            Debug.Log("map button");
            HomeScript.instance.OnPreviewMap(bgIndex, bgType, map.unlock);
        });
    }
    public void SelectMap()
    {
        int bg = GameConfig.GetCurrentBackground();
        foreach (var map in maps)
            map.Select(this.maps.IndexOf(map) == bg);
    }
}
