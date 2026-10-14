namespace Acme.Workforce.Talent.Pool.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending pool slot calculation.</summary>
public interface IPendingPoolSlotStep
{
    string Describe();
    double Adjust(PendingPoolSlot item, double valueValue);
}

public sealed record MinimumPendingPoolSlotAdjuster(double Minimum) : IPendingPoolSlotStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingPoolSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPendingPoolSlotAdjuster(List<double> Bounds, List<double> Rates) : IPendingPoolSlotStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingPoolSlot item, double valueValue)
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

public sealed record ChannelPendingPoolSlotAdjuster(Dictionary<string, double> Factors) : IPendingPoolSlotStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(PendingPoolSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPendingPoolSlotAdjuster(double Percent) : IPendingPoolSlotStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingPoolSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending pool slot steps in order and keeps a trace of each.</summary>
public sealed class PendingPoolSlotEngine(List<IPendingPoolSlotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingPoolSlotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingPoolSlotEngine DefaultPendingPoolSlotEngine()
    {
        return new PendingPoolSlotEngine(
            new List<IPendingPoolSlotStep>
            {
                new MinimumPendingPoolSlotAdjuster(3.6),
                new TieredPendingPoolSlotAdjuster(
                    new List<double> { 1298.0, 4707.0, 8269.0 },
                    new List<double> { 0.92, 0.93, 0.92 }),
                new ChannelPendingPoolSlotAdjuster(new Dictionary<string, double> { ["south"] = 1.25 }),
                new PercentPendingPoolSlotAdjuster(-3.0),
            },
            new List<string>(),
            4.0,
            85568.0);
    }

    public double Run(PendingPoolSlot item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PendingPoolSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
