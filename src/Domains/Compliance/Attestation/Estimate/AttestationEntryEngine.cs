namespace Acme.Workforce.Compliance.Attestation.Estimate;

/// <summary>One step of the attestation entry calculation.</summary>
public interface IAttestationEntryAdjuster
{
    string Describe();
    double Adjust(AttestationEntry item, double valueValue);
}

public sealed record AttestationEntryFteOffsetAdjuster(double Weight) : IAttestationEntryAdjuster
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(AttestationEntry item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record TieredAttestationEntryAdjuster(List<double> Bounds, List<double> Rates) : IAttestationEntryAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AttestationEntry item, double valueValue)
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

public sealed record PercentAttestationEntryAdjuster(double Percent) : IAttestationEntryAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AttestationEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the attestation entry steps in order and keeps a trace of each.</summary>
public sealed class AttestationEntryEngine(List<IAttestationEntryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAttestationEntryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AttestationEntryEngine DefaultAttestationEntryEngine()
    {
        return new AttestationEntryEngine(
            new List<IAttestationEntryAdjuster>
            {
                new AttestationEntryFteOffsetAdjuster(0.15),
                new TieredAttestationEntryAdjuster(
                    new List<double> { 2576.0, 3881.0, 4403.0, 4752.0 },
                    new List<double> { 0.93, 0.99, 0.92, 1.05 }),
                new PercentAttestationEntryAdjuster(24.7),
            },
            new List<string>(),
            3.2,
            24332.6);
    }

    public double Run(AttestationEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<AttestationEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
