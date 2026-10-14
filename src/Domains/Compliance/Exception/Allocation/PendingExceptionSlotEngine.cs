namespace Acme.Workforce.Compliance.Exception.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending exception slot calculation.</summary>
public interface IPendingExceptionSlotAdjuster
{
    string Describe();
    double Adjust(PendingExceptionSlot item, double valueValue);
}

public sealed record RoundingPendingExceptionSlotAdjuster(double Step) : IPendingExceptionSlotAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingExceptionSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedPendingExceptionSlotAdjuster(Registry Flags, string Flag, double Percent) : IPendingExceptionSlotAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingExceptionSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPendingExceptionSlotAdjuster(double Minimum) : IPendingExceptionSlotAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingExceptionSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TeamPendingExceptionSlotAdjuster(Dictionary<string, double> Factors) : IPendingExceptionSlotAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(PendingExceptionSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPendingExceptionSlotAdjuster(double Percent) : IPendingExceptionSlotAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingExceptionSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPendingExceptionSlotAdjuster(List<double> Bounds, List<double> Rates) : IPendingExceptionSlotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingExceptionSlot item, double valueValue)
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

/// <summary>Runs the pending exception slot steps in order and keeps a trace of each.</summary>
public sealed class PendingExceptionSlotEngine(List<IPendingExceptionSlotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingExceptionSlotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingExceptionSlotEngine DefaultPendingExceptionSlotEngine(Registry flags)
    {
        return new PendingExceptionSlotEngine(
            new List<IPendingExceptionSlotAdjuster>
            {
                new RoundingPendingExceptionSlotAdjuster(0.1),
                new FlaggedPendingExceptionSlotAdjuster(flags, "pending-exception-slot-tiered", -0.1),
                new MinimumPendingExceptionSlotAdjuster(31.6),
                new TeamPendingExceptionSlotAdjuster(new Dictionary<string, double> { ["retail"] = 0.88 }),
                new PercentPendingExceptionSlotAdjuster(-1.8),
                new TieredPendingExceptionSlotAdjuster(
                    new List<double> { 4663.0, 5620.0, 7075.0, 9216.0 },
                    new List<double> { 0.8, 0.86, 0.88, 1.13 }),
            },
            new List<string>(),
            0.6,
            60826.5);
    }

    public double Compute(PendingExceptionSlot item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PendingExceptionSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
