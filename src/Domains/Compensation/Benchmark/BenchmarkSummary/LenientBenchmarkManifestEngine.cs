namespace Acme.Workforce.Compensation.Benchmark.BenchmarkSummary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient benchmark manifest calculation.</summary>
public interface ILenientBenchmarkManifestAdjuster
{
    string Describe();
    double Adjust(LenientBenchmarkManifest item, double valueValue);
}

public sealed record SourceLenientBenchmarkManifestAdjuster(Dictionary<string, double> Factors) : ILenientBenchmarkManifestAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(LenientBenchmarkManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumLenientBenchmarkManifestAdjuster(double Minimum) : ILenientBenchmarkManifestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientBenchmarkManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LenientBenchmarkManifestEndsAtOffsetAdjuster(double Weight) : ILenientBenchmarkManifestAdjuster
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(LenientBenchmarkManifest item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record PercentLenientBenchmarkManifestAdjuster(double Percent) : ILenientBenchmarkManifestAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientBenchmarkManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLenientBenchmarkManifestAdjuster(List<double> Bounds, List<double> Rates) : ILenientBenchmarkManifestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientBenchmarkManifest item, double valueValue)
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

public sealed record RoundingLenientBenchmarkManifestAdjuster(double Step) : ILenientBenchmarkManifestAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientBenchmarkManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the lenient benchmark manifest steps in order and keeps a trace of each.</summary>
public sealed class LenientBenchmarkManifestEngine(List<ILenientBenchmarkManifestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientBenchmarkManifestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientBenchmarkManifestEngine DefaultLenientBenchmarkManifestEngine()
    {
        return new LenientBenchmarkManifestEngine(
            new List<ILenientBenchmarkManifestAdjuster>
            {
                new SourceLenientBenchmarkManifestAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.19, ["online"] = 1.09, ["south"] = 1.27 }),
                new MinimumLenientBenchmarkManifestAdjuster(27.3),
                new LenientBenchmarkManifestEndsAtOffsetAdjuster(0.37),
                new PercentLenientBenchmarkManifestAdjuster(-12.9),
                new TieredLenientBenchmarkManifestAdjuster(
                    new List<double> { 2532.0, 4800.0, 7165.0, 9863.0 },
                    new List<double> { 1.08, 1.06, 1.08, 1.06 }),
                new RoundingLenientBenchmarkManifestAdjuster(1.0),
            },
            new List<string>(),
            1.9,
            68739.7);
    }

    public double Evaluate(LenientBenchmarkManifest item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LenientBenchmarkManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
