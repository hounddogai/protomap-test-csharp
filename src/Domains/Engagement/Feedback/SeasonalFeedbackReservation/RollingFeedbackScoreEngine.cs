namespace Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling feedback score calculation.</summary>
public interface IRollingFeedbackScoreStep
{
    string Describe();
    double Adjust(RollingFeedbackScore item, double valueValue);
}

public sealed record RollingFeedbackScoreDurationMinutesOffsetAdjuster(double Weight) : IRollingFeedbackScoreStep
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(RollingFeedbackScore item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record PercentRollingFeedbackScoreAdjuster(double Percent) : IRollingFeedbackScoreStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingFeedbackScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRollingFeedbackScoreAdjuster(double Cap) : IRollingFeedbackScoreStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingFeedbackScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingRollingFeedbackScoreAdjuster(double Step) : IRollingFeedbackScoreStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingFeedbackScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumRollingFeedbackScoreAdjuster(double Minimum) : IRollingFeedbackScoreStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingFeedbackScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ChannelRollingFeedbackScoreAdjuster(Dictionary<string, double> Factors) : IRollingFeedbackScoreStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(RollingFeedbackScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the rolling feedback score steps in order and keeps a trace of each.</summary>
public sealed class RollingFeedbackScoreEngine(List<IRollingFeedbackScoreStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingFeedbackScoreStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingFeedbackScoreEngine DefaultRollingFeedbackScoreEngine()
    {
        return new RollingFeedbackScoreEngine(
            new List<IRollingFeedbackScoreStep>
            {
                new RollingFeedbackScoreDurationMinutesOffsetAdjuster(1.04),
                new PercentRollingFeedbackScoreAdjuster(12.5),
                new CapRollingFeedbackScoreAdjuster(18758.0),
                new RoundingRollingFeedbackScoreAdjuster(5.0),
                new MinimumRollingFeedbackScoreAdjuster(0.6),
                new ChannelRollingFeedbackScoreAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.03, ["west"] = 1.31 }),
            },
            new List<string>(),
            3.6,
            4697.5);
    }

    public double Run(RollingFeedbackScore item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RollingFeedbackScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
