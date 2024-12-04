using Godot;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class CommonExtensions
{
    public static void RemoveAllChild(this Node node)
    {
        int childCount = node.GetChildCount();
        if (childCount > 0)
        {
            for (int i = childCount - 1; i >= 0; i--)
            {
                node.RemoveChild(node.GetChild(i));
            }
        }
    }

    public static string ToStringExtended<T>(this IEnumerable<T> sources)
    {
        List<string> sourceStrings = new List<string>();
        foreach(var source in sources) 
        {
            sourceStrings.Add(source.ToString());
        }

        return string.Join(',', sourceStrings.ToArray());
    }

    //判斷物件是否存在並可使用
    public static bool IsExist(this Node node) 
    {
        return node != null && GodotObject.IsInstanceValid(node);
    }

    public static void SetParent(this Node node, Node parentNode) 
    {
        Node preParentNode = node.GetParentOrNull<Node>();
        if (preParentNode != null) 
        {
            preParentNode.RemoveChild(node);
        }
        parentNode.AddChild(node);
    }

    public static Godot.Collections.Dictionary<Tkey, TValue> Clone<[MustBeVariant] Tkey, [MustBeVariant] TValue>(this Godot.Collections.Dictionary<Tkey, TValue> dic)
    {
        Godot.Collections.Dictionary<Tkey, TValue> result = new Godot.Collections.Dictionary<Tkey, TValue>();
        foreach(var KV in dic) 
        {
            if(KV.Value is IClone<TValue> cloneObj) 
            {
                result.Add(KV.Key, cloneObj.Clone());
            }
            else 
            {
                result.Add(KV.Key, KV.Value);
            }
        }
        return result;
    }

    public static Godot.Collections.Array<T> Clone<[MustBeVariant] T>(this Godot.Collections.Array<T> array)
    {
        Godot.Collections.Array<T> result = new Godot.Collections.Array<T>();
        foreach (var element in array)
        {
            if (element is IClone<T> cloneObj)
            {
                result.Add(cloneObj.Clone());
            }
            else
            {
                result.Add(element);
            }
        }
        return result;
    }

    public static Task ToTask(this SignalAwaiter signalAwaiter)
    {
        var task = Task.Run(async () => await signalAwaiter);
        return task;
    }
}
