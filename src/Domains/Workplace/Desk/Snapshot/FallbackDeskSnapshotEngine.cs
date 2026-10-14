namespace Acme.Workforce.Workplace.Desk.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback desk snapshot calculation.</summary>
public interface IFallbackDeskSnapshotFactor
{
    string Describe();
    double Adjust(FallbackDeskSnapshot item, double valueValue);
}

public sealed record TitleFallbackDeskSnapshotAdjuster(Dictionary<string, double> Factors) : IFallbackDeskSnapshotFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(FallbackDeskSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedFallbackDeskSnapshotAdjuster(Registry Flags, string Flag, double Percent) : IFallbackDeskSnapshotFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackDeskSnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FallbackDeskSnapshotCreatedAtOffsetAdjuster(double Weight) : IFallbackDeskSnapshotFactor
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(FallbackDeskSnapshot item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record MinimumFallbackDeskSnapshotAdjuster(double Minimum) : IFallbackDeskSnapshotFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackDeskSnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredFallbackDeskSnapshotAdjuster(List<double> Bounds, List<double> Rates) : IFallbackDeskSnapshotFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackDeskSnapshot item, double valueValue)
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

/// <summary>Runs the fallback desk snapshot steps in order and keeps a trace of each.</summary>
public sealed class FallbackDeskSnapshotEngine(List<IFallbackDeskSnapshotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackDeskSnapshotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackDeskSnapshotEngine DefaultFallbackDeskSnapshotEngine(Registry flags)
    {
        return new FallbackDeskSnapshotEngine(
            new List<IFallbackDeskSnapshotFactor>
            {
                new TitleFallbackDeskSnapshotAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.82, ["west"] = 0.96, ["retail"] = 0.79 }),
                new FlaggedFallbackDeskSnapshotAdjuster(flags, "fallback-desk-snapshot-fallback", -7.1),
                new FallbackDeskSnapshotCreatedAtOffsetAdjuster(0.89),
                new MinimumFallbackDeskSnapshotAdjuster(14.2),
                new TieredFallbackDeskSnapshotAdjuster(
                    new List<double> { 897.0, 2885.0, 7473.0, 9454.0 },
                    new List<double> { 1.14, 1.0, 0.82, 0.96 }),
            },
            new List<string>(),
            1.3,
            69404.7);
    }

    public double Price(FallbackDeskSnapshot item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackDeskSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
