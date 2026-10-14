namespace Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive policy ack claim calculation.</summary>
public interface IAdaptivePolicyAckClaimModifier
{
    string Describe();
    double Adjust(AdaptivePolicyAckClaim item, double valueValue);
}

public sealed record RoundingAdaptivePolicyAckClaimAdjuster(double Step) : IAdaptivePolicyAckClaimModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptivePolicyAckClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumAdaptivePolicyAckClaimAdjuster(double Minimum) : IAdaptivePolicyAckClaimModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptivePolicyAckClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAdaptivePolicyAckClaimAdjuster(List<double> Bounds, List<double> Rates) : IAdaptivePolicyAckClaimModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptivePolicyAckClaim item, double valueValue)
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

public sealed record SourceAdaptivePolicyAckClaimAdjuster(Dictionary<string, double> Factors) : IAdaptivePolicyAckClaimModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(AdaptivePolicyAckClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedAdaptivePolicyAckClaimAdjuster(Registry Flags, string Flag, double Percent) : IAdaptivePolicyAckClaimModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptivePolicyAckClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adaptive policy ack claim steps in order and keeps a trace of each.</summary>
public sealed class AdaptivePolicyAckClaimEngine(List<IAdaptivePolicyAckClaimModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptivePolicyAckClaimModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptivePolicyAckClaimEngine DefaultAdaptivePolicyAckClaimEngine(Registry flags)
    {
        return new AdaptivePolicyAckClaimEngine(
            new List<IAdaptivePolicyAckClaimModifier>
            {
                new RoundingAdaptivePolicyAckClaimAdjuster(0.25),
                new MinimumAdaptivePolicyAckClaimAdjuster(0.2),
                new TieredAdaptivePolicyAckClaimAdjuster(
                    new List<double> { 248.0, 977.0, 5473.0, 8708.0 },
                    new List<double> { 0.88, 1.02, 1.16, 0.9 }),
                new SourceAdaptivePolicyAckClaimAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.32, ["retail"] = 1.31 }),
                new FlaggedAdaptivePolicyAckClaimAdjuster(flags, "adaptive-policy-ack-claim-primary", -2.9),
            },
            new List<string>(),
            4.5,
            29213.9);
    }

    public double Evaluate(AdaptivePolicyAckClaim item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptivePolicyAckClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
