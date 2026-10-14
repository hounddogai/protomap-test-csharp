namespace Acme.Workforce.Compliance.PolicyAck.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict policy ack claim calculation.</summary>
public interface IStrictPolicyAckClaimFactor
{
    string Describe();
    double Adjust(StrictPolicyAckClaim item, double valueValue);
}

public sealed record PercentStrictPolicyAckClaimAdjuster(double Percent) : IStrictPolicyAckClaimFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictPolicyAckClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingStrictPolicyAckClaimAdjuster(double Step) : IStrictPolicyAckClaimFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictPolicyAckClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumStrictPolicyAckClaimAdjuster(double Minimum) : IStrictPolicyAckClaimFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictPolicyAckClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ChannelStrictPolicyAckClaimAdjuster(Dictionary<string, double> Factors) : IStrictPolicyAckClaimFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(StrictPolicyAckClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredStrictPolicyAckClaimAdjuster(List<double> Bounds, List<double> Rates) : IStrictPolicyAckClaimFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictPolicyAckClaim item, double valueValue)
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

/// <summary>Runs the strict policy ack claim steps in order and keeps a trace of each.</summary>
public sealed class StrictPolicyAckClaimEngine(List<IStrictPolicyAckClaimFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictPolicyAckClaimFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictPolicyAckClaimEngine DefaultStrictPolicyAckClaimEngine()
    {
        return new StrictPolicyAckClaimEngine(
            new List<IStrictPolicyAckClaimFactor>
            {
                new PercentStrictPolicyAckClaimAdjuster(-3.3),
                new RoundingStrictPolicyAckClaimAdjuster(0.05),
                new MinimumStrictPolicyAckClaimAdjuster(2.2),
                new ChannelStrictPolicyAckClaimAdjuster(new Dictionary<string, double> { ["online"] = 0.82 }),
                new TieredStrictPolicyAckClaimAdjuster(
                    new List<double> { 4691.0, 7243.0 },
                    new List<double> { 0.96, 1.01 }),
            },
            new List<string>(),
            0.9,
            15260.0);
    }

    public double Run(StrictPolicyAckClaim item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<StrictPolicyAckClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
