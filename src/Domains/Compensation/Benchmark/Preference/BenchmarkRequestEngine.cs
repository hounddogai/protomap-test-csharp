namespace Acme.Workforce.Compensation.Benchmark.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the benchmark request calculation.</summary>
public interface IBenchmarkRequestStep
{
    string Describe();
    double Adjust(BenchmarkRequest item, double valueValue);
}

public sealed record CapBenchmarkRequestAdjuster(double Cap) : IBenchmarkRequestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BenchmarkRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record BenchmarkRequestSequenceOffsetAdjuster(double Weight) : IBenchmarkRequestStep
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(BenchmarkRequest item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record RoundingBenchmarkRequestAdjuster(double Step) : IBenchmarkRequestStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BenchmarkRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the benchmark request steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkRequestEngine(List<IBenchmarkRequestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkRequestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkRequestEngine DefaultBenchmarkRequestEngine()
    {
        return new BenchmarkRequestEngine(
            new List<IBenchmarkRequestStep>
            {
                new CapBenchmarkRequestAdjuster(19831.0),
                new BenchmarkRequestSequenceOffsetAdjuster(0.97),
                new RoundingBenchmarkRequestAdjuster(1.0),
            },
            new List<string>(),
            1.5,
            67177.4);
    }

    public double Run(BenchmarkRequest item)
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

    public Dictionary<string, double> EvaluateAll(List<BenchmarkRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
