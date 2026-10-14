namespace Acme.Workforce.Compliance.Attestation.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation milestone calculation.</summary>
public interface IAttestationMilestoneModifier
{
    string Describe();
    double Adjust(AttestationMilestone item, double valueValue);
}

public sealed record AttestationMilestoneRevisionOffsetAdjuster(double Weight) : IAttestationMilestoneModifier
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(AttestationMilestone item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record LabelAttestationMilestoneAdjuster(Dictionary<string, double> Factors) : IAttestationMilestoneModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AttestationMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumAttestationMilestoneAdjuster(double Minimum) : IAttestationMilestoneModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAttestationMilestoneAdjuster(double Cap) : IAttestationMilestoneModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AttestationMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredAttestationMilestoneAdjuster(List<double> Bounds, List<double> Rates) : IAttestationMilestoneModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AttestationMilestone item, double valueValue)
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

public sealed record PercentAttestationMilestoneAdjuster(double Percent) : IAttestationMilestoneModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AttestationMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the attestation milestone steps in order and keeps a trace of each.</summary>
public sealed class AttestationMilestoneEngine(List<IAttestationMilestoneModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationMilestoneModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationMilestoneEngine DefaultAttestationMilestoneEngine()
    {
        return new AttestationMilestoneEngine(
            new List<IAttestationMilestoneModifier>
            {
                new AttestationMilestoneRevisionOffsetAdjuster(1.98),
                new LabelAttestationMilestoneAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.2, ["central"] = 0.94, ["retail"] = 0.97 }),
                new MinimumAttestationMilestoneAdjuster(40.0),
                new CapAttestationMilestoneAdjuster(9241.0),
                new TieredAttestationMilestoneAdjuster(
                    new List<double> { 2256.0, 5064.0 },
                    new List<double> { 0.82, 1.18 }),
                new PercentAttestationMilestoneAdjuster(-6.3),
            },
            new List<string>(),
            2.3,
            46454.0);
    }

    public double Run(AttestationMilestone item)
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

    public Dictionary<string, double> EvaluateAll(List<AttestationMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
