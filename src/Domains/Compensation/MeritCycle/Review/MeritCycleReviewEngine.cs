namespace Acme.Workforce.Compensation.MeritCycle.Review;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the merit cycle review calculation.</summary>
public interface IMeritCycleReviewAdjuster
{
    string Describe();
    double Adjust(MeritCycleReview item, double valueValue);
}

public sealed record TieredMeritCycleReviewAdjuster(List<double> Bounds, List<double> Rates) : IMeritCycleReviewAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MeritCycleReview item, double valueValue)
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

public sealed record FlaggedMeritCycleReviewAdjuster(Registry Flags, string Flag, double Percent) : IMeritCycleReviewAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MeritCycleReview item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleMeritCycleReviewAdjuster(Dictionary<string, double> Factors) : IMeritCycleReviewAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(MeritCycleReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumMeritCycleReviewAdjuster(double Minimum) : IMeritCycleReviewAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MeritCycleReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentMeritCycleReviewAdjuster(double Percent) : IMeritCycleReviewAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MeritCycleReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the merit cycle review steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleReviewEngine(List<IMeritCycleReviewAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleReviewAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleReviewEngine DefaultMeritCycleReviewEngine(Registry flags)
    {
        return new MeritCycleReviewEngine(
            new List<IMeritCycleReviewAdjuster>
            {
                new TieredMeritCycleReviewAdjuster(
                    new List<double> { 5315.0, 5686.0, 7454.0 },
                    new List<double> { 1.16, 0.81, 0.92 }),
                new FlaggedMeritCycleReviewAdjuster(flags, "merit-cycle-review-regional", 9.2),
                new TitleMeritCycleReviewAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.82, ["west"] = 1.16, ["south"] = 1.35 }),
                new MinimumMeritCycleReviewAdjuster(27.8),
                new PercentMeritCycleReviewAdjuster(-4.3),
            },
            new List<string>(),
            2.4,
            18524.1);
    }

    public double Run(MeritCycleReview item)
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
