namespace Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the access badge override calculation.</summary>
public interface IAccessBadgeOverrideModifier
{
    string Describe();
    double Adjust(AccessBadgeOverride item, double valueValue);
}

public sealed record AccessBadgeOverrideScoreOffsetAdjuster(double Weight) : IAccessBadgeOverrideModifier
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(AccessBadgeOverride item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record TieredAccessBadgeOverrideAdjuster(List<double> Bounds, List<double> Rates) : IAccessBadgeOverrideModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccessBadgeOverride item, double valueValue)
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

public sealed record FlaggedAccessBadgeOverrideAdjuster(Registry Flags, string Flag, double Percent) : IAccessBadgeOverrideModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccessBadgeOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the access badge override steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeOverrideEngine(List<IAccessBadgeOverrideModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeOverrideModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeOverrideEngine DefaultAccessBadgeOverrideEngine(Registry flags)
    {
        return new AccessBadgeOverrideEngine(
            new List<IAccessBadgeOverrideModifier>
            {
                new AccessBadgeOverrideScoreOffsetAdjuster(0.61),
                new TieredAccessBadgeOverrideAdjuster(
                    new List<double> { 4955.0, 9304.0, 9579.0 },
                    new List<double> { 0.86, 0.96, 0.86 }),
                new FlaggedAccessBadgeOverrideAdjuster(flags, "access-badge-override-strict", -8.5),
            },
            new List<string>(),
            0.2,
            22366.5);
    }

    public double Run(AccessBadgeOverride item)
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
