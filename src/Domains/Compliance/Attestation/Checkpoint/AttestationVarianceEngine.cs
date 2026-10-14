namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation variance calculation.</summary>
public interface IAttestationVarianceStep
{
    string Describe();
    double Adjust(AttestationVariance item, double valueValue);
}

public sealed record AttestationVarianceCostOffsetAdjuster(double Weight) : IAttestationVarianceStep
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(AttestationVariance item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record RoundingAttestationVarianceAdjuster(double Step) : IAttestationVarianceStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AttestationVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumAttestationVarianceAdjuster(double Minimum) : IAttestationVarianceStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the attestation variance steps in order and keeps a trace of each.</summary>
public sealed class AttestationVarianceEngine(List<IAttestationVarianceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationVarianceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationVarianceEngine DefaultAttestationVarianceEngine()
    {
        return new AttestationVarianceEngine(
            new List<IAttestationVarianceStep>
            {
                new AttestationVarianceCostOffsetAdjuster(0.08),
                new RoundingAttestationVarianceAdjuster(1.0),
                new MinimumAttestationVarianceAdjuster(17.8),
            },
            new List<string>(),
            3.4,
            11191.7);
    }

    public double Evaluate(AttestationVariance item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
