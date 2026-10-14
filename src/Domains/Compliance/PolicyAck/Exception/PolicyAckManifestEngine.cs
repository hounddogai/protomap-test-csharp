namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy ack manifest calculation.</summary>
public interface IPolicyAckManifestFactor
{
    string Describe();
    double Adjust(PolicyAckManifest item, double valueValue);
}

public sealed record PolicyAckManifestWindowDaysOffsetAdjuster(double Weight) : IPolicyAckManifestFactor
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(PolicyAckManifest item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record LocalePolicyAckManifestAdjuster(Dictionary<string, double> Factors) : IPolicyAckManifestFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(PolicyAckManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPolicyAckManifestAdjuster(double Percent) : IPolicyAckManifestFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyAckManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPolicyAckManifestAdjuster(double Cap) : IPolicyAckManifestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyAckManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPolicyAckManifestAdjuster(List<double> Bounds, List<double> Rates) : IPolicyAckManifestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyAckManifest item, double valueValue)
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

/// <summary>Runs the policy ack manifest steps in order and keeps a trace of each.</summary>
public sealed class PolicyAckManifestEngine(List<IPolicyAckManifestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAckManifestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAckManifestEngine DefaultPolicyAckManifestEngine()
    {
        return new PolicyAckManifestEngine(
            new List<IPolicyAckManifestFactor>
            {
                new PolicyAckManifestWindowDaysOffsetAdjuster(1.1),
                new LocalePolicyAckManifestAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.33, ["north"] = 0.76 }),
                new PercentPolicyAckManifestAdjuster(20.9),
                new CapPolicyAckManifestAdjuster(6899.0),
                new TieredPolicyAckManifestAdjuster(
                    new List<double> { 1777.0, 4352.0, 7268.0, 8997.0 },
                    new List<double> { 0.94, 0.91, 0.88, 0.82 }),
            },
            new List<string>(),
            1.9,
            83721.4);
    }

    public double Run(PolicyAckManifest item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAckManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
