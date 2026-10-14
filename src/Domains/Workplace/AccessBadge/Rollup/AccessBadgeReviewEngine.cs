namespace Acme.Workforce.Workplace.AccessBadge.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge review calculation.</summary>
public interface IAccessBadgeReviewAdjuster
{
    string Describe();
    double Adjust(AccessBadgeReview item, double valueValue);
}

public sealed record MinimumAccessBadgeReviewAdjuster(double Minimum) : IAccessBadgeReviewAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccessBadgeReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentAccessBadgeReviewAdjuster(double Percent) : IAccessBadgeReviewAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccessBadgeReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedAccessBadgeReviewAdjuster(Registry Flags, string Flag, double Percent) : IAccessBadgeReviewAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccessBadgeReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAccessBadgeReviewAdjuster(double Cap) : IAccessBadgeReviewAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccessBadgeReview item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record AccessBadgeReviewAttemptsOffsetAdjuster(double Weight) : IAccessBadgeReviewAdjuster
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(AccessBadgeReview item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record RoundingAccessBadgeReviewAdjuster(double Step) : IAccessBadgeReviewAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccessBadgeReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the access badge review steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeReviewEngine(List<IAccessBadgeReviewAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeReviewAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeReviewEngine DefaultAccessBadgeReviewEngine(Registry flags)
    {
        return new AccessBadgeReviewEngine(
            new List<IAccessBadgeReviewAdjuster>
            {
                new MinimumAccessBadgeReviewAdjuster(10.8),
                new PercentAccessBadgeReviewAdjuster(0.0),
                new FlaggedAccessBadgeReviewAdjuster(flags, "access-badge-review-fallback", 3.6),
                new CapAccessBadgeReviewAdjuster(10600.0),
                new AccessBadgeReviewAttemptsOffsetAdjuster(0.86),
                new RoundingAccessBadgeReviewAdjuster(0.1),
            },
            new List<string>(),
            3.8,
            73151.4);
    }

    public double Evaluate(AccessBadgeReview item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
