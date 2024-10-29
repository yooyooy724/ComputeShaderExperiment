using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;
using Unity.Collections;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

public class LifeGame : MonoBehaviour, ICalculator
{
    [SerializeField] ComputeShader _compute = null;
    [SerializeField] int gridWidth = 512;
    [SerializeField] int gridHeight = 512;
    VisualEffect _visualEffect;

    protected int GridWidth => gridWidth;
    protected int GridHeight => gridHeight;
    protected int MAX_COUNT => GridWidth * GridHeight; // グリッドの最大セル数

    GraphicsBuffer _stateBuffer;
    GraphicsBuffer _nextStateBuffer;
    const string KERNEL_NAME = "LifeGameCS";
    int _kernelIndex;

    void OnEnable() => Initialize();

    public void Initialize()
    {
        if(_compute == null) return;

        // 1. GPUのメモリ確保
        _stateBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            MAX_COUNT, 
            Marshal.SizeOf<int>());
        _nextStateBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            MAX_COUNT, 
            Marshal.SizeOf<int>());

        // 初期の生存状態をランダムに設定
        var initialState = CreateInitialStateArray();
        _stateBuffer.SetData(initialState);
        initialState.Dispose();

        // 2. ComputeShaderにバッファを設定
        _kernelIndex = _compute.FindKernel(KERNEL_NAME);
        _compute.SetBuffer(_kernelIndex, "stateBuffer", _stateBuffer);
        _compute.SetBuffer(_kernelIndex, "nextStateBuffer", _nextStateBuffer);
        _compute.SetInt("gridWidth", GridWidth);
        _compute.SetInt("gridHeight", GridHeight);

        InitializeVFX();
    }

    protected NativeArray<int> CreateInitialStateArray()
    {  
        var random = new Random(256);
        var array = new NativeArray<int>(MAX_COUNT, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        // ランダムに初期状態を設定
        for (var i = 0; i < array.Length; i++)
            array[i] = random.NextBool() ? 1 : 0;

        return array;
    }

    protected void InitializeVFX()
    {
        _visualEffect = GetComponent<VisualEffect>();
        _visualEffect.SetGraphicsBuffer("StateBuffer", _stateBuffer);
        _visualEffect.SetInt("GridWidth", GridWidth);
        _visualEffect.SetInt("GridHeight", GridHeight);
    }

    void Update() => Compute();

    public void Compute()
    {
        if(_compute == null) return;

        // 3. カーネル呼び出し
        _compute.GetKernelThreadGroupSizes(_kernelIndex, out var x, out var y, out _);
        _compute.Dispatch(_kernelIndex, (int)(GridWidth / x), (int)(GridHeight / y), 1);

        // 4. バッファのスワップ
        var temp = _stateBuffer;
        _stateBuffer = _nextStateBuffer;
        _nextStateBuffer = temp;

        // 次のフレームで最新の状態を使用するようにセット
        _compute.SetBuffer(_kernelIndex, "stateBuffer", _stateBuffer);
        _compute.SetBuffer(_kernelIndex, "nextStateBuffer", _nextStateBuffer);
    }

    void OnDisable() => Dispose();

    public void Dispose()
    {
        // 5. GPUメモリ破棄
        _stateBuffer?.Dispose();
        _nextStateBuffer?.Dispose();
    }
}
