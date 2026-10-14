namespace Acme.Workforce.Compliance.Attestation.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped attestation audit calculation.</summary>
public interface ICappedAttestationAuditFactor
{
    string Describe();
    double Adjust(CappedAttestationAudit item, double valueValue);
}

public sealed record LabelCappedAttestationAuditAdjuster(Dictionary<string, double> Factors) : ICappedAttestationAuditFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(CappedAttestationAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingCappedAttestationAuditAdjuster(double Step) : ICappedAttestationAuditFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedAttestationAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredCappedAttestationAuditAdjuster(List<double> Bounds, List<double> Rates) : ICappedAttestationAuditFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedAttestationAudit item, double valueValue)
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

public sealed record CapCappedAttestationAuditAdjuster(double Cap) : ICappedAttestationAuditFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedAttestationAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CappedAttestationAuditThresholdOffsetAdjuster(double Weight) : ICappedAttestationAuditFactor
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(CappedAttestationAudit item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

/// <summary>Runs the capped attestation audit steps in order and keeps a trace of each.</summary>
public sealed class CappedAttestationAuditEngine(List<ICappedAttestationAuditFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedAttestationAuditFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedAttestationAuditEngine DefaultCappedAttestationAuditEngine()
    {
        return new CappedAttestationAuditEngine(
            new List<ICappedAttestationAuditFactor>
            {
                new LabelCappedAttestationAuditAdjuster(new Dictionary<string, double> { ["central"] = 1.27 }),
                new RoundingCappedAttestationAuditAdjuster(5.0),
                new TieredCappedAttestationAuditAdjuster(
                    new List<double> { 2389.0, 8735.0 },
                    new List<double> { 1.04, 1.17 }),
                new CapCappedAttestationAuditAdjuster(25191.0),
                new CappedAttestationAuditThresholdOffsetAdjuster(1.0),
            },
            new List<string>(),
            1.4,
            32659.1);
    }

    public double Compute(CappedAttestationAudit item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<CappedAttestationAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
