namespace Acme.Workforce.Engagement.TownHall.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive town hall score calculation.</summary>
public interface IAdaptiveTownHallScoreStep
{
    string Describe();
    double Adjust(AdaptiveTownHallScore item, double valueValue);
}

public sealed record PercentAdaptiveTownHallScoreAdjuster(double Percent) : IAdaptiveTownHallScoreStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveTownHallScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdaptiveTownHallScoreAdjuster(double Minimum) : IAdaptiveTownHallScoreStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveTownHallScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapAdaptiveTownHallScoreAdjuster(double Cap) : IAdaptiveTownHallScoreStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveTownHallScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameAdaptiveTownHallScoreAdjuster(Dictionary<string, double> Factors) : IAdaptiveTownHallScoreStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AdaptiveTownHallScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingAdaptiveTownHallScoreAdjuster(double Step) : IAdaptiveTownHallScoreStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveTownHallScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the adaptive town hall score steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveTownHallScoreEngine(List<IAdaptiveTownHallScoreStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveTownHallScoreStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveTownHallScoreEngine DefaultAdaptiveTownHallScoreEngine()
    {
        return new AdaptiveTownHallScoreEngine(
            new List<IAdaptiveTownHallScoreStep>
            {
                new PercentAdaptiveTownHallScoreAdjuster(5.3),
                new MinimumAdaptiveTownHallScoreAdjuster(11.9),
                new CapAdaptiveTownHallScoreAdjuster(12920.0),
                new NameAdaptiveTownHallScoreAdjuster(new Dictionary<string, double> { ["retail"] = 1.06 }),
                new RoundingAdaptiveTownHallScoreAdjuster(0.1),
            },
            new List<string>(),
            0.4,
            33024.5);
    }

    public double Evaluate(AdaptiveTownHallScore item)
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveTownHallScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
