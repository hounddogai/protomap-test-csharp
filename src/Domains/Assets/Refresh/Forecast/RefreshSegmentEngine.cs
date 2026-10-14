namespace Acme.Workforce.Assets.Refresh.Forecast;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh segment calculation.</summary>
public interface IRefreshSegmentAdjuster
{
    string Describe();
    double Adjust(RefreshSegment item, double valueValue);
}

public sealed record MinimumRefreshSegmentAdjuster(double Minimum) : IRefreshSegmentAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RefreshSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentRefreshSegmentAdjuster(double Percent) : IRefreshSegmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RefreshSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRefreshSegmentAdjuster(double Step) : IRefreshSegmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RefreshSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record RefreshSegmentSequenceOffsetAdjuster(double Weight) : IRefreshSegmentAdjuster
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(RefreshSegment item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record FlaggedRefreshSegmentAdjuster(Registry Flags, string Flag, double Percent) : IRefreshSegmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RefreshSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the refresh segment steps in order and keeps a trace of each.</summary>
public sealed class RefreshSegmentEngine(List<IRefreshSegmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshSegmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshSegmentEngine DefaultRefreshSegmentEngine(Registry flags)
    {
        return new RefreshSegmentEngine(
            new List<IRefreshSegmentAdjuster>
            {
                new MinimumRefreshSegmentAdjuster(23.8),
                new PercentRefreshSegmentAdjuster(-13.0),
                new RoundingRefreshSegmentAdjuster(5.0),
                new RefreshSegmentSequenceOffsetAdjuster(0.59),
                new FlaggedRefreshSegmentAdjuster(flags, "refresh-segment-regional", -8.5),
            },
            new List<string>(),
            3.5,
            49373.4);
    }

    public double Evaluate(RefreshSegment item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RefreshSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
