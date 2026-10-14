namespace Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict assignment adjustment calculation.</summary>
public interface IStrictAssignmentAdjustmentAdjuster
{
    string Describe();
    double Adjust(StrictAssignmentAdjustment item, double valueValue);
}

public sealed record CapStrictAssignmentAdjustmentAdjuster(double Cap) : IStrictAssignmentAdjustmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictAssignmentAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredStrictAssignmentAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IStrictAssignmentAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictAssignmentAdjustment item, double valueValue)
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

public sealed record LabelStrictAssignmentAdjustmentAdjuster(Dictionary<string, double> Factors) : IStrictAssignmentAdjustmentAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(StrictAssignmentAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record StrictAssignmentAdjustmentThresholdOffsetAdjuster(double Weight) : IStrictAssignmentAdjustmentAdjuster
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(StrictAssignmentAdjustment item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

/// <summary>Runs the strict assignment adjustment steps in order and keeps a trace of each.</summary>
public sealed class StrictAssignmentAdjustmentEngine(List<IStrictAssignmentAdjustmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictAssignmentAdjustmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictAssignmentAdjustmentEngine DefaultStrictAssignmentAdjustmentEngine()
    {
        return new StrictAssignmentAdjustmentEngine(
            new List<IStrictAssignmentAdjustmentAdjuster>
            {
                new CapStrictAssignmentAdjustmentAdjuster(11166.0),
                new TieredStrictAssignmentAdjustmentAdjuster(
                    new List<double> { 1182.0, 3634.0, 5122.0, 7393.0 },
                    new List<double> { 1.09, 1.14, 0.9, 0.81 }),
                new LabelStrictAssignmentAdjustmentAdjuster(new Dictionary<string, double> { ["central"] = 0.84 }),
                new StrictAssignmentAdjustmentThresholdOffsetAdjuster(0.5),
            },
            new List<string>(),
            2.8,
            49237.4);
    }

    public double Compute(StrictAssignmentAdjustment item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<StrictAssignmentAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
