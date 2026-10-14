namespace Acme.Workforce.Assets.Refresh.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh entry calculation.</summary>
public interface IRefreshEntryStep
{
    string Describe();
    double Adjust(RefreshEntry item, double valueValue);
}

public sealed record RegionRefreshEntryAdjuster(Dictionary<string, double> Factors) : IRefreshEntryStep
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(RefreshEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RefreshEntryDiscountOffsetAdjuster(double Weight) : IRefreshEntryStep
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(RefreshEntry item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record RoundingRefreshEntryAdjuster(double Step) : IRefreshEntryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RefreshEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedRefreshEntryAdjuster(Registry Flags, string Flag, double Percent) : IRefreshEntryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RefreshEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentRefreshEntryAdjuster(double Percent) : IRefreshEntryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RefreshEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRefreshEntryAdjuster(double Cap) : IRefreshEntryStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RefreshEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the refresh entry steps in order and keeps a trace of each.</summary>
public sealed class RefreshEntryEngine(List<IRefreshEntryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshEntryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshEntryEngine DefaultRefreshEntryEngine(Registry flags)
    {
        return new RefreshEntryEngine(
            new List<IRefreshEntryStep>
            {
                new RegionRefreshEntryAdjuster(new Dictionary<string, double> { ["west"] = 1.03 }),
                new RefreshEntryDiscountOffsetAdjuster(1.09),
                new RoundingRefreshEntryAdjuster(0.05),
                new FlaggedRefreshEntryAdjuster(flags, "refresh-entry-scheduled", 5.9),
                new PercentRefreshEntryAdjuster(-21.6),
                new CapRefreshEntryAdjuster(13995.0),
            },
            new List<string>(),
            1.8,
            29285.3);
    }

    public double Run(RefreshEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<RefreshEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
