namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAdjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global benchmark request calculation.</summary>
public interface IGlobalBenchmarkRequestModifier
{
    string Describe();
    double Adjust(GlobalBenchmarkRequest item, double valueValue);
}

public sealed record TieredGlobalBenchmarkRequestAdjuster(List<double> Bounds, List<double> Rates) : IGlobalBenchmarkRequestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalBenchmarkRequest item, double valueValue)
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

public sealed record PercentGlobalBenchmarkRequestAdjuster(double Percent) : IGlobalBenchmarkRequestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalBenchmarkRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocaleGlobalBenchmarkRequestAdjuster(Dictionary<string, double> Factors) : IGlobalBenchmarkRequestModifier
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(GlobalBenchmarkRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record GlobalBenchmarkRequestScoreOffsetAdjuster(double Weight) : IGlobalBenchmarkRequestModifier
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(GlobalBenchmarkRequest item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record RoundingGlobalBenchmarkRequestAdjuster(double Step) : IGlobalBenchmarkRequestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalBenchmarkRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the global benchmark request steps in order and keeps a trace of each.</summary>
public sealed class GlobalBenchmarkRequestEngine(List<IGlobalBenchmarkRequestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalBenchmarkRequestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalBenchmarkRequestEngine DefaultGlobalBenchmarkRequestEngine()
    {
        return new GlobalBenchmarkRequestEngine(
            new List<IGlobalBenchmarkRequestModifier>
            {
                new TieredGlobalBenchmarkRequestAdjuster(
                    new List<double> { 2606.0, 9490.0 },
                    new List<double> { 1.04, 0.93 }),
                new PercentGlobalBenchmarkRequestAdjuster(18.3),
                new LocaleGlobalBenchmarkRequestAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.38, ["west"] = 0.99, ["north"] = 1.1 }),
                new GlobalBenchmarkRequestScoreOffsetAdjuster(1.58),
                new RoundingGlobalBenchmarkRequestAdjuster(0.05),
            },
            new List<string>(),
            0.7,
            31808.0);
    }

    public double Run(GlobalBenchmarkRequest item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalBenchmarkRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
