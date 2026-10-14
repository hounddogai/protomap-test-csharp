namespace Acme.Workforce.Workplace.AccessBadge.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge snapshot calculation.</summary>
public interface IAccessBadgeSnapshotModifier
{
    string Describe();
    double Adjust(AccessBadgeSnapshot item, double valueValue);
}

public sealed record TieredAccessBadgeSnapshotAdjuster(List<double> Bounds, List<double> Rates) : IAccessBadgeSnapshotModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccessBadgeSnapshot item, double valueValue)
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

public sealed record LabelAccessBadgeSnapshotAdjuster(Dictionary<string, double> Factors) : IAccessBadgeSnapshotModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AccessBadgeSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedAccessBadgeSnapshotAdjuster(Registry Flags, string Flag, double Percent) : IAccessBadgeSnapshotModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccessBadgeSnapshot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the access badge snapshot steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeSnapshotEngine(List<IAccessBadgeSnapshotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeSnapshotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeSnapshotEngine DefaultAccessBadgeSnapshotEngine(Registry flags)
    {
        return new AccessBadgeSnapshotEngine(
            new List<IAccessBadgeSnapshotModifier>
            {
                new TieredAccessBadgeSnapshotAdjuster(
                    new List<double> { 2815.0, 2882.0, 3511.0, 8585.0 },
                    new List<double> { 1.02, 0.82, 0.83, 1.1 }),
                new LabelAccessBadgeSnapshotAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.92, ["south"] = 1.24 }),
                new FlaggedAccessBadgeSnapshotAdjuster(flags, "access-badge-snapshot-manual", 3.4),
            },
            new List<string>(),
            4.7,
            93977.9);
    }

    public double Run(AccessBadgeSnapshot item)
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
