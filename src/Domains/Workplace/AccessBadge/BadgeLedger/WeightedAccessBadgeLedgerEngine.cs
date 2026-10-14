namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the weighted access badge ledger calculation.</summary>
public interface IWeightedAccessBadgeLedgerStep
{
    string Describe();
    double Adjust(WeightedAccessBadgeLedger item, double valueValue);
}

public sealed record FlaggedWeightedAccessBadgeLedgerAdjuster(Registry Flags, string Flag, double Percent) : IWeightedAccessBadgeLedgerStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedAccessBadgeLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumWeightedAccessBadgeLedgerAdjuster(double Minimum) : IWeightedAccessBadgeLedgerStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedAccessBadgeLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameWeightedAccessBadgeLedgerAdjuster(Dictionary<string, double> Factors) : IWeightedAccessBadgeLedgerStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(WeightedAccessBadgeLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the weighted access badge ledger steps in order and keeps a trace of each.</summary>
public sealed class WeightedAccessBadgeLedgerEngine(List<IWeightedAccessBadgeLedgerStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedAccessBadgeLedgerStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedAccessBadgeLedgerEngine DefaultWeightedAccessBadgeLedgerEngine(Registry flags)
    {
        return new WeightedAccessBadgeLedgerEngine(
            new List<IWeightedAccessBadgeLedgerStep>
            {
                new FlaggedWeightedAccessBadgeLedgerAdjuster(flags, "weighted-access-badge-ledger-tiered", 4.3),
                new MinimumWeightedAccessBadgeLedgerAdjuster(33.1),
                new NameWeightedAccessBadgeLedgerAdjuster(new Dictionary<string, double> { ["north"] = 0.86 }),
            },
            new List<string>(),
            0.4,
            18212.3);
    }

    public double Compute(WeightedAccessBadgeLedger item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedAccessBadgeLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
