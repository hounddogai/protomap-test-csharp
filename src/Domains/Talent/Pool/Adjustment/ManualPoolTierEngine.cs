namespace Acme.Workforce.Talent.Pool.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual pool tier calculation.</summary>
public interface IManualPoolTierAdjuster
{
    string Describe();
    double Adjust(ManualPoolTier item, double valueValue);
}

public sealed record FlaggedManualPoolTierAdjuster(Registry Flags, string Flag, double Percent) : IManualPoolTierAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualPoolTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapManualPoolTierAdjuster(double Cap) : IManualPoolTierAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualPoolTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentManualPoolTierAdjuster(double Percent) : IManualPoolTierAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualPoolTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingManualPoolTierAdjuster(double Step) : IManualPoolTierAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualPoolTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SegmentManualPoolTierAdjuster(Dictionary<string, double> Factors) : IManualPoolTierAdjuster
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(ManualPoolTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the manual pool tier steps in order and keeps a trace of each.</summary>
public sealed class ManualPoolTierEngine(List<IManualPoolTierAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualPoolTierAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualPoolTierEngine DefaultManualPoolTierEngine(Registry flags)
    {
        return new ManualPoolTierEngine(
            new List<IManualPoolTierAdjuster>
            {
                new FlaggedManualPoolTierAdjuster(flags, "manual-pool-tier-legacy", -7.5),
                new CapManualPoolTierAdjuster(13544.0),
                new PercentManualPoolTierAdjuster(-16.3),
                new RoundingManualPoolTierAdjuster(0.05),
                new SegmentManualPoolTierAdjuster(new Dictionary<string, double> { ["south"] = 1.14 }),
            },
            new List<string>(),
            2.6,
            43256.7);
    }

    public double Run(ManualPoolTier item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<ManualPoolTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
