namespace Acme.Workforce.Compensation.Benchmark.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered benchmark score calculation.</summary>
public interface ITieredBenchmarkScoreAdjuster
{
    string Describe();
    double Adjust(TieredBenchmarkScore item, double valueValue);
}

public sealed record MinimumTieredBenchmarkScoreAdjuster(double Minimum) : ITieredBenchmarkScoreAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredBenchmarkScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameTieredBenchmarkScoreAdjuster(Dictionary<string, double> Factors) : ITieredBenchmarkScoreAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(TieredBenchmarkScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingTieredBenchmarkScoreAdjuster(double Step) : ITieredBenchmarkScoreAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredBenchmarkScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredTieredBenchmarkScoreAdjuster(List<double> Bounds, List<double> Rates) : ITieredBenchmarkScoreAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredBenchmarkScore item, double valueValue)
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

/// <summary>Runs the tiered benchmark score steps in order and keeps a trace of each.</summary>
public sealed class TieredBenchmarkScoreEngine(List<ITieredBenchmarkScoreAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredBenchmarkScoreAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredBenchmarkScoreEngine DefaultTieredBenchmarkScoreEngine()
    {
        return new TieredBenchmarkScoreEngine(
            new List<ITieredBenchmarkScoreAdjuster>
            {
                new MinimumTieredBenchmarkScoreAdjuster(18.1),
                new NameTieredBenchmarkScoreAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.39, ["west"] = 1.35 }),
                new RoundingTieredBenchmarkScoreAdjuster(1.0),
                new TieredTieredBenchmarkScoreAdjuster(
                    new List<double> { 5614.0, 6786.0 },
                    new List<double> { 0.9, 1.05 }),
            },
            new List<string>(),
            3.9,
            65473.6);
    }

    public double Evaluate(TieredBenchmarkScore item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TieredBenchmarkScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
