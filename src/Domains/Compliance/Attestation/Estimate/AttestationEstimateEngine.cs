namespace Acme.Workforce.Compliance.Attestation.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation estimate calculation.</summary>
public interface IAttestationEstimateFactor
{
    string Describe();
    double Adjust(AttestationEstimate item, double valueValue);
}

public sealed record MinimumAttestationEstimateAdjuster(double Minimum) : IAttestationEstimateFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AttestationEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingAttestationEstimateAdjuster(double Step) : IAttestationEstimateFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AttestationEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record AttestationEstimateExpiresAtOffsetAdjuster(double Weight) : IAttestationEstimateFactor
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(AttestationEstimate item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record PercentAttestationEstimateAdjuster(double Percent) : IAttestationEstimateFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AttestationEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAttestationEstimateAdjuster(List<double> Bounds, List<double> Rates) : IAttestationEstimateFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AttestationEstimate item, double valueValue)
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

public sealed record CapAttestationEstimateAdjuster(double Cap) : IAttestationEstimateFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AttestationEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the attestation estimate steps in order and keeps a trace of each.</summary>
public sealed class AttestationEstimateEngine(List<IAttestationEstimateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationEstimateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationEstimateEngine DefaultAttestationEstimateEngine()
    {
        return new AttestationEstimateEngine(
            new List<IAttestationEstimateFactor>
            {
                new MinimumAttestationEstimateAdjuster(16.3),
                new RoundingAttestationEstimateAdjuster(0.5),
                new AttestationEstimateExpiresAtOffsetAdjuster(0.08),
                new PercentAttestationEstimateAdjuster(6.8),
                new TieredAttestationEstimateAdjuster(
                    new List<double> { 582.0, 2738.0, 3127.0 },
                    new List<double> { 1.0, 1.07, 0.82 }),
                new CapAttestationEstimateAdjuster(14483.0),
            },
            new List<string>(),
            0.8,
            78682.5);
    }

    public double Price(AttestationEstimate item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
