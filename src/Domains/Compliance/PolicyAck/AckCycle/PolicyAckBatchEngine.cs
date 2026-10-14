namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy ack batch calculation.</summary>
public interface IPolicyAckBatchStep
{
    string Describe();
    double Adjust(PolicyAckBatch item, double valueValue);
}

public sealed record RoundingPolicyAckBatchAdjuster(double Step) : IPolicyAckBatchStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyAckBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NamePolicyAckBatchAdjuster(Dictionary<string, double> Factors) : IPolicyAckBatchStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PolicyAckBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPolicyAckBatchAdjuster(double Minimum) : IPolicyAckBatchStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyAckBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPolicyAckBatchAdjuster(double Cap) : IPolicyAckBatchStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyAckBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPolicyAckBatchAdjuster(List<double> Bounds, List<double> Rates) : IPolicyAckBatchStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyAckBatch item, double valueValue)
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

/// <summary>Runs the policy ack batch steps in order and keeps a trace of each.</summary>
public sealed class PolicyAckBatchEngine(List<IPolicyAckBatchStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAckBatchStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAckBatchEngine DefaultPolicyAckBatchEngine()
    {
        return new PolicyAckBatchEngine(
            new List<IPolicyAckBatchStep>
            {
                new RoundingPolicyAckBatchAdjuster(0.25),
                new NamePolicyAckBatchAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.98, ["west"] = 1.03, ["north"] = 1.23 }),
                new MinimumPolicyAckBatchAdjuster(4.9),
                new CapPolicyAckBatchAdjuster(36235.0),
                new TieredPolicyAckBatchAdjuster(
                    new List<double> { 4354.0, 4458.0, 4922.0, 7952.0 },
                    new List<double> { 1.19, 1.11, 0.96, 1.04 }),
            },
            new List<string>(),
            1.8,
            24940.8);
    }

    public double Price(PolicyAckBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAckBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
