using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

/// <summary>
/// このクラスはJobSystemとBurstCompilerを使用する
/// </summary>
public class LotsTimer_CPU_MultiThread : LotsTimer, ICalculator
{
    private NativeArray<float> _timerArray;

    public new void Initialize()
    {
        _timerArray = new NativeArray<float>(MAX_COUNT, Allocator.Persistent);
        var tempArray = CreateArray();
        _timerArray.CopyFrom(tempArray);
        tempArray.Dispose();    
    }

    public new void Compute()
    {
        var job = new TimerJob
        {
            timerArray = _timerArray,
            deltaTime = Time.deltaTime
        };

        var handle = job.Schedule(_timerArray.Length, 64);
        handle.Complete();
    }


    public new void Dispose()
    {
        if (_timerArray.IsCreated)
        {
            _timerArray.Dispose();
        }
    }

    [BurstCompile]
    private struct TimerJob : IJobParallelFor
    {
        public NativeArray<float> timerArray;
        [ReadOnly] public float deltaTime;

        public void Execute(int index)
        {
            timerArray[index] += deltaTime;
        }
    }
}
