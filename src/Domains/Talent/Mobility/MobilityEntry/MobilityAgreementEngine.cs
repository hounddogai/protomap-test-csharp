namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the mobility agreement calculation.</summary>
public interface IMobilityAgreementAdjuster
{
    string Describe();
    double Adjust(MobilityAgreement item, double valueValue);
}

public sealed record RoundingMobilityAgreementAdjuster(double Step) : IMobilityAgreementAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(MobilityAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentMobilityAgreementAdjuster(double Percent) : IMobilityAgreementAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MobilityAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapMobilityAgreementAdjuster(double Cap) : IMobilityAgreementAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(MobilityAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumMobilityAgreementAdjuster(double Minimum) : IMobilityAgreementAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MobilityAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SegmentMobilityAgreementAdjuster(Dictionary<string, double> Factors) : IMobilityAgreementAdjuster
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(MobilityAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredMobilityAgreementAdjuster(List<double> Bounds, List<double> Rates) : IMobilityAgreementAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MobilityAgreement item, double valueValue)
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

/// <summary>Runs the mobility agreement steps in order and keeps a trace of each.</summary>
public sealed class MobilityAgreementEngine(List<IMobilityAgreementAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMobilityAgreementAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MobilityAgreementEngine DefaultMobilityAgreementEngine()
    {
        return new MobilityAgreementEngine(
            new List<IMobilityAgreementAdjuster>
            {
                new RoundingMobilityAgreementAdjuster(0.1),
                new PercentMobilityAgreementAdjuster(15.9),
                new CapMobilityAgreementAdjuster(6069.0),
                new MinimumMobilityAgreementAdjuster(48.8),
                new SegmentMobilityAgreementAdjuster(new Dictionary<string, double> { ["east"] = 0.72 }),
                new TieredMobilityAgreementAdjuster(
                    new List<double> { 141.0, 8783.0 },
                    new List<double> { 1.18, 1.08 }),
            },
            new List<string>(),
            4.8,
            74736.6);
    }

    public double Compute(MobilityAgreement item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<MobilityAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
