namespace Acme.Workforce.Compensation.Benchmark.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the benchmark threshold calculation.</summary>
public interface IBenchmarkThresholdStep
{
    string Describe();
    double Adjust(BenchmarkThreshold item, double valueValue);
}

public sealed record CapBenchmarkThresholdAdjuster(double Cap) : IBenchmarkThresholdStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BenchmarkThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelBenchmarkThresholdAdjuster(Dictionary<string, double> Factors) : IBenchmarkThresholdStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(BenchmarkThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredBenchmarkThresholdAdjuster(List<double> Bounds, List<double> Rates) : IBenchmarkThresholdStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BenchmarkThreshold item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

/// <summary>Runs the benchmark threshold steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkThresholdEngine(List<IBenchmarkThresholdStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkThresholdStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkThresholdEngine DefaultBenchmarkThresholdEngine()
    {
        return new BenchmarkThresholdEngine(
            new List<IBenchmarkThresholdStep>
            {
                new CapBenchmarkThresholdAdjuster(44852.0),
                new LabelBenchmarkThresholdAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.76, ["west"] = 1.05, ["south"] = 1.21 }),
                new TieredBenchmarkThresholdAdjuster(
                    new List<double> { 2153.0, 5155.0, 9252.0, 9895.0 },
                    new List<double> { 0.97, 0.96, 1.09, 1.13 }),
            },
            new List<string>(),
            2.2,
            58062.2);
    }

    public double Compute(BenchmarkThreshold item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<BenchmarkThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
