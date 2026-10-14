namespace Acme.Workforce.Engagement.ActionPlan.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan batch calculation.</summary>
public interface IActionPlanBatchModifier
{
    string Describe();
    double Adjust(ActionPlanBatch item, double valueValue);
}

public sealed record DepartmentActionPlanBatchAdjuster(Dictionary<string, double> Factors) : IActionPlanBatchModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(ActionPlanBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapActionPlanBatchAdjuster(double Cap) : IActionPlanBatchModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ActionPlanBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredActionPlanBatchAdjuster(List<double> Bounds, List<double> Rates) : IActionPlanBatchModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ActionPlanBatch item, double valueValue)
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

public sealed record ActionPlanBatchCreatedAtOffsetAdjuster(double Weight) : IActionPlanBatchModifier
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(ActionPlanBatch item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record FlaggedActionPlanBatchAdjuster(Registry Flags, string Flag, double Percent) : IActionPlanBatchModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ActionPlanBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the action plan batch steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanBatchEngine(List<IActionPlanBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanBatchEngine DefaultActionPlanBatchEngine(Registry flags)
    {
        return new ActionPlanBatchEngine(
            new List<IActionPlanBatchModifier>
            {
                new DepartmentActionPlanBatchAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.82, ["south"] = 0.81, ["retail"] = 1.4 }),
                new CapActionPlanBatchAdjuster(2862.0),
                new TieredActionPlanBatchAdjuster(
                    new List<double> { 941.0, 1112.0, 5292.0 },
                    new List<double> { 0.97, 0.83, 0.87 }),
                new ActionPlanBatchCreatedAtOffsetAdjuster(0.34),
                new FlaggedActionPlanBatchAdjuster(flags, "action-plan-batch-fallback", 9.8),
            },
            new List<string>(),
            2.7,
            90402.1);
    }

    public double Compute(ActionPlanBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
