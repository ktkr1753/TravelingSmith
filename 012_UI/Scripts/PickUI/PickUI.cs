using Godot;
using System;
using System.Collections.Generic;

public partial class PickUI : UIBase
{
    //[Export] private ItemElementPool itemElementPool;
    [Export] private PickFeatureElementPool featureElementPool;
    //[Export] private ItemInfoPanelPool infoPanelPool;
    [Export] private Button confirmButton;
    private List<ItemBaseResource> items = new List<ItemBaseResource>();
    private List<FeatureResource> canChoosefeatures = new List<FeatureResource>();
    private Action pauseFinishCallback;

    /*
    private int _nowSelectedItemIndex = -1;
    public int nowSelectedItemIndex 
    {
        get { return _nowSelectedItemIndex; }
        set 
        {
            if (_nowSelectedItemIndex != value) 
            {
                int preState = _nowSelectedItemIndex;
                _nowSelectedItemIndex = value;
                OnSelectedIndexChange(preState, _nowSelectedItemIndex);
            }
        }
    }

    private void OnSelectedIndexChange(int preState, int nextState) 
    {
        if(preState >= 0 && preState < itemElementPool.inuses.Count) 
        {
            itemElementPool.inuses[preState].nowSelectedState = ItemElement.SelectedState.None;
        }

        if(nextState >= 0 && nextState < itemElementPool.inuses.Count) 
        {
            itemElementPool.inuses[nextState].nowSelectedState = ItemElement.SelectedState.Selected;
            Vector2 globalPos = new Vector2();
            globalPos = GetInfoPanelPos(0);
            SetInfoPanel(0, itemElementPool.inuses[nextState].item, globalPos);
        }
        else 
        {
            SetInfoPanel(0, null);
        }
        SetConfirmButton();
    }
    */
    /*
    private int _nowPickElementIndex = -1;
    public int nowPickElementIndex
    {
        get { return _nowPickElementIndex; }
        set
        {
            if (_nowPickElementIndex != value)
            {
                int preState = _nowPickElementIndex;
                _nowPickElementIndex = value;

                if (preState >= 0 && preState < itemElementPool.inuses.Count)
                {
                    itemElementPool.inuses[preState].isPicking = false;
                }

                if (_nowPickElementIndex >= 0 && _nowPickElementIndex < itemElementPool.inuses.Count)
                {
                    SetPickedItemElement(itemElementPool.inuses[_nowPickElementIndex].item);
                    itemElementPool.inuses[_nowPickElementIndex].isPicking = true;
                }
                else
                {
                    SetPickedItemElement(null);
                }
            }
        }
    }
    */
    private int _nowSelectedFeatureElementIndex = -1;
    public int nowSelectedFeatureElementIndex
    {
        get { return _nowSelectedFeatureElementIndex; }
        set 
        {
            if(_nowSelectedFeatureElementIndex != value) 
            {
                int preState = _nowSelectedFeatureElementIndex;
                _nowSelectedFeatureElementIndex = value;
                OnSelectedFeatureIndexChange(preState, _nowSelectedFeatureElementIndex);
            }
        }
    }

    private void OnSelectedFeatureIndexChange(int preState, int nextState) 
    {
        if (preState >= 0 && preState < featureElementPool.inuses.Count)
        {
            featureElementPool.inuses[preState].SetSelectedHint(false);
        }

        if (nextState >= 0 && nextState < featureElementPool.inuses.Count)
        {
            featureElementPool.inuses[nextState].SetSelectedHint(true);
        }
        SetConfirmButton();
    }

    public const int chooseNum = 3;

    public override void Init()
    {
        base.Init();

        pauseFinishCallback = GameManager.instance.AddNeedPause();
        ResetCanChoosefeatures();
        SetView();
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        pauseFinishCallback?.Invoke();
        pauseFinishCallback = null;
    }

    private void ResetCanChoosefeatures() 
    {
        canChoosefeatures.Clear();

        List<FeatureResource> tempChoosefeatures = new List<FeatureResource>();
        foreach (var KV in GameManager.instance.featureConfig.config) 
        {
            if (!GameManager.instance.itemManager.haveFeatures.Contains(KV.Key)) 
            {
                tempChoosefeatures.Add(KV.Value);
            }
        }

        if(tempChoosefeatures.Count > chooseNum) 
        {
            List<int> randomNum = GameManager.instance.randomManager.GetNotRepeatList(RandomType.PickFeature, 0, tempChoosefeatures.Count, chooseNum);
            for(int i = 0; i < randomNum.Count; i++) 
            {
                canChoosefeatures.Add(tempChoosefeatures[randomNum[i]]);
            }
        }
        else if(tempChoosefeatures.Count > 0) 
        {
            List<int> randomNum = GameManager.instance.randomManager.GetNotRepeatList(RandomType.PickFeature, 0, tempChoosefeatures.Count);
            for (int i = 0; i < randomNum.Count; i++)
            {
                canChoosefeatures.Add(tempChoosefeatures[randomNum[i]]);
            }
        }
        else 
        {
            Debug.PrintErr($"沒有可以選擇的能力");
        }

    }


    private void SetView() 
    {
        SetFeatureElements();
        SetConfirmButton();
    }

    private void SetFeatureElements() 
    {
        ClearFeatureElements();
        for(int i = 0; i < canChoosefeatures.Count; i++) 
        {
            PickFeatureElement element = featureElementPool.GetElement();
            element.SetData(i, canChoosefeatures[i]);
            element.onMainClick += OnFeatureClick;
        }
    }

    private void ClearFeatureElements() 
    {
        for(int i = 0; i < featureElementPool.inuses.Count; i++) 
        {
            PickFeatureElement element = featureElementPool.inuses[i];
            element.SetData(-1, null);
            element.SetSelectedHint(false);
            element.onMainClick -= OnFeatureClick;
        }
    }

    private void SetConfirmButton() 
    {
        if(nowSelectedFeatureElementIndex != -1) 
        {
            confirmButton.Disabled = false;
        }
        else 
        {
            confirmButton.Disabled = true;
        }
    }

    private void SetPickedItemElement(ItemBaseResource item)
    {
        if (item != null)
        {
            PickUpUI pickUpUI = GameManager.instance.uiManager.GetOpenedUI<PickUpUI>(UIIndex.PickUpUI);
            if (pickUpUI != null)
            {
                pickUpUI.SetData(item);
            }
            else
            {

                pickUpUI = GameManager.instance.uiManager.OpenUI<PickUpUI>(UIIndex.PickUpUI, new List<object>() { item });
            }
        }
        else
        {
            GameManager.instance.uiManager.CloseUI(UIIndex.PickUpUI);
        }
    }

    private void OnFeatureClick(int index) 
    {
        nowSelectedFeatureElementIndex = index;
        GameManager.instance.soundManager.PlaySound(SoundEnum.sound_button2);
    }

    public void OnConfirmClick() 
    {
        FeatureIndex featureIndex = FeatureIndex.None;
        if(nowSelectedFeatureElementIndex >= 0 && nowSelectedFeatureElementIndex < canChoosefeatures.Count) 
        {
            FeatureResource featureData = canChoosefeatures[nowSelectedFeatureElementIndex];
            featureIndex = featureData.index;
        }

        if(featureIndex != FeatureIndex.None) 
        {
            GameManager.instance.itemManager.AddFeature(featureIndex);
        }
        else 
        {
            Debug.PrintErr("雖然按下確認鍵，但是沒有能力ID");
        }

        GameManager.instance.uiManager.CloseUI(this);
    }

    public void OnCancelClick() 
    {
        GameManager.instance.uiManager.CloseUI(this);
    }

}
