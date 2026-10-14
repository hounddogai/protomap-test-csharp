namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pulse survey mapping calculation.</summary>
public interface IPulseSurveyMappingFactor
{
    string Describe();
    double Adjust(PulseSurveyMapping item, double valueValue);
}

public sealed record PercentPulseSurveyMappingAdjuster(double Percent) : IPulseSurveyMappingFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PulseSurveyMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPulseSurveyMappingAdjuster(double Cap) : IPulseSurveyMappingFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PulseSurveyMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingPulseSurveyMappingAdjuster(double Step) : IPulseSurveyMappingFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PulseSurveyMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedPulseSurveyMappingAdjuster(Registry Flags, string Flag, double Percent) : IPulseSurveyMappingFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PulseSurveyMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelPulseSurveyMappingAdjuster(Dictionary<string, double> Factors) : IPulseSurveyMappingFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PulseSurveyMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPulseSurveyMappingAdjuster(double Minimum) : IPulseSurveyMappingFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PulseSurveyMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pulse survey mapping steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveyMappingEngine(List<IPulseSurveyMappingFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveyMappingFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveyMappingEngine DefaultPulseSurveyMappingEngine(Registry flags)
    {
        return new PulseSurveyMappingEngine(
            new List<IPulseSurveyMappingFactor>
            {
                new PercentPulseSurveyMappingAdjuster(3.7),
                new CapPulseSurveyMappingAdjuster(34519.0),
                new RoundingPulseSurveyMappingAdjuster(0.1),
                new FlaggedPulseSurveyMappingAdjuster(flags, "pulse-survey-mapping-rolling", 5.0),
                new LabelPulseSurveyMappingAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.21, ["central"] = 0.85, ["east"] = 0.99 }),
                new MinimumPulseSurveyMappingAdjuster(13.5),
            },
            new List<string>(),
            4.3,
            51194.8);
    }

    public double Evaluate(PulseSurveyMapping item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveyMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
