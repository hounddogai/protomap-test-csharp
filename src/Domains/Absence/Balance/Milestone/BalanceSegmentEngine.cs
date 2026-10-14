namespace Acme.Workforce.Absence.Balance.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the balance segment calculation.</summary>
public interface IBalanceSegmentStep
{
    string Describe();
    double Adjust(BalanceSegment item, double valueValue);
}

public sealed record PercentBalanceSegmentAdjuster(double Percent) : IBalanceSegmentStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BalanceSegment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedBalanceSegmentAdjuster(Registry Flags, string Flag, double Percent) : IBalanceSegmentStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BalanceSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumBalanceSegmentAdjuster(double Minimum) : IBalanceSegmentStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BalanceSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the balance segment steps in order and keeps a trace of each.</summary>
public sealed class BalanceSegmentEngine(List<IBalanceSegmentStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBalanceSegmentStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BalanceSegmentEngine DefaultBalanceSegmentEngine(Registry flags)
    {
        return new BalanceSegmentEngine(
            new List<IBalanceSegmentStep>
            {
                new PercentBalanceSegmentAdjuster(14.0),
                new FlaggedBalanceSegmentAdjuster(flags, "balance-segment-tiered", 9.2),
                new MinimumBalanceSegmentAdjuster(31.3),
            },
            new List<string>(),
            0.8,
            31682.7);
    }

    public double Price(BalanceSegment item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<BalanceSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
