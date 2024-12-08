using Godot;
using System;
using System.Collections.Generic;

public partial class ElementPoolBase<T2> : Node where T2 : Node
{
    [Export] public PackedScene prefab;
    [Export] public Node parent;
    [Export] public Node backup;

    public List<T2> inuses = new List<T2>();
    public Queue<T2> unuses = new Queue<T2>();

    public T2 GetElement()
    {
        T2 result = default;
        if (unuses.Count > 0)
        {
            result = unuses.Dequeue();
            result.SetParent(parent);
        }
        else
        {
            result = UtilityTool.CreateInstance<T2>(prefab, parent);
        }

        inuses.Add(result);

        return result;
    }

    public void ReturnAllElement()
    {
        List<T2> tempList = new List<T2>(inuses);

        for (int i = 0; i < tempList.Count; i++)
        {
            ReturnElement(tempList[i]);
        }
    }

    public void ReturnElement(T2 element)
    {
        element.SetParent(backup);
        inuses.Remove(element);
        unuses.Enqueue(element);
    }
}
