namespace Acme.Workforce.Contractors.Engagement.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal engagement rollup calculation.</summary>
public interface ISeasonalEngagementRollupFactor
{
    string Describe();
    double Adjust(SeasonalEngagementRollup item, double valueValue);
}

public sealed record FlaggedSeasonalEngagementRollupAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalEngagementRollupFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalEngagementRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SeasonalEngagementRollupCostOffsetAdjuster(double Weight) : ISeasonalEngagementRollupFactor
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(SeasonalEngagementRollup item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record PercentSeasonalEngagementRollupAdjuster(double Percent) : ISeasonalEngagementRollupFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalEngagementRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSeasonalEngagementRollupAdjuster(double Cap) : ISeasonalEngagementRollupFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalEngagementRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingSeasonalEngagementRollupAdjuster(double Step) : ISeasonalEngagementRollupFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalEngagementRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the seasonal engagement rollup steps in order and keeps a trace of each.</summary>
public sealed class SeasonalEngagementRollupEngine(List<ISeasonalEngagementRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalEngagementRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalEngagementRollupEngine DefaultSeasonalEngagementRollupEngine(Registry flags)
    {
        return new SeasonalEngagementRollupEngine(
            new List<ISeasonalEngagementRollupFactor>
            {
                new FlaggedSeasonalEngagementRollupAdjuster(flags, "seasonal-engagement-rollup-primary", -4.5),
                new SeasonalEngagementRollupCostOffsetAdjuster(1.76),
                new PercentSeasonalEngagementRollupAdjuster(16.5),
                new CapSeasonalEngagementRollupAdjuster(39392.0),
                new RoundingSeasonalEngagementRollupAdjuster(1.0),
            },
            new List<string>(),
            4.7,
            14551.3);
    }

    public double Compute(SeasonalEngagementRollup item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalEngagementRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
