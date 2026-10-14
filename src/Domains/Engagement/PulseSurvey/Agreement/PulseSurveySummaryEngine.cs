namespace Acme.Workforce.Engagement.PulseSurvey.Agreement;

/// <summary>One step of the pulse survey summary calculation.</summary>
public interface IPulseSurveySummaryStep
{
    string Describe();
    double Adjust(PulseSurveySummary item, double valueValue);
}

public sealed record PulseSurveySummaryRatingOffsetAdjuster(double Weight) : IPulseSurveySummaryStep
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(PulseSurveySummary item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record CapPulseSurveySummaryAdjuster(double Cap) : IPulseSurveySummaryStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PulseSurveySummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumPulseSurveySummaryAdjuster(double Minimum) : IPulseSurveySummaryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PulseSurveySummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPulseSurveySummaryAdjuster(List<double> Bounds, List<double> Rates) : IPulseSurveySummaryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PulseSurveySummary item, double valueValue)
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

/// <summary>Runs the pulse survey summary steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveySummaryEngine(List<IPulseSurveySummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveySummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveySummaryEngine DefaultPulseSurveySummaryEngine()
    {
        return new PulseSurveySummaryEngine(
            new List<IPulseSurveySummaryStep>
            {
                new PulseSurveySummaryRatingOffsetAdjuster(1.7),
                new CapPulseSurveySummaryAdjuster(6342.0),
                new MinimumPulseSurveySummaryAdjuster(40.1),
                new TieredPulseSurveySummaryAdjuster(
                    new List<double> { 87.0, 3659.0 },
                    new List<double> { 1.13, 0.84 }),
            },
            new List<string>(),
            1.0,
            10342.9);
    }

    public double Evaluate(PulseSurveySummary item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveySummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
