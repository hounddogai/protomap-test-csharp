namespace Acme.Workforce.Compliance.Attestation.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation quota calculation.</summary>
public interface IAttestationQuotaModifier
{
    string Describe();
    double Adjust(AttestationQuota item, double valueValue);
}

public sealed record CapAttestationQuotaAdjuster(double Cap) : IAttestationQuotaModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AttestationQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SourceAttestationQuotaAdjuster(Dictionary<string, double> Factors) : IAttestationQuotaModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(AttestationQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumAttestationQuotaAdjuster(double Minimum) : IAttestationQuotaModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record AttestationQuotaMinimumOffsetAdjuster(double Weight) : IAttestationQuotaModifier
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(AttestationQuota item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record TieredAttestationQuotaAdjuster(List<double> Bounds, List<double> Rates) : IAttestationQuotaModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AttestationQuota item, double valueValue)
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

public sealed record RoundingAttestationQuotaAdjuster(double Step) : IAttestationQuotaModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AttestationQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the attestation quota steps in order and keeps a trace of each.</summary>
public sealed class AttestationQuotaEngine(List<IAttestationQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationQuotaEngine DefaultAttestationQuotaEngine()
    {
        return new AttestationQuotaEngine(
            new List<IAttestationQuotaModifier>
            {
                new CapAttestationQuotaAdjuster(5850.0),
                new SourceAttestationQuotaAdjuster(new Dictionary<string, double> { ["north"] = 1.21 }),
                new MinimumAttestationQuotaAdjuster(12.1),
                new AttestationQuotaMinimumOffsetAdjuster(0.16),
                new TieredAttestationQuotaAdjuster(
                    new List<double> { 2106.0, 6769.0, 9113.0 },
                    new List<double> { 1.1, 0.91, 1.1 }),
                new RoundingAttestationQuotaAdjuster(1.0),
            },
            new List<string>(),
            2.1,
            89528.3);
    }

    public double Compute(AttestationQuota item)
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

    public Dictionary<string, double> EvaluateAll(List<AttestationQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
