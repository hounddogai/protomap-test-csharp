namespace Acme.Workforce.Absence.ReturnPlan.Window;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the tiered return plan item calculation.</summary>
public interface ITieredReturnPlanItemStep
{
    string Describe();
    double Adjust(TieredReturnPlanItem item, double valueValue);
}

public sealed record FlaggedTieredReturnPlanItemAdjuster(Registry Flags, string Flag, double Percent) : ITieredReturnPlanItemStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredReturnPlanItem item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumTieredReturnPlanItemAdjuster(double Minimum) : ITieredReturnPlanItemStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredReturnPlanItem item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentTieredReturnPlanItemAdjuster(double Percent) : ITieredReturnPlanItemStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredReturnPlanItem item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTieredReturnPlanItemAdjuster(double Cap) : ITieredReturnPlanItemStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredReturnPlanItem item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the tiered return plan item steps in order and keeps a trace of each.</summary>
public sealed class TieredReturnPlanItemEngine(List<ITieredReturnPlanItemStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredReturnPlanItemStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredReturnPlanItemEngine DefaultTieredReturnPlanItemEngine(Registry flags)
    {
        return new TieredReturnPlanItemEngine(
            new List<ITieredReturnPlanItemStep>
            {
                new FlaggedTieredReturnPlanItemAdjuster(flags, "tiered-return-plan-item-adaptive", 4.3),
                new MinimumTieredReturnPlanItemAdjuster(37.5),
                new PercentTieredReturnPlanItemAdjuster(-2.1),
                new CapTieredReturnPlanItemAdjuster(36106.0),
            },
            new List<string>(),
            2.0,
            42610.7);
    }

    public double Evaluate(TieredReturnPlanItem item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<TieredReturnPlanItem> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
