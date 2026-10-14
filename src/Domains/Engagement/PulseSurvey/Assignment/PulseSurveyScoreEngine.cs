namespace Acme.Workforce.Engagement.PulseSurvey.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pulse survey score calculation.</summary>
public interface IPulseSurveyScoreAdjuster
{
    string Describe();
    double Adjust(PulseSurveyScore item, double valueValue);
}

public sealed record FlaggedPulseSurveyScoreAdjuster(Registry Flags, string Flag, double Percent) : IPulseSurveyScoreAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PulseSurveyScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPulseSurveyScoreAdjuster(double Cap) : IPulseSurveyScoreAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PulseSurveyScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumPulseSurveyScoreAdjuster(double Minimum) : IPulseSurveyScoreAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PulseSurveyScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pulse survey score steps in order and keeps a trace of each.</summary>
public sealed class PulseSurveyScoreEngine(List<IPulseSurveyScoreAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPulseSurveyScoreAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PulseSurveyScoreEngine DefaultPulseSurveyScoreEngine(Registry flags)
    {
        return new PulseSurveyScoreEngine(
            new List<IPulseSurveyScoreAdjuster>
            {
                new FlaggedPulseSurveyScoreAdjuster(flags, "pulse-survey-score-seasonal", -1.6),
                new CapPulseSurveyScoreAdjuster(49532.0),
                new MinimumPulseSurveyScoreAdjuster(40.8),
            },
            new List<string>(),
            0.5,
            25962.3);
    }

    public double Compute(PulseSurveyScore item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PulseSurveyScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
