namespace Acme.Workforce.Compensation.Benchmark.Profile;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the benchmark score calculation.</summary>
public interface IBenchmarkScoreFactor
{
    string Describe();
    double Adjust(BenchmarkScore item, double valueValue);
}

public sealed record SourceBenchmarkScoreAdjuster(Dictionary<string, double> Factors) : IBenchmarkScoreFactor
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(BenchmarkScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedBenchmarkScoreAdjuster(Registry Flags, string Flag, double Percent) : IBenchmarkScoreFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BenchmarkScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentBenchmarkScoreAdjuster(double Percent) : IBenchmarkScoreFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BenchmarkScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredBenchmarkScoreAdjuster(List<double> Bounds, List<double> Rates) : IBenchmarkScoreFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BenchmarkScore item, double valueValue)
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

/// <summary>Runs the benchmark score steps in order and keeps a trace of each.</summary>
public sealed class BenchmarkScoreEngine(List<IBenchmarkScoreFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBenchmarkScoreFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BenchmarkScoreEngine DefaultBenchmarkScoreEngine(Registry flags)
    {
        return new BenchmarkScoreEngine(
            new List<IBenchmarkScoreFactor>
            {
                new SourceBenchmarkScoreAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.06, ["north"] = 1.15 }),
                new FlaggedBenchmarkScoreAdjuster(flags, "benchmark-score-rolling", 1.8),
                new PercentBenchmarkScoreAdjuster(-15.0),
                new TieredBenchmarkScoreAdjuster(
                    new List<double> { 1619.0, 7250.0, 8591.0 },
                    new List<double> { 1.09, 0.82, 1.01 }),
            },
            new List<string>(),
            3.2,
            15508.9);
    }

    public double Run(BenchmarkScore item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<BenchmarkScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
