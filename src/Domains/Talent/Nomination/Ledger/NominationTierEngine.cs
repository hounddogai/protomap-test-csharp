namespace Acme.Workforce.Talent.Nomination.Ledger;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the nomination tier calculation.</summary>
public interface INominationTierFactor
{
    string Describe();
    double Adjust(NominationTier item, double valueValue);
}

public sealed record NominationTierCostOffsetAdjuster(double Weight) : INominationTierFactor
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(NominationTier item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record TieredNominationTierAdjuster(List<double> Bounds, List<double> Rates) : INominationTierFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(NominationTier item, double valueValue)
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

public sealed record FlaggedNominationTierAdjuster(Registry Flags, string Flag, double Percent) : INominationTierFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(NominationTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelNominationTierAdjuster(Dictionary<string, double> Factors) : INominationTierFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(NominationTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the nomination tier steps in order and keeps a trace of each.</summary>
public sealed class NominationTierEngine(List<INominationTierFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationTierFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationTierEngine DefaultNominationTierEngine(Registry flags)
    {
        return new NominationTierEngine(
            new List<INominationTierFactor>
            {
                new NominationTierCostOffsetAdjuster(0.62),
                new TieredNominationTierAdjuster(
                    new List<double> { 845.0, 3657.0, 5174.0, 5538.0 },
                    new List<double> { 0.98, 0.94, 0.81, 1.19 }),
                new FlaggedNominationTierAdjuster(flags, "nomination-tier-tiered", 5.7),
                new LabelNominationTierAdjuster(new Dictionary<string, double> { ["west"] = 0.72, ["east"] = 0.9 }),
            },
            new List<string>(),
            2.2,
            70417.6);
    }

    public double Compute(NominationTier item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<NominationTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
