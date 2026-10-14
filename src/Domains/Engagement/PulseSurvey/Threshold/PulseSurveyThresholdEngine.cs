namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pulse survey threshold calculation.</summary>
public interface IPulseSurveyThresholdFactor
{
    string Describe();
    double Adjust(PulseSurveyThreshold item, double valueValue);
}

public sealed record MinimumPulseSurveyThresholdAdjuster(double Minimum) : IPulseSurveyThresholdFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PulseSurveyThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPulseSurveyThresholdAdjuster(List<double> Bounds, List<double> Rates) : IPulseSurveyThresholdFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PulseSurveyThreshold item, double valueValue)
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

public sealed record RoundingPulseSurveyThresholdAdjuster(double Step) : IPulseSurveyThresholdFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PulseSurveyThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PulseSurveyThresholdThresholdOffsetAdjuster(double Weight) : IPulseSurveyThresholdFactor
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(PulseSurveyThreshold item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record LabelPulseSurveyThresholdAdjuster(Dictionary<string, double> Factors) : IPulseSurveyThresholdFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PulseSurveyThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPulseSurveyThresholdAdjuster(double Cap) : IPulseSurveyThresholdFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PulseSurveyThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the pulse survey threshold steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveyThresholdEngine(List<IPulseSurveyThresholdFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveyThresholdFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveyThresholdEngine DefaultPulseSurveyThresholdEngine()
    {
        return new PulseSurveyThresholdEngine(
            new List<IPulseSurveyThresholdFactor>
            {
                new MinimumPulseSurveyThresholdAdjuster(38.4),
                new TieredPulseSurveyThresholdAdjuster(
                    new List<double> { 1174.0, 2382.0, 7487.0, 9231.0 },
                    new List<double> { 0.86, 1.02, 0.98, 0.92 }),
                new RoundingPulseSurveyThresholdAdjuster(5.0),
                new PulseSurveyThresholdThresholdOffsetAdjuster(0.51),
                new LabelPulseSurveyThresholdAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.82, ["north"] = 1.2, ["retail"] = 0.9 }),
                new CapPulseSurveyThresholdAdjuster(49017.0),
            },
            new List<string>(),
            3.9,
            74989.9);
    }

    public double Price(PulseSurveyThreshold item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveyThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
