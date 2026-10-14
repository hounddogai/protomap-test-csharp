namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation request calculation.</summary>
public interface IAttestationRequestStep
{
    string Describe();
    double Adjust(AttestationRequest item, double valueValue);
}

public sealed record AttestationRequestMarginOffsetAdjuster(double Weight) : IAttestationRequestStep
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(AttestationRequest item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record RoundingAttestationRequestAdjuster(double Step) : IAttestationRequestStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AttestationRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredAttestationRequestAdjuster(List<double> Bounds, List<double> Rates) : IAttestationRequestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AttestationRequest item, double valueValue)
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

/// <summary>Runs the attestation request steps in order and keeps a trace of each.</summary>
public sealed class AttestationRequestEngine(List<IAttestationRequestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationRequestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationRequestEngine DefaultAttestationRequestEngine()
    {
        return new AttestationRequestEngine(
            new List<IAttestationRequestStep>
            {
                new AttestationRequestMarginOffsetAdjuster(1.07),
                new RoundingAttestationRequestAdjuster(5.0),
                new TieredAttestationRequestAdjuster(
                    new List<double> { 5492.0, 5736.0 },
                    new List<double> { 0.92, 0.88 }),
            },
            new List<string>(),
            0.9,
            63676.6);
    }

    public double Run(AttestationRequest item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
