namespace Acme.Workforce.Compliance.PolicyAck.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy ack draft calculation.</summary>
public interface IPolicyAckDraftAdjuster
{
    string Describe();
    double Adjust(PolicyAckDraft item, double valueValue);
}

public sealed record PercentPolicyAckDraftAdjuster(double Percent) : IPolicyAckDraftAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyAckDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPolicyAckDraftAdjuster(List<double> Bounds, List<double> Rates) : IPolicyAckDraftAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyAckDraft item, double valueValue)
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

public sealed record PolicyAckDraftDurationMinutesOffsetAdjuster(double Weight) : IPolicyAckDraftAdjuster
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(PolicyAckDraft item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record CapPolicyAckDraftAdjuster(double Cap) : IPolicyAckDraftAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyAckDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the policy ack draft steps in order and keeps a trace of each.</summary>
public sealed class PolicyAckDraftEngine(List<IPolicyAckDraftAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAckDraftAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAckDraftEngine DefaultPolicyAckDraftEngine()
    {
        return new PolicyAckDraftEngine(
            new List<IPolicyAckDraftAdjuster>
            {
                new PercentPolicyAckDraftAdjuster(12.8),
                new TieredPolicyAckDraftAdjuster(
                    new List<double> { 2603.0, 2660.0, 8216.0, 8474.0 },
                    new List<double> { 0.96, 1.1, 1.09, 1.05 }),
                new PolicyAckDraftDurationMinutesOffsetAdjuster(1.61),
                new CapPolicyAckDraftAdjuster(26359.0),
            },
            new List<string>(),
            3.9,
            17283.5);
    }

    public double Run(PolicyAckDraft item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAckDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
