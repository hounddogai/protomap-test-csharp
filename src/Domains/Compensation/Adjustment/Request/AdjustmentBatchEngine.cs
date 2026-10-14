namespace Acme.Workforce.Compensation.Adjustment.Request;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adjustment batch calculation.</summary>
public interface IAdjustmentBatchModifier
{
    string Describe();
    double Adjust(AdjustmentBatch item, double valueValue);
}

public sealed record MinimumAdjustmentBatchAdjuster(double Minimum) : IAdjustmentBatchModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdjustmentBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SegmentAdjustmentBatchAdjuster(Dictionary<string, double> Factors) : IAdjustmentBatchModifier
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(AdjustmentBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedAdjustmentBatchAdjuster(Registry Flags, string Flag, double Percent) : IAdjustmentBatchModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdjustmentBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAdjustmentBatchAdjuster(double Percent) : IAdjustmentBatchModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdjustmentBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adjustment batch steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentBatchEngine(List<IAdjustmentBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentBatchEngine DefaultAdjustmentBatchEngine(Registry flags)
    {
        return new AdjustmentBatchEngine(
            new List<IAdjustmentBatchModifier>
            {
                new MinimumAdjustmentBatchAdjuster(20.6),
                new SegmentAdjustmentBatchAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.1, ["north"] = 0.75, ["south"] = 1.16 }),
                new FlaggedAdjustmentBatchAdjuster(flags, "adjustment-batch-scheduled", -9.6),
                new PercentAdjustmentBatchAdjuster(-18.5),
            },
            new List<string>(),
            2.8,
            74852.8);
    }

    public double Compute(AdjustmentBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
