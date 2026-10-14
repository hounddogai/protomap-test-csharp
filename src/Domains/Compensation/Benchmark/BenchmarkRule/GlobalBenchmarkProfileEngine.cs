namespace Acme.Workforce.Compensation.Benchmark.BenchmarkRule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global benchmark profile calculation.</summary>
public interface IGlobalBenchmarkProfileStep
{
    string Describe();
    double Adjust(GlobalBenchmarkProfile item, double valueValue);
}

public sealed record CapGlobalBenchmarkProfileAdjuster(double Cap) : IGlobalBenchmarkProfileStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalBenchmarkProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentGlobalBenchmarkProfileAdjuster(double Percent) : IGlobalBenchmarkProfileStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalBenchmarkProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelGlobalBenchmarkProfileAdjuster(Dictionary<string, double> Factors) : IGlobalBenchmarkProfileStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(GlobalBenchmarkProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the global benchmark profile steps in order and keeps a trace of each.</summary>
public sealed class GlobalBenchmarkProfileEngine(List<IGlobalBenchmarkProfileStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalBenchmarkProfileStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalBenchmarkProfileEngine DefaultGlobalBenchmarkProfileEngine()
    {
        return new GlobalBenchmarkProfileEngine(
            new List<IGlobalBenchmarkProfileStep>
            {
                new CapGlobalBenchmarkProfileAdjuster(9427.0),
                new PercentGlobalBenchmarkProfileAdjuster(17.6),
                new LabelGlobalBenchmarkProfileAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.93, ["online"] = 1.32 }),
            },
            new List<string>(),
            0.0,
            1026.9);
    }

    public double Run(GlobalBenchmarkProfile item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalBenchmarkProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
