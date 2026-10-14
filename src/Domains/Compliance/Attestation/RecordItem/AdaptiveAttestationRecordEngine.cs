namespace Acme.Workforce.Compliance.Attestation.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive attestation record calculation.</summary>
public interface IAdaptiveAttestationRecordFactor
{
    string Describe();
    double Adjust(AdaptiveAttestationRecord item, double valueValue);
}

public sealed record RoundingAdaptiveAttestationRecordAdjuster(double Step) : IAdaptiveAttestationRecordFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveAttestationRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentAdaptiveAttestationRecordAdjuster(double Percent) : IAdaptiveAttestationRecordFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveAttestationRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdaptiveAttestationRecordAdjuster(double Minimum) : IAdaptiveAttestationRecordFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveAttestationRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAdaptiveAttestationRecordAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveAttestationRecordFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveAttestationRecord item, double valueValue)
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

public sealed record AdaptiveAttestationRecordRatingOffsetAdjuster(double Weight) : IAdaptiveAttestationRecordFactor
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(AdaptiveAttestationRecord item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

/// <summary>Runs the adaptive attestation record steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveAttestationRecordEngine(List<IAdaptiveAttestationRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveAttestationRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveAttestationRecordEngine DefaultAdaptiveAttestationRecordEngine()
    {
        return new AdaptiveAttestationRecordEngine(
            new List<IAdaptiveAttestationRecordFactor>
            {
                new RoundingAdaptiveAttestationRecordAdjuster(0.25),
                new PercentAdaptiveAttestationRecordAdjuster(8.2),
                new MinimumAdaptiveAttestationRecordAdjuster(6.8),
                new TieredAdaptiveAttestationRecordAdjuster(
                    new List<double> { 5141.0, 9337.0 },
                    new List<double> { 0.94, 0.97 }),
                new AdaptiveAttestationRecordRatingOffsetAdjuster(0.06),
            },
            new List<string>(),
            4.0,
            76203.6);
    }

    public double Evaluate(AdaptiveAttestationRecord item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveAttestationRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
