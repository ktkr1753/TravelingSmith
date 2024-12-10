using Godot;
using System;
using System.Collections.Generic;

public partial class UIManager : CanvasLayer
{
    [Export] Control background;
    [Export] Control layer1;
    [Export] Control layer2;
    [Export] Control layer3;
    [Export] Godot.Collections.Dictionary<UIIndex, UIConfigResource> UIs;
    [Export] private MainGameItemElementPool itemFxtPool;
    Godot.Collections.Dictionary<UILayer, Control> layers = new Godot.Collections.Dictionary<UILayer, Control>();

    List<UIBase> openUIs = new List<UIBase>();

    private bool _isFoucsUIOpen = false;
    public bool isFoucsUIOpen { get { return _isFoucsUIOpen; } private set { _isFoucsUIOpen = value; } }


    public void Init()
    {
        InitLayerDic();
    }

    private void InitLayerDic()
    {
        layers.Add(UILayer.Layer1, layer1);
        layers.Add(UILayer.Layer2, layer2);
        layers.Add(UILayer.Layer3, layer3);
    }


    public override void _Process(double delta)
    {
        //UI
        if (!isFoucsUIOpen)
        {

        }
    }

    public T OpenUI<T>(UIIndex uiType, List<object> parameter = null) where T: UIBase
    {
        return OpenUI(uiType, parameter) as T;
    }

    public UIBase OpenUI(UIIndex uiType, List<object> parameter = null)
    {
        UIBase ui = null;

        bool isFind = false;
        for (int i = 0; i < openUIs.Count; i++)
        {
            if (openUIs[i].uIType == uiType && openUIs[i].isSingle)
            {
                ui = openUIs[i];
                isFind = true;
                Debug.Print("uiType:", uiType, "已開啟，不再重複生成");
                break;
            }
        }

        if (!isFind)
        {
            if (UIs.TryGetValue(uiType, out UIConfigResource config_resource))
            {
                ui = UtilityTool.CreateInstance<UIBase>(config_resource.prefab, layers[config_resource.layer]);
                openUIs.Add(ui);
                CheckUIState();
                if (parameter != null)
                {
                    ui.SetParameter(parameter);
                }
                ui.Init();
            }
            else
            {
                Debug.PrintErr("沒找到對應的UI, uitype:", uiType);
            }
        }

        return ui;
    }

    public void CloseUI()
    {
        if (openUIs.Count > 0)
        {
            UIBase ui = openUIs[openUIs.Count - 1];
            CloseUI(ui, false);
        }
    }

    public void CloseUI(UIIndex uiType)
    {
        for (int i = 0; i < openUIs.Count; i++)
        {
            if (openUIs[i].uIType == uiType)
            {
                UIBase ui = openUIs[i];
                CloseUI(ui, false);
            }
        }
    }

    public void CloseUI(UIBase ui, bool check = true)
    {
        Debug.Print("CloseUI:", ui.Name);
        if (!check || openUIs.Contains(ui))
        {
            ui.QueueFree();
            openUIs.Remove(ui);
            CheckUIState();
        }
        else
        {
            Debug.PrintErr("openUI中沒有該UI:", ui);
        }
    }

    private void CheckUIState()
    {
        bool isOpen = false;
        bool isFoucs = false;
        for (int i = 0; i < openUIs.Count; i++)
        {
            if (openUIs[i].isBackGround)
            {
                isOpen = true;
            }

            if (openUIs[i].isFoucs)
            {
                isFoucs = true;
            }
        }

        background.Visible = isOpen;
        isFoucsUIOpen = isFoucs;
    }

    public T GetOpenedUI<T>(UIIndex uiType) where T : UIBase 
    {
        return GetOpenedUI(uiType) as T;
    }

    public UIBase GetOpenedUI(UIIndex uiType)
    {
        UIBase result = null;
        for (int i = 0; i < openUIs.Count; i++)
        {
            if (openUIs[i].uIType == uiType)
            {
                result = openUIs[i];
                break;
            }
        }

        return result;
    }

    public Tween StartFlyItem(ItemBaseResource item, Vector2 startPoint, Vector2 endPoint, double duration, Action endCallback = null)
    {
        Tween tween = GetTree().CreateTween();

        MainGameItemElement itemElement = itemFxtPool.GetElement();
        itemElement.showProcess = false;
        itemElement.SetData(item);
        itemElement.GlobalPosition = startPoint;

        tween.TweenProperty(itemElement, "global_position", endPoint, duration);
        tween.TweenCallback(Callable.From(() =>
        {
            itemFxtPool.ReturnElement(itemElement);
            endCallback?.Invoke();
        }));

        return tween;
    }
}
