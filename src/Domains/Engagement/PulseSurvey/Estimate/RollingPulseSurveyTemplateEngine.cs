namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling pulse survey template calculation.</summary>
public interface IRollingPulseSurveyTemplateFactor
{
    string Describe();
    double Adjust(RollingPulseSurveyTemplate item, double valueValue);
}

public sealed record FlaggedRollingPulseSurveyTemplateAdjuster(Registry Flags, string Flag, double Percent) : IRollingPulseSurveyTemplateFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingPulseSurveyTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRollingPulseSurveyTemplateAdjuster(double Step) : IRollingPulseSurveyTemplateFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingPulseSurveyTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapRollingPulseSurveyTemplateAdjuster(double Cap) : IRollingPulseSurveyTemplateFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingPulseSurveyTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredRollingPulseSurveyTemplateAdjuster(List<double> Bounds, List<double> Rates) : IRollingPulseSurveyTemplateFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingPulseSurveyTemplate item, double valueValue)
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

/// <summary>Runs the rolling pulse survey template steps in order and keeps a trace of each.</summary>
public sealed class RollingPulseSurveyTemplateEngine(List<IRollingPulseSurveyTemplateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingPulseSurveyTemplateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingPulseSurveyTemplateEngine DefaultRollingPulseSurveyTemplateEngine(Registry flags)
    {
        return new RollingPulseSurveyTemplateEngine(
            new List<IRollingPulseSurveyTemplateFactor>
            {
                new FlaggedRollingPulseSurveyTemplateAdjuster(flags, "rolling-pulse-survey-template-lenient", -9.9),
                new RoundingRollingPulseSurveyTemplateAdjuster(0.1),
                new CapRollingPulseSurveyTemplateAdjuster(8461.0),
                new TieredRollingPulseSurveyTemplateAdjuster(
                    new List<double> { 1466.0, 4179.0 },
                    new List<double> { 1.08, 0.96 }),
            },
            new List<string>(),
            0.6,
            36344.3);
    }

    public double Compute(RollingPulseSurveyTemplate item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<RollingPulseSurveyTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
