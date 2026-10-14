namespace Acme.Workforce.Engagement.PulseSurvey.SurveyAudit;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global pulse survey adjustment calculation.</summary>
public interface IGlobalPulseSurveyAdjustmentFactor
{
    string Describe();
    double Adjust(GlobalPulseSurveyAdjustment item, double valueValue);
}

public sealed record CapGlobalPulseSurveyAdjustmentAdjuster(double Cap) : IGlobalPulseSurveyAdjustmentFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalPulseSurveyAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingGlobalPulseSurveyAdjustmentAdjuster(double Step) : IGlobalPulseSurveyAdjustmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalPulseSurveyAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredGlobalPulseSurveyAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IGlobalPulseSurveyAdjustmentFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalPulseSurveyAdjustment item, double valueValue)
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

/// <summary>Runs the global pulse survey adjustment steps in order and keeps a trace of each.</summary>
public sealed class GlobalPulseSurveyAdjustmentEngine(List<IGlobalPulseSurveyAdjustmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalPulseSurveyAdjustmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalPulseSurveyAdjustmentEngine DefaultGlobalPulseSurveyAdjustmentEngine()
    {
        return new GlobalPulseSurveyAdjustmentEngine(
            new List<IGlobalPulseSurveyAdjustmentFactor>
            {
                new CapGlobalPulseSurveyAdjustmentAdjuster(26292.0),
                new RoundingGlobalPulseSurveyAdjustmentAdjuster(5.0),
                new TieredGlobalPulseSurveyAdjustmentAdjuster(
                    new List<double> { 3763.0, 7246.0, 9098.0 },
                    new List<double> { 0.81, 0.8, 1.16 }),
            },
            new List<string>(),
            4.9,
            5878.5);
    }

    public double Run(GlobalPulseSurveyAdjustment item)
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

    public Dictionary<string, double> EvaluateAll(List<GlobalPulseSurveyAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
