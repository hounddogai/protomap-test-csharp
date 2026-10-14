namespace Acme.Workforce.Orgdesign.Reorg.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending reorg entry calculation.</summary>
public interface IPendingReorgEntryModifier
{
    string Describe();
    double Adjust(PendingReorgEntry item, double valueValue);
}

public sealed record TieredPendingReorgEntryAdjuster(List<double> Bounds, List<double> Rates) : IPendingReorgEntryModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingReorgEntry item, double valueValue)
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

public sealed record MinimumPendingReorgEntryAdjuster(double Minimum) : IPendingReorgEntryModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingReorgEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPendingReorgEntryAdjuster(double Cap) : IPendingReorgEntryModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingReorgEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPendingReorgEntryAdjuster(double Percent) : IPendingReorgEntryModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingReorgEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TeamPendingReorgEntryAdjuster(Dictionary<string, double> Factors) : IPendingReorgEntryModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(PendingReorgEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PendingReorgEntryVersionOffsetAdjuster(double Weight) : IPendingReorgEntryModifier
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(PendingReorgEntry item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

/// <summary>Runs the pending reorg entry steps in order and keeps a trace of each.</summary>
public sealed class PendingReorgEntryEngine(List<IPendingReorgEntryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingReorgEntryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingReorgEntryEngine DefaultPendingReorgEntryEngine()
    {
        return new PendingReorgEntryEngine(
            new List<IPendingReorgEntryModifier>
            {
                new TieredPendingReorgEntryAdjuster(
                    new List<double> { 3100.0, 5151.0, 6376.0 },
                    new List<double> { 0.95, 1.11, 0.89 }),
                new MinimumPendingReorgEntryAdjuster(41.0),
                new CapPendingReorgEntryAdjuster(22316.0),
                new PercentPendingReorgEntryAdjuster(14.1),
                new TeamPendingReorgEntryAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.07, ["east"] = 1.28 }),
                new PendingReorgEntryVersionOffsetAdjuster(0.42),
            },
            new List<string>(),
            1.4,
            67454.5);
    }

    public double Compute(PendingReorgEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingReorgEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
