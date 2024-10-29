using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

/// <summary>
/// このクラスはJobSystemとBurstCompilerを使用する
/// </summary>
public class LifeGame_GPU_MultiThread : LifeGame, ICalculator
{
    private NativeArray<int> _stateArray;
    private NativeArray<int> _nextStateArray;

    public new void Initialize()
    {
        _stateArray = CreateInitialStateArray();
        _nextStateArray = new NativeArray<int>(_stateArray.Length, Allocator.Persistent);
    }

    public new void Compute()
    {
        var job = new LifeGameJob
        {
            stateArray = _stateArray,
            nextStateArray = _nextStateArray,
            gridWidth = GridWidth,
            gridHeight = GridHeight
        };

        var handle = job.Schedule(_stateArray.Length, 64);
        handle.Complete();

        // バッファのスワップ
        var temp = _stateArray;
        _stateArray = _nextStateArray;
        _nextStateArray = temp;
    }

    public new void Dispose()
    {
        if (_stateArray.IsCreated) _stateArray.Dispose();
        if (_nextStateArray.IsCreated) _nextStateArray.Dispose();
    }

    private NativeArray<int> CreateInitialStateArray()
    {
        var random = new Random(256);
        var array = new NativeArray<int>(GridWidth * GridHeight, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        for (var i = 0; i < array.Length; i++)
            array[i] = random.NextBool() ? 1 : 0;

        return array;
    }

    [BurstCompile]
    private struct LifeGameJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<int> stateArray;
        public NativeArray<int> nextStateArray;
        public int gridWidth;
        public int gridHeight;

        public void Execute(int index)
        {
            int x = index % gridWidth;
            int y = index / gridWidth;
            int neighbors = CountNeighbors(x, y);

            // ライフゲームのルールを適用
            if (stateArray[index] == 1 && (neighbors < 2 || neighbors > 3))
            {
                nextStateArray[index] = 0; // 過疎または過密で死滅
            }
            else if (stateArray[index] == 0 && neighbors == 3)
            {
                nextStateArray[index] = 1; // 誕生
            }
            else
            {
                nextStateArray[index] = stateArray[index]; // 現状維持
            }
        }

        private int CountNeighbors(int x, int y)
        {
            int count = 0;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;
                    int nx = x + i;
                    int ny = y + j;
                    if (nx >= 0 && nx < gridWidth && ny >= 0 && ny < gridHeight)
                    {
                        int neighborIndex = ny * gridWidth + nx;
                        count += stateArray[neighborIndex];
                    }
                }
            }
            return count;
        }
    }
}
