namespace Acme.Workforce.Engagement.PulseSurvey.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pulse survey metric calculation.</summary>
public interface IPulseSurveyMetricModifier
{
    string Describe();
    double Adjust(PulseSurveyMetric item, double valueValue);
}

public sealed record TieredPulseSurveyMetricAdjuster(List<double> Bounds, List<double> Rates) : IPulseSurveyMetricModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PulseSurveyMetric item, double valueValue)
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

public sealed record RoundingPulseSurveyMetricAdjuster(double Step) : IPulseSurveyMetricModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PulseSurveyMetric item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapPulseSurveyMetricAdjuster(double Cap) : IPulseSurveyMetricModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PulseSurveyMetric item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPulseSurveyMetricAdjuster(double Percent) : IPulseSurveyMetricModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PulseSurveyMetric item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentPulseSurveyMetricAdjuster(Dictionary<string, double> Factors) : IPulseSurveyMetricModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(PulseSurveyMetric item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pulse survey metric steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveyMetricEngine(List<IPulseSurveyMetricModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveyMetricModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveyMetricEngine DefaultPulseSurveyMetricEngine()
    {
        return new PulseSurveyMetricEngine(
            new List<IPulseSurveyMetricModifier>
            {
                new TieredPulseSurveyMetricAdjuster(
                    new List<double> { 2183.0, 3856.0, 6357.0, 7938.0 },
                    new List<double> { 0.96, 0.92, 0.93, 1.07 }),
                new RoundingPulseSurveyMetricAdjuster(0.05),
                new CapPulseSurveyMetricAdjuster(49827.0),
                new PercentPulseSurveyMetricAdjuster(11.2),
                new DepartmentPulseSurveyMetricAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.12, ["north"] = 1.03, ["online"] = 1.25 }),
            },
            new List<string>(),
            3.2,
            9326.7);
    }

    public double Evaluate(PulseSurveyMetric item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveyMetric> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
