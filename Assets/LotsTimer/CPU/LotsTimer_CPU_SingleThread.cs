using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

public class LotsTimer_CPU_SingleThread : LotsTimer, ICalculator
{
    private NativeArray<float> _timerArray;

    public new void Initialize()
    {
        _timerArray = CreateArray();
    }

    public new void Compute()
    {
        for (int i = 0; i < MAX_COUNT; i++)
        {
            _timerArray[i] += Time.deltaTime;
        }
    }

    public new void Dispose()
    {
        if (_timerArray.IsCreated)
        {
            _timerArray.Dispose();
        }
    }
}