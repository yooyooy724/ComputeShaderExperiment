using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

public class LifeGame_CPU_SingleThread : LifeGame, ICalculator
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
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                int index = y * GridWidth + x;
                int neighbors = CountNeighbors(x, y);

                // ライフゲームのルールを適用
                if (_stateArray[index] == 1 && (neighbors < 2 || neighbors > 3))
                {
                    _nextStateArray[index] = 0; // 過疎または過密で死滅
                }
                else if (_stateArray[index] == 0 && neighbors == 3)
                {
                    _nextStateArray[index] = 1; // 誕生
                }
                else
                {
                    _nextStateArray[index] = _stateArray[index]; // 現状維持
                }
            }
        }

        // バッファのスワップ
        NativeArray<int> temp = _stateArray;
        _stateArray = _nextStateArray;
        _nextStateArray = temp;
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
                if (nx >= 0 && nx < GridWidth && ny >= 0 && ny < GridHeight)
                {
                    count += _stateArray[ny * GridWidth + nx];
                }
            }
        }
        return count;
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
}
