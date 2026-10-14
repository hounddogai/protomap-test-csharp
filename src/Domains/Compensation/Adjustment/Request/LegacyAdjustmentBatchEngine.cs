namespace Acme.Workforce.Compensation.Adjustment.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy adjustment batch calculation.</summary>
public interface ILegacyAdjustmentBatchModifier
{
    string Describe();
    double Adjust(LegacyAdjustmentBatch item, double valueValue);
}

public sealed record TeamLegacyAdjustmentBatchAdjuster(Dictionary<string, double> Factors) : ILegacyAdjustmentBatchModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(LegacyAdjustmentBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapLegacyAdjustmentBatchAdjuster(double Cap) : ILegacyAdjustmentBatchModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyAdjustmentBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLegacyAdjustmentBatchAdjuster(double Minimum) : ILegacyAdjustmentBatchModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyAdjustmentBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the legacy adjustment batch steps in order and keeps a trace of each.</summary>
public sealed class LegacyAdjustmentBatchEngine(List<ILegacyAdjustmentBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyAdjustmentBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyAdjustmentBatchEngine DefaultLegacyAdjustmentBatchEngine()
    {
        return new LegacyAdjustmentBatchEngine(
            new List<ILegacyAdjustmentBatchModifier>
            {
                new TeamLegacyAdjustmentBatchAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.86, ["central"] = 1.11 }),
                new CapLegacyAdjustmentBatchAdjuster(3245.0),
                new MinimumLegacyAdjustmentBatchAdjuster(22.9),
            },
            new List<string>(),
            0.4,
            48995.3);
    }

    public double Evaluate(LegacyAdjustmentBatch item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyAdjustmentBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
