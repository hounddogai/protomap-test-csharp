namespace Acme.Workforce.Compliance.Attestation.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy attestation entry calculation.</summary>
public interface ILegacyAttestationEntryStep
{
    string Describe();
    double Adjust(LegacyAttestationEntry item, double valueValue);
}

public sealed record PercentLegacyAttestationEntryAdjuster(double Percent) : ILegacyAttestationEntryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyAttestationEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLegacyAttestationEntryAdjuster(List<double> Bounds, List<double> Rates) : ILegacyAttestationEntryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyAttestationEntry item, double valueValue)
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

public sealed record MinimumLegacyAttestationEntryAdjuster(double Minimum) : ILegacyAttestationEntryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyAttestationEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LegacyAttestationEntryHoursOffsetAdjuster(double Weight) : ILegacyAttestationEntryStep
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(LegacyAttestationEntry item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record CapLegacyAttestationEntryAdjuster(double Cap) : ILegacyAttestationEntryStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyAttestationEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the legacy attestation entry steps in order and keeps a trace of each.</summary>
public sealed class LegacyAttestationEntryEngine(List<ILegacyAttestationEntryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyAttestationEntryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyAttestationEntryEngine DefaultLegacyAttestationEntryEngine()
    {
        return new LegacyAttestationEntryEngine(
            new List<ILegacyAttestationEntryStep>
            {
                new PercentLegacyAttestationEntryAdjuster(13.9),
                new TieredLegacyAttestationEntryAdjuster(
                    new List<double> { 1490.0, 1988.0 },
                    new List<double> { 0.82, 0.96 }),
                new MinimumLegacyAttestationEntryAdjuster(27.7),
                new LegacyAttestationEntryHoursOffsetAdjuster(0.56),
                new CapLegacyAttestationEntryAdjuster(36608.0),
            },
            new List<string>(),
            2.3,
            8908.2);
    }

    public double Run(LegacyAttestationEntry item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyAttestationEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
