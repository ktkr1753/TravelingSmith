using Godot;
using System;
using System.Collections.Generic;

public partial class RandomManager
{
    public Dictionary<RandomType, StepRandom> randomDic = new Dictionary<RandomType, StepRandom>();

    protected void InitRandom(RandomType randomType)
    {
        Random rnd = new Random();
        InitRandom(randomType, rnd.Next());
    }

    protected void InitRandom(RandomType randomType, int seed)
    {
        StepRandom stepRandom = new StepRandom(seed);
        randomDic[randomType] = stepRandom;
    }

    //取得指定數值bound1與bound2之間的隨機數，包含最小數但不包含最大數
    public int GetRange(RandomType randomType, int bound1, int bound2)
    {
        int result = 0;

        if (!randomDic.ContainsKey(randomType))
        {
            InitRandom(randomType);
        }

        result = randomDic[randomType].GetRange(bound1, bound2);

        return result;
    }

    //取得指定數值bound1與bound2之間的隨機數，包含最小數但不包含最大數
    public float GetRange(RandomType randomType, float bound1, float bound2)
    {
        float result = 0;

        if (!randomDic.ContainsKey(randomType))
        {
            InitRandom(randomType);
        }

        result = randomDic[randomType].GetRange(bound1, bound2);

        return result;
    }

    /// <summary>
    /// 取得於最大最小區間的隨機不重複數列(包含最小值,不包含最大值)
    /// </summary>
    /// <param name="randomType"></param> 該隨機數用途
    /// <param name="bound1"></param> 最小值
    /// <param name="bound2"></param> 最大值
    /// <param name="pickNum"></param> 數列的元素數量
    /// <returns></returns>
    public List<int> GetNotRepeatList(RandomType randomType, int bound1, int bound2, int pickNum = 0)
    {
        List<int> result = new List<int>();
        int min = Mathf.Min(bound1, bound2);
        int max = Mathf.Max(bound1, bound2);

        List<int> tempList = new List<int>();
        for (int i = min; i < max; i++)
        {
            tempList.Add(i);
        }
        if(pickNum == 0) 
        {
            pickNum = tempList.Count;
        }
        else 
        {
            pickNum = Math.Clamp(pickNum, 0, tempList.Count);
        }

        for (int i = 0; i < pickNum; i++)
        {
            int randomPick = GetRange(randomType, 0, tempList.Count);
            result.Add(tempList[randomPick]);
            tempList.RemoveAt(randomPick);
        }

        return result;
    }

    public List<T> GetRandomOrderList<T>(RandomType randomType, List<T> list)
    {

        return GetRandomListOfNumber(randomType, list, list.Count);
    }

    /// <summary>
    /// 從該集合中的隨機取得一定數量
    /// </summary>
    /// <param name="randomType"></param> 該隨機數用途
    /// <param name="list"></param> 集合
    /// <param name="number"></param> 要取的數量
    /// <returns></returns>
    public List<T> GetRandomListOfNumber<T>(RandomType randomType, List<T> list, int number)
    {
        List<T> resultList = new List<T>();
        if (number > list.Count || number < 0)
        {
            Debug.PrintErr("當前list長度為:", list.Count, "不可輸入數字:", number);
            return resultList;
        }

        List<int> orders = GetNotRepeatList(randomType, 0, list.Count);
        for (int i = 0; i < number; i++)
        {
            resultList.Add(list[orders[i]]);
        }

        return resultList;
    }
}
