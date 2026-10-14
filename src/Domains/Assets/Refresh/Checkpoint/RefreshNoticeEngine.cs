namespace Acme.Workforce.Assets.Refresh.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh notice calculation.</summary>
public interface IRefreshNoticeStep
{
    string Describe();
    double Adjust(RefreshNotice item, double valueValue);
}

public sealed record MinimumRefreshNoticeAdjuster(double Minimum) : IRefreshNoticeStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RefreshNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentRefreshNoticeAdjuster(double Percent) : IRefreshNoticeStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RefreshNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RefreshNoticeAttemptsOffsetAdjuster(double Weight) : IRefreshNoticeStep
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(RefreshNotice item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record SegmentRefreshNoticeAdjuster(Dictionary<string, double> Factors) : IRefreshNoticeStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(RefreshNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the refresh notice steps in order and keeps a trace of each.</summary>
public sealed class RefreshNoticeEngine(List<IRefreshNoticeStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshNoticeStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshNoticeEngine DefaultRefreshNoticeEngine()
    {
        return new RefreshNoticeEngine(
            new List<IRefreshNoticeStep>
            {
                new MinimumRefreshNoticeAdjuster(32.6),
                new PercentRefreshNoticeAdjuster(-9.5),
                new RefreshNoticeAttemptsOffsetAdjuster(0.88),
                new SegmentRefreshNoticeAdjuster(new Dictionary<string, double> { ["east"] = 0.87 }),
            },
            new List<string>(),
            1.4,
            30687.5);
    }

    public double Price(RefreshNotice item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<RefreshNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
