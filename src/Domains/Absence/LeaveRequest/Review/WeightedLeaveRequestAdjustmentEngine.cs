namespace Acme.Workforce.Absence.LeaveRequest.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted leave request adjustment calculation.</summary>
public interface IWeightedLeaveRequestAdjustmentAdjuster
{
    string Describe();
    double Adjust(WeightedLeaveRequestAdjustment item, double valueValue);
}

public sealed record WeightedLeaveRequestAdjustmentScoreOffsetAdjuster(double Weight) : IWeightedLeaveRequestAdjustmentAdjuster
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(WeightedLeaveRequestAdjustment item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record RoundingWeightedLeaveRequestAdjustmentAdjuster(double Step) : IWeightedLeaveRequestAdjustmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedLeaveRequestAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapWeightedLeaveRequestAdjustmentAdjuster(double Cap) : IWeightedLeaveRequestAdjustmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedLeaveRequestAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SourceWeightedLeaveRequestAdjustmentAdjuster(Dictionary<string, double> Factors) : IWeightedLeaveRequestAdjustmentAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(WeightedLeaveRequestAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentWeightedLeaveRequestAdjustmentAdjuster(double Percent) : IWeightedLeaveRequestAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedLeaveRequestAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted leave request adjustment steps in order and keeps a trace of each.</summary>
public sealed class WeightedLeaveRequestAdjustmentEngine(List<IWeightedLeaveRequestAdjustmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedLeaveRequestAdjustmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedLeaveRequestAdjustmentEngine DefaultWeightedLeaveRequestAdjustmentEngine()
    {
        return new WeightedLeaveRequestAdjustmentEngine(
            new List<IWeightedLeaveRequestAdjustmentAdjuster>
            {
                new WeightedLeaveRequestAdjustmentScoreOffsetAdjuster(1.37),
                new RoundingWeightedLeaveRequestAdjustmentAdjuster(0.1),
                new CapWeightedLeaveRequestAdjustmentAdjuster(36530.0),
                new SourceWeightedLeaveRequestAdjustmentAdjuster(new Dictionary<string, double> { ["north"] = 0.93 }),
                new PercentWeightedLeaveRequestAdjustmentAdjuster(14.8),
            },
            new List<string>(),
            0.3,
            87628.6);
    }

    public double Compute(WeightedLeaveRequestAdjustment item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedLeaveRequestAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
