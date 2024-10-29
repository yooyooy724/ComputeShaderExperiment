using UnityEngine;
using System.Linq;
public interface ICalculator
{
    void Initialize();
    void Compute();
    void Dispose();
}

public class StopWatch : MonoBehaviour
{
    private ICalculator calculator;
    [SerializeField] int computeTimesCount = 100;
    private void Awake()
    {
        calculator = GetComponent<ICalculator>();
        if (calculator == null)
            throw new System.Exception("ICalculatorが見つかりません");

        calculator.Initialize();
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        for (int i = 0; i < computeTimesCount; i++)
        {
            calculator.Compute();
        }
        stopwatch.Stop();

        Debug.Log($"{gameObject.name}の計算時間: {stopwatch.ElapsedMilliseconds:F6}ミリ秒");

        calculator.Dispose();

        Destroy(gameObject);
    }
}
