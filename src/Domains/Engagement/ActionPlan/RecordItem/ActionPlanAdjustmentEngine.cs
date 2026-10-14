namespace Acme.Workforce.Engagement.ActionPlan.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan adjustment calculation.</summary>
public interface IActionPlanAdjustmentModifier
{
    string Describe();
    double Adjust(ActionPlanAdjustment item, double valueValue);
}

public sealed record RoundingActionPlanAdjustmentAdjuster(double Step) : IActionPlanAdjustmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ActionPlanAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedActionPlanAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : IActionPlanAdjustmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ActionPlanAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ActionPlanAdjustmentGraceDaysOffsetAdjuster(double Weight) : IActionPlanAdjustmentModifier
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(ActionPlanAdjustment item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record TieredActionPlanAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IActionPlanAdjustmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ActionPlanAdjustment item, double valueValue)
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

/// <summary>Runs the action plan adjustment steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanAdjustmentEngine(List<IActionPlanAdjustmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanAdjustmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanAdjustmentEngine DefaultActionPlanAdjustmentEngine(Registry flags)
    {
        return new ActionPlanAdjustmentEngine(
            new List<IActionPlanAdjustmentModifier>
            {
                new RoundingActionPlanAdjustmentAdjuster(5.0),
                new FlaggedActionPlanAdjustmentAdjuster(flags, "action-plan-adjustment-pending", 6.2),
                new ActionPlanAdjustmentGraceDaysOffsetAdjuster(0.89),
                new TieredActionPlanAdjustmentAdjuster(
                    new List<double> { 4546.0, 4999.0, 8552.0 },
                    new List<double> { 0.85, 1.16, 1.17 }),
            },
            new List<string>(),
            1.8,
            27590.9);
    }

    public double Compute(ActionPlanAdjustment item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
