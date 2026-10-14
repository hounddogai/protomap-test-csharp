namespace Acme.Workforce.Engagement.PulseSurvey.Exception;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pulse survey exception calculation.</summary>
public interface IPulseSurveyExceptionModifier
{
    string Describe();
    double Adjust(PulseSurveyException item, double valueValue);
}

public sealed record RoundingPulseSurveyExceptionAdjuster(double Step) : IPulseSurveyExceptionModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PulseSurveyException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CategoryPulseSurveyExceptionAdjuster(Dictionary<string, double> Factors) : IPulseSurveyExceptionModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(PulseSurveyException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPulseSurveyExceptionAdjuster(List<double> Bounds, List<double> Rates) : IPulseSurveyExceptionModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PulseSurveyException item, double valueValue)
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

public sealed record FlaggedPulseSurveyExceptionAdjuster(Registry Flags, string Flag, double Percent) : IPulseSurveyExceptionModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PulseSurveyException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPulseSurveyExceptionAdjuster(double Cap) : IPulseSurveyExceptionModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PulseSurveyException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the pulse survey exception steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveyExceptionEngine(List<IPulseSurveyExceptionModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveyExceptionModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveyExceptionEngine DefaultPulseSurveyExceptionEngine(Registry flags)
    {
        return new PulseSurveyExceptionEngine(
            new List<IPulseSurveyExceptionModifier>
            {
                new RoundingPulseSurveyExceptionAdjuster(1.0),
                new CategoryPulseSurveyExceptionAdjuster(new Dictionary<string, double> { ["south"] = 0.89 }),
                new TieredPulseSurveyExceptionAdjuster(
                    new List<double> { 7841.0, 9740.0 },
                    new List<double> { 0.86, 1.07 }),
                new FlaggedPulseSurveyExceptionAdjuster(flags, "pulse-survey-exception-batched", 4.6),
                new CapPulseSurveyExceptionAdjuster(19506.0),
            },
            new List<string>(),
            2.4,
            36331.9);
    }

    public double Evaluate(PulseSurveyException item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveyException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
