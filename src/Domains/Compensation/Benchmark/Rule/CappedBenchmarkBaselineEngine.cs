namespace Acme.Workforce.Compensation.Benchmark.Rule;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the capped benchmark baseline calculation.</summary>
public interface ICappedBenchmarkBaselineModifier
{
    string Describe();
    double Adjust(CappedBenchmarkBaseline item, double valueValue);
}

public sealed record CappedBenchmarkBaselineVersionOffsetAdjuster(double Weight) : ICappedBenchmarkBaselineModifier
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(CappedBenchmarkBaseline item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record TieredCappedBenchmarkBaselineAdjuster(List<double> Bounds, List<double> Rates) : ICappedBenchmarkBaselineModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedBenchmarkBaseline item, double valueValue)
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

public sealed record PercentCappedBenchmarkBaselineAdjuster(double Percent) : ICappedBenchmarkBaselineModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedBenchmarkBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedCappedBenchmarkBaselineAdjuster(Registry Flags, string Flag, double Percent) : ICappedBenchmarkBaselineModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedBenchmarkBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapCappedBenchmarkBaselineAdjuster(double Cap) : ICappedBenchmarkBaselineModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedBenchmarkBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the capped benchmark baseline steps in order and keeps a trace of each.</summary>
public sealed class CappedBenchmarkBaselineEngine(List<ICappedBenchmarkBaselineModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedBenchmarkBaselineModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedBenchmarkBaselineEngine DefaultCappedBenchmarkBaselineEngine(Registry flags)
    {
        return new CappedBenchmarkBaselineEngine(
            new List<ICappedBenchmarkBaselineModifier>
            {
                new CappedBenchmarkBaselineVersionOffsetAdjuster(0.84),
                new TieredCappedBenchmarkBaselineAdjuster(
                    new List<double> { 1974.0, 2535.0, 5958.0, 6413.0 },
                    new List<double> { 0.99, 0.97, 1.01, 0.86 }),
                new PercentCappedBenchmarkBaselineAdjuster(-8.1),
                new FlaggedCappedBenchmarkBaselineAdjuster(flags, "capped-benchmark-baseline-pending", -0.4),
                new CapCappedBenchmarkBaselineAdjuster(10668.0),
            },
            new List<string>(),
            2.0,
            47729.7);
    }

    public double Compute(CappedBenchmarkBaseline item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CappedBenchmarkBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
