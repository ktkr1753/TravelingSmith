using Godot;
using System;

public partial class StepRandom
{
    private int _seed;
    public int seed { get { return _seed; } private set { _seed = value; } }

    private Random _rnd;
    public Random rnd { get { return _rnd; } private set { _rnd = value; } }

    public StepRandom(int seed)
    {
        this.seed = seed;
        rnd = new Random(seed);
    }

    public int GetRange(int bound1, int bound2)
    {
        int result;

        int min = Mathf.Min(bound1, bound2);
        int max = Mathf.Max(bound1, bound2);

        result = rnd.Next(min, max);
        //Debug.Print("bound1:{0},bound2:{1},result:{2}", bound1, bound2, result);
        return result;
    }

    public float GetRange(float bound1, float bound2)
    {
        float result;

        float min = Mathf.Min(bound1, bound2);
        float max = Mathf.Max(bound1, bound2);

        result = (float)(rnd.NextDouble() * (max - min) + min);
        //Debug.Print("bound1:{0},bound2:{1},result:{2}", bound1, bound2, result);
        return result;
    }
}
