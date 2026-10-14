namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pending reorg variance calculation.</summary>
public interface IPendingReorgVarianceAdjuster
{
    string Describe();
    double Adjust(PendingReorgVariance item, double valueValue);
}

public sealed record NamePendingReorgVarianceAdjuster(Dictionary<string, double> Factors) : IPendingReorgVarianceAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PendingReorgVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PendingReorgVarianceThresholdOffsetAdjuster(double Weight) : IPendingReorgVarianceAdjuster
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(PendingReorgVariance item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record FlaggedPendingReorgVarianceAdjuster(Registry Flags, string Flag, double Percent) : IPendingReorgVarianceAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingReorgVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPendingReorgVarianceAdjuster(List<double> Bounds, List<double> Rates) : IPendingReorgVarianceAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingReorgVariance item, double valueValue)
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

public sealed record PercentPendingReorgVarianceAdjuster(double Percent) : IPendingReorgVarianceAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingReorgVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPendingReorgVarianceAdjuster(double Minimum) : IPendingReorgVarianceAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingReorgVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pending reorg variance steps in order and keeps a trace of each.</summary>
public sealed class PendingReorgVarianceEngine(List<IPendingReorgVarianceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingReorgVarianceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingReorgVarianceEngine DefaultPendingReorgVarianceEngine(Registry flags)
    {
        return new PendingReorgVarianceEngine(
            new List<IPendingReorgVarianceAdjuster>
            {
                new NamePendingReorgVarianceAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.93, ["retail"] = 1.12, ["online"] = 1.35 }),
                new PendingReorgVarianceThresholdOffsetAdjuster(1.28),
                new FlaggedPendingReorgVarianceAdjuster(flags, "pending-reorg-variance-pending", 0.2),
                new TieredPendingReorgVarianceAdjuster(
                    new List<double> { 701.0, 6802.0 },
                    new List<double> { 0.86, 1.05 }),
                new PercentPendingReorgVarianceAdjuster(22.3),
                new MinimumPendingReorgVarianceAdjuster(44.4),
            },
            new List<string>(),
            2.4,
            90899.7);
    }

    public double Compute(PendingReorgVariance item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PendingReorgVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
