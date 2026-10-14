namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pulse survey segment calculation.</summary>
public interface IPulseSurveySegmentAdjuster
{
    string Describe();
    double Adjust(PulseSurveySegment item, double valueValue);
}

public sealed record CapPulseSurveySegmentAdjuster(double Cap) : IPulseSurveySegmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PulseSurveySegment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumPulseSurveySegmentAdjuster(double Minimum) : IPulseSurveySegmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PulseSurveySegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPulseSurveySegmentAdjuster(double Percent) : IPulseSurveySegmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PulseSurveySegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPulseSurveySegmentAdjuster(double Step) : IPulseSurveySegmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PulseSurveySegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PulseSurveySegmentWeightOffsetAdjuster(double Weight) : IPulseSurveySegmentAdjuster
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(PulseSurveySegment item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

/// <summary>Runs the pulse survey segment steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveySegmentEngine(List<IPulseSurveySegmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveySegmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveySegmentEngine DefaultPulseSurveySegmentEngine()
    {
        return new PulseSurveySegmentEngine(
            new List<IPulseSurveySegmentAdjuster>
            {
                new CapPulseSurveySegmentAdjuster(29848.0),
                new MinimumPulseSurveySegmentAdjuster(33.7),
                new PercentPulseSurveySegmentAdjuster(21.1),
                new RoundingPulseSurveySegmentAdjuster(0.1),
                new PulseSurveySegmentWeightOffsetAdjuster(0.09),
            },
            new List<string>(),
            3.2,
            91690.2);
    }

    public double Evaluate(PulseSurveySegment item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveySegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
