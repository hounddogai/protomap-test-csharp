namespace Acme.Workforce.Contractors.Engagement.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted engagement agreement calculation.</summary>
public interface IWeightedEngagementAgreementStep
{
    string Describe();
    double Adjust(WeightedEngagementAgreement item, double valueValue);
}

public sealed record CapWeightedEngagementAgreementAdjuster(double Cap) : IWeightedEngagementAgreementStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedEngagementAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LocaleWeightedEngagementAgreementAdjuster(Dictionary<string, double> Factors) : IWeightedEngagementAgreementStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(WeightedEngagementAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record WeightedEngagementAgreementThresholdOffsetAdjuster(double Weight) : IWeightedEngagementAgreementStep
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(WeightedEngagementAgreement item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record MinimumWeightedEngagementAgreementAdjuster(double Minimum) : IWeightedEngagementAgreementStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedEngagementAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredWeightedEngagementAgreementAdjuster(List<double> Bounds, List<double> Rates) : IWeightedEngagementAgreementStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WeightedEngagementAgreement item, double valueValue)
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

public sealed record RoundingWeightedEngagementAgreementAdjuster(double Step) : IWeightedEngagementAgreementStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedEngagementAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the weighted engagement agreement steps in order and keeps a trace of each.</summary>
public sealed class WeightedEngagementAgreementEngine(List<IWeightedEngagementAgreementStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedEngagementAgreementStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedEngagementAgreementEngine DefaultWeightedEngagementAgreementEngine()
    {
        return new WeightedEngagementAgreementEngine(
            new List<IWeightedEngagementAgreementStep>
            {
                new CapWeightedEngagementAgreementAdjuster(10414.0),
                new LocaleWeightedEngagementAgreementAdjuster(new Dictionary<string, double> { ["retail"] = 1.14 }),
                new WeightedEngagementAgreementThresholdOffsetAdjuster(0.64),
                new MinimumWeightedEngagementAgreementAdjuster(37.4),
                new TieredWeightedEngagementAgreementAdjuster(
                    new List<double> { 570.0, 1468.0, 4638.0 },
                    new List<double> { 0.97, 0.8, 0.97 }),
                new RoundingWeightedEngagementAgreementAdjuster(1.0),
            },
            new List<string>(),
            0.9,
            98756.8);
    }

    public double Run(WeightedEngagementAgreement item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedEngagementAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
