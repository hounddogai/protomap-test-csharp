namespace Acme.Workforce.Workplace.FloorPlan.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the floor plan review calculation.</summary>
public interface IFloorPlanReviewAdjuster
{
    string Describe();
    double Adjust(FloorPlanReview item, double valueValue);
}

public sealed record CategoryFloorPlanReviewAdjuster(Dictionary<string, double> Factors) : IFloorPlanReviewAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(FloorPlanReview item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FloorPlanReviewThresholdOffsetAdjuster(double Weight) : IFloorPlanReviewAdjuster
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(FloorPlanReview item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record RoundingFloorPlanReviewAdjuster(double Step) : IFloorPlanReviewAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FloorPlanReview item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumFloorPlanReviewAdjuster(double Minimum) : IFloorPlanReviewAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FloorPlanReview item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentFloorPlanReviewAdjuster(double Percent) : IFloorPlanReviewAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FloorPlanReview item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the floor plan review steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanReviewEngine(List<IFloorPlanReviewAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanReviewAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanReviewEngine DefaultFloorPlanReviewEngine()
    {
        return new FloorPlanReviewEngine(
            new List<IFloorPlanReviewAdjuster>
            {
                new CategoryFloorPlanReviewAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.36, ["retail"] = 0.8, ["east"] = 0.71 }),
                new FloorPlanReviewThresholdOffsetAdjuster(1.25),
                new RoundingFloorPlanReviewAdjuster(0.5),
                new MinimumFloorPlanReviewAdjuster(49.4),
                new PercentFloorPlanReviewAdjuster(6.9),
            },
            new List<string>(),
            0.6,
            45832.3);
    }

    public double Run(FloorPlanReview item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanReview> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
