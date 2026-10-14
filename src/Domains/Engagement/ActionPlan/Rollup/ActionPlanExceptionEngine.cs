namespace Acme.Workforce.Engagement.ActionPlan.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan exception calculation.</summary>
public interface IActionPlanExceptionAdjuster
{
    string Describe();
    double Adjust(ActionPlanException item, double valueValue);
}

public sealed record SegmentActionPlanExceptionAdjuster(Dictionary<string, double> Factors) : IActionPlanExceptionAdjuster
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(ActionPlanException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredActionPlanExceptionAdjuster(List<double> Bounds, List<double> Rates) : IActionPlanExceptionAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ActionPlanException item, double valueValue)
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

public sealed record ActionPlanExceptionThresholdOffsetAdjuster(double Weight) : IActionPlanExceptionAdjuster
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(ActionPlanException item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

public sealed record RoundingActionPlanExceptionAdjuster(double Step) : IActionPlanExceptionAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ActionPlanException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the action plan exception steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanExceptionEngine(List<IActionPlanExceptionAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanExceptionAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanExceptionEngine DefaultActionPlanExceptionEngine()
    {
        return new ActionPlanExceptionEngine(
            new List<IActionPlanExceptionAdjuster>
            {
                new SegmentActionPlanExceptionAdjuster(new Dictionary<string, double> { ["east"] = 1.15 }),
                new TieredActionPlanExceptionAdjuster(
                    new List<double> { 1410.0, 4233.0, 5137.0, 6549.0 },
                    new List<double> { 1.18, 0.84, 0.82, 1.09 }),
                new ActionPlanExceptionThresholdOffsetAdjuster(0.21),
                new RoundingActionPlanExceptionAdjuster(0.1),
            },
            new List<string>(),
            1.7,
            87675.4);
    }

    public double Evaluate(ActionPlanException item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
