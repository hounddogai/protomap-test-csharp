namespace Acme.Workforce.Engagement.PulseSurvey.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pulse survey template calculation.</summary>
public interface IPulseSurveyTemplateFactor
{
    string Describe();
    double Adjust(PulseSurveyTemplate item, double valueValue);
}

public sealed record PulseSurveyTemplateBudgetOffsetAdjuster(double Weight) : IPulseSurveyTemplateFactor
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(PulseSurveyTemplate item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record FlaggedPulseSurveyTemplateAdjuster(Registry Flags, string Flag, double Percent) : IPulseSurveyTemplateFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PulseSurveyTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPulseSurveyTemplateAdjuster(double Step) : IPulseSurveyTemplateFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PulseSurveyTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredPulseSurveyTemplateAdjuster(List<double> Bounds, List<double> Rates) : IPulseSurveyTemplateFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PulseSurveyTemplate item, double valueValue)
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

public sealed record CapPulseSurveyTemplateAdjuster(double Cap) : IPulseSurveyTemplateFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PulseSurveyTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPulseSurveyTemplateAdjuster(double Percent) : IPulseSurveyTemplateFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PulseSurveyTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pulse survey template steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveyTemplateEngine(List<IPulseSurveyTemplateFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveyTemplateFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveyTemplateEngine DefaultPulseSurveyTemplateEngine(Registry flags)
    {
        return new PulseSurveyTemplateEngine(
            new List<IPulseSurveyTemplateFactor>
            {
                new PulseSurveyTemplateBudgetOffsetAdjuster(0.78),
                new FlaggedPulseSurveyTemplateAdjuster(flags, "pulse-survey-template-global", -9.2),
                new RoundingPulseSurveyTemplateAdjuster(0.1),
                new TieredPulseSurveyTemplateAdjuster(
                    new List<double> { 1843.0, 6496.0, 8218.0, 9208.0 },
                    new List<double> { 1.04, 1.0, 0.85, 1.01 }),
                new CapPulseSurveyTemplateAdjuster(4156.0),
                new PercentPulseSurveyTemplateAdjuster(-0.5),
            },
            new List<string>(),
            4.9,
            56259.7);
    }

    public double Evaluate(PulseSurveyTemplate item)
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveyTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
