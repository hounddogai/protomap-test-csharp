namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling floor plan batch calculation.</summary>
public interface IRollingFloorPlanBatchStep
{
    string Describe();
    double Adjust(RollingFloorPlanBatch item, double valueValue);
}

public sealed record TitleRollingFloorPlanBatchAdjuster(Dictionary<string, double> Factors) : IRollingFloorPlanBatchStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RollingFloorPlanBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingRollingFloorPlanBatchAdjuster(double Step) : IRollingFloorPlanBatchStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingFloorPlanBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapRollingFloorPlanBatchAdjuster(double Cap) : IRollingFloorPlanBatchStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingFloorPlanBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumRollingFloorPlanBatchAdjuster(double Minimum) : IRollingFloorPlanBatchStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingFloorPlanBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the rolling floor plan batch steps in order and keeps a trace of each.</summary>
public sealed class RollingFloorPlanBatchEngine(List<IRollingFloorPlanBatchStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingFloorPlanBatchStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingFloorPlanBatchEngine DefaultRollingFloorPlanBatchEngine()
    {
        return new RollingFloorPlanBatchEngine(
            new List<IRollingFloorPlanBatchStep>
            {
                new TitleRollingFloorPlanBatchAdjuster(new Dictionary<string, double> { ["central"] = 0.78 }),
                new RoundingRollingFloorPlanBatchAdjuster(0.5),
                new CapRollingFloorPlanBatchAdjuster(23985.0),
                new MinimumRollingFloorPlanBatchAdjuster(40.8),
            },
            new List<string>(),
            1.4,
            94439.3);
    }

    public double Evaluate(RollingFloorPlanBatch item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RollingFloorPlanBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
