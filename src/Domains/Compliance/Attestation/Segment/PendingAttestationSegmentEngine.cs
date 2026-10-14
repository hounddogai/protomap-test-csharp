namespace Acme.Workforce.Compliance.Attestation.Segment;

/// <summary>One step of the pending attestation segment calculation.</summary>
public interface IPendingAttestationSegmentModifier
{
    string Describe();
    double Adjust(PendingAttestationSegment item, double valueValue);
}

public sealed record CapPendingAttestationSegmentAdjuster(double Cap) : IPendingAttestationSegmentModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingAttestationSegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPendingAttestationSegmentAdjuster(List<double> Bounds, List<double> Rates) : IPendingAttestationSegmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingAttestationSegment item, double valueValue)
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

public sealed record PercentPendingAttestationSegmentAdjuster(double Percent) : IPendingAttestationSegmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingAttestationSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending attestation segment steps in order and keeps a trace of each.</summary>
public sealed class PendingAttestationSegmentEngine(List<IPendingAttestationSegmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingAttestationSegmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingAttestationSegmentEngine DefaultPendingAttestationSegmentEngine()
    {
        return new PendingAttestationSegmentEngine(
            new List<IPendingAttestationSegmentModifier>
            {
                new CapPendingAttestationSegmentAdjuster(39585.0),
                new TieredPendingAttestationSegmentAdjuster(
                    new List<double> { 2286.0, 3755.0, 4158.0, 7625.0 },
                    new List<double> { 1.0, 1.14, 0.85, 0.86 }),
                new PercentPendingAttestationSegmentAdjuster(-23.3),
            },
            new List<string>(),
            1.6,
            58778.1);
    }

    public double Compute(PendingAttestationSegment item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingAttestationSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
