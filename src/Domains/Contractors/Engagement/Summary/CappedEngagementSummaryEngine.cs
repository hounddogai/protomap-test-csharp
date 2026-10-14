namespace Acme.Workforce.Contractors.Engagement.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped engagement summary calculation.</summary>
public interface ICappedEngagementSummaryStep
{
    string Describe();
    double Adjust(CappedEngagementSummary item, double valueValue);
}

public sealed record CapCappedEngagementSummaryAdjuster(double Cap) : ICappedEngagementSummaryStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedEngagementSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumCappedEngagementSummaryAdjuster(double Minimum) : ICappedEngagementSummaryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedEngagementSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedCappedEngagementSummaryAdjuster(Registry Flags, string Flag, double Percent) : ICappedEngagementSummaryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedEngagementSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentCappedEngagementSummaryAdjuster(double Percent) : ICappedEngagementSummaryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedEngagementSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCappedEngagementSummaryAdjuster(double Step) : ICappedEngagementSummaryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedEngagementSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the capped engagement summary steps in order and keeps a trace of each.</summary>
public sealed class CappedEngagementSummaryEngine(List<ICappedEngagementSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedEngagementSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedEngagementSummaryEngine DefaultCappedEngagementSummaryEngine(Registry flags)
    {
        return new CappedEngagementSummaryEngine(
            new List<ICappedEngagementSummaryStep>
            {
                new CapCappedEngagementSummaryAdjuster(39610.0),
                new MinimumCappedEngagementSummaryAdjuster(22.6),
                new FlaggedCappedEngagementSummaryAdjuster(flags, "capped-engagement-summary-primary", 2.4),
                new PercentCappedEngagementSummaryAdjuster(22.5),
                new RoundingCappedEngagementSummaryAdjuster(0.05),
            },
            new List<string>(),
            0.6,
            3878.7);
    }

    public double Run(CappedEngagementSummary item)
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

    public Dictionary<string, double> EvaluateAll(List<CappedEngagementSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
