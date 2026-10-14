namespace Acme.Workforce.Absence.LeaveRequest.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal leave request review calculation.</summary>
public interface ISeasonalLeaveRequestReviewAdjuster
{
    string Describe();
    double Adjust(SeasonalLeaveRequestReview item, double valueValue);
}

public sealed record MinimumSeasonalLeaveRequestReviewAdjuster(double Minimum) : ISeasonalLeaveRequestReviewAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalLeaveRequestReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleSeasonalLeaveRequestReviewAdjuster(Dictionary<string, double> Factors) : ISeasonalLeaveRequestReviewAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(SeasonalLeaveRequestReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredSeasonalLeaveRequestReviewAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalLeaveRequestReviewAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalLeaveRequestReview item, double valueValue)
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

/// <summary>Runs the seasonal leave request review steps in order and keeps a trace of each.</summary>
public sealed class SeasonalLeaveRequestReviewEngine(List<ISeasonalLeaveRequestReviewAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalLeaveRequestReviewAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalLeaveRequestReviewEngine DefaultSeasonalLeaveRequestReviewEngine()
    {
        return new SeasonalLeaveRequestReviewEngine(
            new List<ISeasonalLeaveRequestReviewAdjuster>
            {
                new MinimumSeasonalLeaveRequestReviewAdjuster(38.3),
                new TitleSeasonalLeaveRequestReviewAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.01, ["east"] = 1.27 }),
                new TieredSeasonalLeaveRequestReviewAdjuster(
                    new List<double> { 4872.0, 5135.0, 8919.0 },
                    new List<double> { 0.81, 1.13, 1.01 }),
            },
            new List<string>(),
            3.9,
            82119.4);
    }

    public double Run(SeasonalLeaveRequestReview item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalLeaveRequestReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
