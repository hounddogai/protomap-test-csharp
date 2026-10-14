namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the primary policy ack mapping calculation.</summary>
public interface IPrimaryPolicyAckMappingFactor
{
    string Describe();
    double Adjust(PrimaryPolicyAckMapping item, double valueValue);
}

public sealed record LocalePrimaryPolicyAckMappingAdjuster(Dictionary<string, double> Factors) : IPrimaryPolicyAckMappingFactor
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(PrimaryPolicyAckMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedPrimaryPolicyAckMappingAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryPolicyAckMappingFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryPolicyAckMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPrimaryPolicyAckMappingAdjuster(double Percent) : IPrimaryPolicyAckMappingFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryPolicyAckMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPrimaryPolicyAckMappingAdjuster(double Cap) : IPrimaryPolicyAckMappingFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryPolicyAckMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the primary policy ack mapping steps in order and keeps a trace of each.</summary>
public sealed class PrimaryPolicyAckMappingEngine(List<IPrimaryPolicyAckMappingFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryPolicyAckMappingFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryPolicyAckMappingEngine DefaultPrimaryPolicyAckMappingEngine(Registry flags)
    {
        return new PrimaryPolicyAckMappingEngine(
            new List<IPrimaryPolicyAckMappingFactor>
            {
                new LocalePrimaryPolicyAckMappingAdjuster(new Dictionary<string, double> { ["west"] = 0.76 }),
                new FlaggedPrimaryPolicyAckMappingAdjuster(flags, "primary-policy-ack-mapping-seasonal", 9.0),
                new PercentPrimaryPolicyAckMappingAdjuster(0.6),
                new CapPrimaryPolicyAckMappingAdjuster(8920.0),
            },
            new List<string>(),
            3.6,
            73587.2);
    }

    public double Run(PrimaryPolicyAckMapping item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryPolicyAckMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
