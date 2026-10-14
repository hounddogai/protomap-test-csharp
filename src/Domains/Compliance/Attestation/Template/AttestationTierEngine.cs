namespace Acme.Workforce.Compliance.Attestation.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation tier calculation.</summary>
public interface IAttestationTierFactor
{
    string Describe();
    double Adjust(AttestationTier item, double valueValue);
}

public sealed record PercentAttestationTierAdjuster(double Percent) : IAttestationTierFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AttestationTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelAttestationTierAdjuster(Dictionary<string, double> Factors) : IAttestationTierFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AttestationTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumAttestationTierAdjuster(double Minimum) : IAttestationTierFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAttestationTierAdjuster(List<double> Bounds, List<double> Rates) : IAttestationTierFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AttestationTier item, double valueValue)
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

public sealed record CapAttestationTierAdjuster(double Cap) : IAttestationTierFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AttestationTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the attestation tier steps in order and keeps a trace of each.</summary>
public sealed class AttestationTierEngine(List<IAttestationTierFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationTierFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationTierEngine DefaultAttestationTierEngine()
    {
        return new AttestationTierEngine(
            new List<IAttestationTierFactor>
            {
                new PercentAttestationTierAdjuster(3.9),
                new LabelAttestationTierAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.26, ["west"] = 1.32, ["east"] = 1.26 }),
                new MinimumAttestationTierAdjuster(37.6),
                new TieredAttestationTierAdjuster(
                    new List<double> { 4070.0, 4378.0, 6245.0, 6505.0 },
                    new List<double> { 1.14, 1.11, 0.88, 0.99 }),
                new CapAttestationTierAdjuster(40613.0),
            },
            new List<string>(),
            0.5,
            81039.9);
    }

    public double Evaluate(AttestationTier item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
