namespace Acme.Workforce.Compliance.Attestation.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the attestation snapshot calculation.</summary>
public interface IAttestationSnapshotModifier
{
    string Describe();
    double Adjust(AttestationSnapshot item, double valueValue);
}

public sealed record TieredAttestationSnapshotAdjuster(List<double> Bounds, List<double> Rates) : IAttestationSnapshotModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AttestationSnapshot item, double valueValue)
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

public sealed record RoundingAttestationSnapshotAdjuster(double Step) : IAttestationSnapshotModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AttestationSnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelAttestationSnapshotAdjuster(Dictionary<string, double> Factors) : IAttestationSnapshotModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AttestationSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentAttestationSnapshotAdjuster(double Percent) : IAttestationSnapshotModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AttestationSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AttestationSnapshotVersionOffsetAdjuster(double Weight) : IAttestationSnapshotModifier
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(AttestationSnapshot item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record CapAttestationSnapshotAdjuster(double Cap) : IAttestationSnapshotModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AttestationSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the attestation snapshot steps in order and keeps a trace of each.</summary>
public sealed class AttestationSnapshotEngine(List<IAttestationSnapshotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationSnapshotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationSnapshotEngine DefaultAttestationSnapshotEngine()
    {
        return new AttestationSnapshotEngine(
            new List<IAttestationSnapshotModifier>
            {
                new TieredAttestationSnapshotAdjuster(
                    new List<double> { 1360.0, 6702.0, 7463.0, 9891.0 },
                    new List<double> { 0.91, 0.82, 1.02, 0.98 }),
                new RoundingAttestationSnapshotAdjuster(0.5),
                new LabelAttestationSnapshotAdjuster(new Dictionary<string, double> { ["north"] = 1.35 }),
                new PercentAttestationSnapshotAdjuster(-9.0),
                new AttestationSnapshotVersionOffsetAdjuster(0.01),
                new CapAttestationSnapshotAdjuster(2864.0),
            },
            new List<string>(),
            3.7,
            73234.5);
    }

    public double Compute(AttestationSnapshot item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AttestationSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
