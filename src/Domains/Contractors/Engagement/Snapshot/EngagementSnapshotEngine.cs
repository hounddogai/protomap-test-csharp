namespace Acme.Workforce.Contractors.Engagement.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the engagement snapshot calculation.</summary>
public interface IEngagementSnapshotFactor
{
    string Describe();
    double Adjust(EngagementSnapshot item, double valueValue);
}

public sealed record TieredEngagementSnapshotAdjuster(List<double> Bounds, List<double> Rates) : IEngagementSnapshotFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EngagementSnapshot item, double valueValue)
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

public sealed record MinimumEngagementSnapshotAdjuster(double Minimum) : IEngagementSnapshotFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EngagementSnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedEngagementSnapshotAdjuster(Registry Flags, string Flag, double Percent) : IEngagementSnapshotFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EngagementSnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the engagement snapshot steps in order and keeps a trace of each.</summary>
public sealed class EngagementSnapshotEngine(List<IEngagementSnapshotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementSnapshotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementSnapshotEngine DefaultEngagementSnapshotEngine(Registry flags)
    {
        return new EngagementSnapshotEngine(
            new List<IEngagementSnapshotFactor>
            {
                new TieredEngagementSnapshotAdjuster(
                    new List<double> { 1921.0, 4113.0, 5757.0, 5893.0 },
                    new List<double> { 1.17, 1.09, 1.2, 1.15 }),
                new MinimumEngagementSnapshotAdjuster(43.7),
                new FlaggedEngagementSnapshotAdjuster(flags, "engagement-snapshot-capped", -1.4),
            },
            new List<string>(),
            4.9,
            32046.2);
    }

    public double Run(EngagementSnapshot item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<EngagementSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
