namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the adaptive desk claim calculation.</summary>
public interface IAdaptiveDeskClaimStep
{
    string Describe();
    double Adjust(AdaptiveDeskClaim item, double valueValue);
}

public sealed record FlaggedAdaptiveDeskClaimAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveDeskClaimStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveDeskClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelAdaptiveDeskClaimAdjuster(Dictionary<string, double> Factors) : IAdaptiveDeskClaimStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(AdaptiveDeskClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredAdaptiveDeskClaimAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveDeskClaimStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveDeskClaim item, double valueValue)
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

/// <summary>Runs the adaptive desk claim steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveDeskClaimEngine(List<IAdaptiveDeskClaimStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveDeskClaimStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveDeskClaimEngine DefaultAdaptiveDeskClaimEngine(Registry flags)
    {
        return new AdaptiveDeskClaimEngine(
            new List<IAdaptiveDeskClaimStep>
            {
                new FlaggedAdaptiveDeskClaimAdjuster(flags, "adaptive-desk-claim-lenient", 6.2),
                new ChannelAdaptiveDeskClaimAdjuster(new Dictionary<string, double> { ["south"] = 0.77 }),
                new TieredAdaptiveDeskClaimAdjuster(
                    new List<double> { 557.0, 8245.0, 8315.0 },
                    new List<double> { 1.1, 0.92, 0.93 }),
            },
            new List<string>(),
            2.3,
            86576.7);
    }

    public double Price(AdaptiveDeskClaim item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveDeskClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
