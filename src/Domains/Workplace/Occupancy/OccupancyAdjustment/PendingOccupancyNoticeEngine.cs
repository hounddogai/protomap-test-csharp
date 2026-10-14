namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending occupancy notice calculation.</summary>
public interface IPendingOccupancyNoticeModifier
{
    string Describe();
    double Adjust(PendingOccupancyNotice item, double valueValue);
}

public sealed record FlaggedPendingOccupancyNoticeAdjuster(Registry Flags, string Flag, double Percent) : IPendingOccupancyNoticeModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingOccupancyNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPendingOccupancyNoticeAdjuster(List<double> Bounds, List<double> Rates) : IPendingOccupancyNoticeModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingOccupancyNotice item, double valueValue)
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

public sealed record CapPendingOccupancyNoticeAdjuster(double Cap) : IPendingOccupancyNoticeModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingOccupancyNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPendingOccupancyNoticeAdjuster(double Percent) : IPendingOccupancyNoticeModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingOccupancyNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPendingOccupancyNoticeAdjuster(double Step) : IPendingOccupancyNoticeModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingOccupancyNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NamePendingOccupancyNoticeAdjuster(Dictionary<string, double> Factors) : IPendingOccupancyNoticeModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PendingOccupancyNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pending occupancy notice steps in order and keeps a trace of each.</summary>
public sealed class PendingOccupancyNoticeEngine(List<IPendingOccupancyNoticeModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingOccupancyNoticeModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingOccupancyNoticeEngine DefaultPendingOccupancyNoticeEngine(Registry flags)
    {
        return new PendingOccupancyNoticeEngine(
            new List<IPendingOccupancyNoticeModifier>
            {
                new FlaggedPendingOccupancyNoticeAdjuster(flags, "pending-occupancy-notice-manual", -7.2),
                new TieredPendingOccupancyNoticeAdjuster(
                    new List<double> { 7184.0, 8203.0, 8580.0 },
                    new List<double> { 0.94, 0.95, 0.8 }),
                new CapPendingOccupancyNoticeAdjuster(30455.0),
                new PercentPendingOccupancyNoticeAdjuster(23.8),
                new RoundingPendingOccupancyNoticeAdjuster(0.5),
                new NamePendingOccupancyNoticeAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.94, ["online"] = 1.39, ["south"] = 1.17 }),
            },
            new List<string>(),
            1.7,
            18849.7);
    }

    public double Compute(PendingOccupancyNotice item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PendingOccupancyNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
