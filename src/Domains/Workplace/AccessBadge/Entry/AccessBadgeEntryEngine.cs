namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge entry calculation.</summary>
public interface IAccessBadgeEntryModifier
{
    string Describe();
    double Adjust(AccessBadgeEntry item, double valueValue);
}

public sealed record PercentAccessBadgeEntryAdjuster(double Percent) : IAccessBadgeEntryModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccessBadgeEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TeamAccessBadgeEntryAdjuster(Dictionary<string, double> Factors) : IAccessBadgeEntryModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(AccessBadgeEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedAccessBadgeEntryAdjuster(Registry Flags, string Flag, double Percent) : IAccessBadgeEntryModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccessBadgeEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAccessBadgeEntryAdjuster(double Step) : IAccessBadgeEntryModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccessBadgeEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapAccessBadgeEntryAdjuster(double Cap) : IAccessBadgeEntryModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccessBadgeEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the access badge entry steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeEntryEngine(List<IAccessBadgeEntryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeEntryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeEntryEngine DefaultAccessBadgeEntryEngine(Registry flags)
    {
        return new AccessBadgeEntryEngine(
            new List<IAccessBadgeEntryModifier>
            {
                new PercentAccessBadgeEntryAdjuster(5.7),
                new TeamAccessBadgeEntryAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.08, ["central"] = 1.18, ["east"] = 1.24 }),
                new FlaggedAccessBadgeEntryAdjuster(flags, "access-badge-entry-adaptive", 1.8),
                new RoundingAccessBadgeEntryAdjuster(0.05),
                new CapAccessBadgeEntryAdjuster(15538.0),
            },
            new List<string>(),
            4.0,
            98121.1);
    }

    public double Run(AccessBadgeEntry item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
