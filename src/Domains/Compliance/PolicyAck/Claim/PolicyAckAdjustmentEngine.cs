namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy ack adjustment calculation.</summary>
public interface IPolicyAckAdjustmentAdjuster
{
    string Describe();
    double Adjust(PolicyAckAdjustment item, double valueValue);
}

public sealed record MinimumPolicyAckAdjustmentAdjuster(double Minimum) : IPolicyAckAdjustmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyAckAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PolicyAckAdjustmentLimitOffsetAdjuster(double Weight) : IPolicyAckAdjustmentAdjuster
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(PolicyAckAdjustment item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record RoundingPolicyAckAdjustmentAdjuster(double Step) : IPolicyAckAdjustmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyAckAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapPolicyAckAdjustmentAdjuster(double Cap) : IPolicyAckAdjustmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyAckAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPolicyAckAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IPolicyAckAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyAckAdjustment item, double valueValue)
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

/// <summary>Runs the policy ack adjustment steps in order and keeps a trace of each.</summary>
public sealed class PolicyAckAdjustmentEngine(List<IPolicyAckAdjustmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAckAdjustmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAckAdjustmentEngine DefaultPolicyAckAdjustmentEngine()
    {
        return new PolicyAckAdjustmentEngine(
            new List<IPolicyAckAdjustmentAdjuster>
            {
                new MinimumPolicyAckAdjustmentAdjuster(16.3),
                new PolicyAckAdjustmentLimitOffsetAdjuster(0.4),
                new RoundingPolicyAckAdjustmentAdjuster(1.0),
                new CapPolicyAckAdjustmentAdjuster(30835.0),
                new TieredPolicyAckAdjustmentAdjuster(
                    new List<double> { 1469.0, 4801.0, 5665.0, 5923.0 },
                    new List<double> { 1.12, 0.91, 0.95, 0.95 }),
            },
            new List<string>(),
            4.7,
            25175.8);
    }

    public double Compute(PolicyAckAdjustment item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAckAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
