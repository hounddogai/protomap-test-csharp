namespace Acme.Workforce.Engagement.Recognition.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the recognition milestone calculation.</summary>
public interface IRecognitionMilestoneFactor
{
    string Describe();
    double Adjust(RecognitionMilestone item, double valueValue);
}

public sealed record RegionRecognitionMilestoneAdjuster(Dictionary<string, double> Factors) : IRecognitionMilestoneFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(RecognitionMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RecognitionMilestoneLimitOffsetAdjuster(double Weight) : IRecognitionMilestoneFactor
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(RecognitionMilestone item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record PercentRecognitionMilestoneAdjuster(double Percent) : IRecognitionMilestoneFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RecognitionMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRecognitionMilestoneAdjuster(double Minimum) : IRecognitionMilestoneFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RecognitionMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRecognitionMilestoneAdjuster(double Cap) : IRecognitionMilestoneFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RecognitionMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingRecognitionMilestoneAdjuster(double Step) : IRecognitionMilestoneFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RecognitionMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the recognition milestone steps in order and keeps a trace of each.</summary>
public sealed class RecognitionMilestoneEngine(List<IRecognitionMilestoneFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionMilestoneFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionMilestoneEngine DefaultRecognitionMilestoneEngine()
    {
        return new RecognitionMilestoneEngine(
            new List<IRecognitionMilestoneFactor>
            {
                new RegionRecognitionMilestoneAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.81, ["west"] = 1.17 }),
                new RecognitionMilestoneLimitOffsetAdjuster(1.2),
                new PercentRecognitionMilestoneAdjuster(24.7),
                new MinimumRecognitionMilestoneAdjuster(39.0),
                new CapRecognitionMilestoneAdjuster(49707.0),
                new RoundingRecognitionMilestoneAdjuster(0.25),
            },
            new List<string>(),
            2.7,
            83556.4);
    }

    public double Compute(RecognitionMilestone item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
