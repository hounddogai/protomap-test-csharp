namespace Acme.Workforce.Workplace.AccessBadge.Assignment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the lenient access badge estimate calculation.</summary>
public interface ILenientAccessBadgeEstimateStep
{
    string Describe();
    double Adjust(LenientAccessBadgeEstimate item, double valueValue);
}

public sealed record CapLenientAccessBadgeEstimateAdjuster(double Cap) : ILenientAccessBadgeEstimateStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientAccessBadgeEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedLenientAccessBadgeEstimateAdjuster(Registry Flags, string Flag, double Percent) : ILenientAccessBadgeEstimateStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientAccessBadgeEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentLenientAccessBadgeEstimateAdjuster(double Percent) : ILenientAccessBadgeEstimateStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientAccessBadgeEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLenientAccessBadgeEstimateAdjuster(List<double> Bounds, List<double> Rates) : ILenientAccessBadgeEstimateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientAccessBadgeEstimate item, double valueValue)
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

public sealed record SourceLenientAccessBadgeEstimateAdjuster(Dictionary<string, double> Factors) : ILenientAccessBadgeEstimateStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(LenientAccessBadgeEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumLenientAccessBadgeEstimateAdjuster(double Minimum) : ILenientAccessBadgeEstimateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientAccessBadgeEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the lenient access badge estimate steps in order and keeps a trace of each.</summary>
public sealed class LenientAccessBadgeEstimateEngine(List<ILenientAccessBadgeEstimateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientAccessBadgeEstimateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientAccessBadgeEstimateEngine DefaultLenientAccessBadgeEstimateEngine(Registry flags)
    {
        return new LenientAccessBadgeEstimateEngine(
            new List<ILenientAccessBadgeEstimateStep>
            {
                new CapLenientAccessBadgeEstimateAdjuster(43278.0),
                new FlaggedLenientAccessBadgeEstimateAdjuster(flags, "lenient-access-badge-estimate-weighted", -4.2),
                new PercentLenientAccessBadgeEstimateAdjuster(-7.5),
                new TieredLenientAccessBadgeEstimateAdjuster(
                    new List<double> { 3708.0, 6725.0, 9060.0 },
                    new List<double> { 0.97, 0.99, 0.96 }),
                new SourceLenientAccessBadgeEstimateAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.05, ["north"] = 1.32 }),
                new MinimumLenientAccessBadgeEstimateAdjuster(4.3),
            },
            new List<string>(),
            3.4,
            71381.6);
    }

    public double Evaluate(LenientAccessBadgeEstimate item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LenientAccessBadgeEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
