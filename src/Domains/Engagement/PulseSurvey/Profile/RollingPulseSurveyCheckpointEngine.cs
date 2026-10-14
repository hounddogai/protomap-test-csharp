namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling pulse survey checkpoint calculation.</summary>
public interface IRollingPulseSurveyCheckpointModifier
{
    string Describe();
    double Adjust(RollingPulseSurveyCheckpoint item, double valueValue);
}

public sealed record TieredRollingPulseSurveyCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IRollingPulseSurveyCheckpointModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingPulseSurveyCheckpoint item, double valueValue)
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

public sealed record CapRollingPulseSurveyCheckpointAdjuster(double Cap) : IRollingPulseSurveyCheckpointModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingPulseSurveyCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedRollingPulseSurveyCheckpointAdjuster(Registry Flags, string Flag, double Percent) : IRollingPulseSurveyCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingPulseSurveyCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRollingPulseSurveyCheckpointAdjuster(double Step) : IRollingPulseSurveyCheckpointModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingPulseSurveyCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentRollingPulseSurveyCheckpointAdjuster(double Percent) : IRollingPulseSurveyCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingPulseSurveyCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling pulse survey checkpoint steps in order and keeps a trace of each.</summary>
public sealed class RollingPulseSurveyCheckpointEngine(List<IRollingPulseSurveyCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingPulseSurveyCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingPulseSurveyCheckpointEngine DefaultRollingPulseSurveyCheckpointEngine(Registry flags)
    {
        return new RollingPulseSurveyCheckpointEngine(
            new List<IRollingPulseSurveyCheckpointModifier>
            {
                new TieredRollingPulseSurveyCheckpointAdjuster(
                    new List<double> { 7237.0, 7472.0 },
                    new List<double> { 0.87, 0.9 }),
                new CapRollingPulseSurveyCheckpointAdjuster(43507.0),
                new FlaggedRollingPulseSurveyCheckpointAdjuster(
                    flags,
                    "rolling-pulse-survey-checkpoint-rolling",
                    -1.6),
                new RoundingRollingPulseSurveyCheckpointAdjuster(5.0),
                new PercentRollingPulseSurveyCheckpointAdjuster(-23.3),
            },
            new List<string>(),
            4.8,
            95752.4);
    }

    public double Price(RollingPulseSurveyCheckpoint item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RollingPulseSurveyCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
