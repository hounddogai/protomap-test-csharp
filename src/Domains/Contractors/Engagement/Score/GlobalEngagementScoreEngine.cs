namespace Acme.Workforce.Contractors.Engagement.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global engagement score calculation.</summary>
public interface IGlobalEngagementScoreStep
{
    string Describe();
    double Adjust(GlobalEngagementScore item, double valueValue);
}

public sealed record FlaggedGlobalEngagementScoreAdjuster(Registry Flags, string Flag, double Percent) : IGlobalEngagementScoreStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalEngagementScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapGlobalEngagementScoreAdjuster(double Cap) : IGlobalEngagementScoreStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalEngagementScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentGlobalEngagementScoreAdjuster(double Percent) : IGlobalEngagementScoreStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalEngagementScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumGlobalEngagementScoreAdjuster(double Minimum) : IGlobalEngagementScoreStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalEngagementScore item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleGlobalEngagementScoreAdjuster(Dictionary<string, double> Factors) : IGlobalEngagementScoreStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(GlobalEngagementScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredGlobalEngagementScoreAdjuster(List<double> Bounds, List<double> Rates) : IGlobalEngagementScoreStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalEngagementScore item, double valueValue)
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

/// <summary>Runs the global engagement score steps in order and keeps a trace of each.</summary>
public sealed class GlobalEngagementScoreEngine(List<IGlobalEngagementScoreStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalEngagementScoreStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalEngagementScoreEngine DefaultGlobalEngagementScoreEngine(Registry flags)
    {
        return new GlobalEngagementScoreEngine(
            new List<IGlobalEngagementScoreStep>
            {
                new FlaggedGlobalEngagementScoreAdjuster(flags, "global-engagement-score-tiered", -1.3),
                new CapGlobalEngagementScoreAdjuster(27792.0),
                new PercentGlobalEngagementScoreAdjuster(7.3),
                new MinimumGlobalEngagementScoreAdjuster(45.8),
                new TitleGlobalEngagementScoreAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.27, ["west"] = 0.88, ["retail"] = 1.21 }),
                new TieredGlobalEngagementScoreAdjuster(
                    new List<double> { 1931.0, 6870.0, 7363.0 },
                    new List<double> { 1.2, 0.81, 0.85 }),
            },
            new List<string>(),
            4.7,
            6287.7);
    }

    public double Run(GlobalEngagementScore item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalEngagementScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
