namespace Acme.Workforce.Compliance.Attestation.Rule;

/// <summary>One step of the attestation score calculation.</summary>
public interface IAttestationScoreModifier
{
    string Describe();
    double Adjust(AttestationScore item, double valueValue);
}

public sealed record AttestationScoreBatchSizeOffsetAdjuster(double Weight) : IAttestationScoreModifier
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(AttestationScore item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record TieredAttestationScoreAdjuster(List<double> Bounds, List<double> Rates) : IAttestationScoreModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AttestationScore item, double valueValue)
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

public sealed record MinimumAttestationScoreAdjuster(double Minimum) : IAttestationScoreModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the attestation score steps in order and keeps a trace of each.</summary>
public sealed class AttestationScoreEngine(List<IAttestationScoreModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationScoreModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationScoreEngine DefaultAttestationScoreEngine()
    {
        return new AttestationScoreEngine(
            new List<IAttestationScoreModifier>
            {
                new AttestationScoreBatchSizeOffsetAdjuster(1.87),
                new TieredAttestationScoreAdjuster(
                    new List<double> { 2381.0, 8976.0 },
                    new List<double> { 1.12, 1.01 }),
                new MinimumAttestationScoreAdjuster(5.0),
            },
            new List<string>(),
            0.5,
            32480.4);
    }

    public double Run(AttestationScore item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
