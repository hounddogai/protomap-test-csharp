namespace Acme.Workforce.Engagement.PulseSurvey.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pulse survey target calculation.</summary>
public interface IPulseSurveyTargetFactor
{
    string Describe();
    double Adjust(PulseSurveyTarget item, double valueValue);
}

public sealed record RoundingPulseSurveyTargetAdjuster(double Step) : IPulseSurveyTargetFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PulseSurveyTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PulseSurveyTargetCostOffsetAdjuster(double Weight) : IPulseSurveyTargetFactor
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(PulseSurveyTarget item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record FlaggedPulseSurveyTargetAdjuster(Registry Flags, string Flag, double Percent) : IPulseSurveyTargetFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PulseSurveyTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPulseSurveyTargetAdjuster(double Minimum) : IPulseSurveyTargetFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PulseSurveyTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pulse survey target steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveyTargetEngine(List<IPulseSurveyTargetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveyTargetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveyTargetEngine DefaultPulseSurveyTargetEngine(Registry flags)
    {
        return new PulseSurveyTargetEngine(
            new List<IPulseSurveyTargetFactor>
            {
                new RoundingPulseSurveyTargetAdjuster(5.0),
                new PulseSurveyTargetCostOffsetAdjuster(1.66),
                new FlaggedPulseSurveyTargetAdjuster(flags, "pulse-survey-target-primary", 5.3),
                new MinimumPulseSurveyTargetAdjuster(48.0),
            },
            new List<string>(),
            4.4,
            99764.0);
    }

    public double Run(PulseSurveyTarget item)
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveyTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
