namespace Acme.Workforce.Compensation.Benchmark.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered benchmark profile calculation.</summary>
public interface ITieredBenchmarkProfileModifier
{
    string Describe();
    double Adjust(TieredBenchmarkProfile item, double valueValue);
}

public sealed record TieredBenchmarkProfileCreatedAtOffsetAdjuster(double Weight) : ITieredBenchmarkProfileModifier
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(TieredBenchmarkProfile item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record CapTieredBenchmarkProfileAdjuster(double Cap) : ITieredBenchmarkProfileModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredBenchmarkProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingTieredBenchmarkProfileAdjuster(double Step) : ITieredBenchmarkProfileModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredBenchmarkProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumTieredBenchmarkProfileAdjuster(double Minimum) : ITieredBenchmarkProfileModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredBenchmarkProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RegionTieredBenchmarkProfileAdjuster(Dictionary<string, double> Factors) : ITieredBenchmarkProfileModifier
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(TieredBenchmarkProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedTieredBenchmarkProfileAdjuster(Registry Flags, string Flag, double Percent) : ITieredBenchmarkProfileModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredBenchmarkProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tiered benchmark profile steps in order and keeps a trace of each.</summary>
public sealed class TieredBenchmarkProfileEngine(List<ITieredBenchmarkProfileModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredBenchmarkProfileModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredBenchmarkProfileEngine DefaultTieredBenchmarkProfileEngine(Registry flags)
    {
        return new TieredBenchmarkProfileEngine(
            new List<ITieredBenchmarkProfileModifier>
            {
                new TieredBenchmarkProfileCreatedAtOffsetAdjuster(0.97),
                new CapTieredBenchmarkProfileAdjuster(27509.0),
                new RoundingTieredBenchmarkProfileAdjuster(0.05),
                new MinimumTieredBenchmarkProfileAdjuster(29.1),
                new RegionTieredBenchmarkProfileAdjuster(new Dictionary<string, double> { ["north"] = 1.33 }),
                new FlaggedTieredBenchmarkProfileAdjuster(flags, "tiered-benchmark-profile-regional", -0.4),
            },
            new List<string>(),
            3.7,
            7417.8);
    }

    public double Run(TieredBenchmarkProfile item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TieredBenchmarkProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
