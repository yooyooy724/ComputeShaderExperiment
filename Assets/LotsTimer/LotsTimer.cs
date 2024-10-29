using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;
using Unity.Collections;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

public class LotsTimer : MonoBehaviour, ICalculator
{
    [SerializeField] ComputeShader _compute = null;
    VisualEffect _visualEffect;

    protected const int MAX_COUNT = 250000;

    GraphicsBuffer _timerBuffer;
    const string KERNEL_NAME = "LotsTimerCS";
    int _kernelIndex;
    
    void OnEnable() => Initialize();
    public void Initialize()
    {
        // 1. GPUのメモリ確保
        _timerBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.Structured,
            MAX_COUNT, 
            Marshal.SizeOf<float>());
                
        var tempArray = CreateArray();
        _timerBuffer.SetData(tempArray);
        tempArray.Dispose();

        // 2. 入力データの転送
        _kernelIndex = _compute.FindKernel(KERNEL_NAME);
        _compute.SetBuffer(_kernelIndex, "timeBuffer", _timerBuffer);
        _compute.SetInt("timeBufferLength", MAX_COUNT);

        InitializeVFX();
    }

    protected NativeArray<float> CreateArray()
    {  
        var random = new Random(256);
        var array = new NativeArray<float>(MAX_COUNT, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        for (var i = 0; i < array.Length; i++)
            array[i] = random.NextFloat() * 360;

        return array;
    }

    protected void InitializeVFX()
    {
        _visualEffect = GetComponent<VisualEffect>();
        _visualEffect.SetGraphicsBuffer("TimeBuffer", _timerBuffer);
        _visualEffect.SetInt("TimerCount", MAX_COUNT);
    }

    void Update() => Compute();
    public void Compute()
    {
        _compute.SetFloat("deltaTime", Time.deltaTime);

        // 3. カーネル呼び出し
        _compute.GetKernelThreadGroupSizes(0, out var x, out var y, out var z);
        _compute.Dispatch(_kernelIndex, (int) (MAX_COUNT / x), 1, 1);

        // 5はなし
    } 

    void OnDisable() => Dispose();
    public void Dispose()
    {
        // 6. GPUメモリ破棄
        _timerBuffer?.Dispose();
    }


    

}
