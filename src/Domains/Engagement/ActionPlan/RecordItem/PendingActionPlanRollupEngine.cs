namespace Acme.Workforce.Engagement.ActionPlan.RecordItem;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pending action plan rollup calculation.</summary>
public interface IPendingActionPlanRollupStep
{
    string Describe();
    double Adjust(PendingActionPlanRollup item, double valueValue);
}

public sealed record CapPendingActionPlanRollupAdjuster(double Cap) : IPendingActionPlanRollupStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingActionPlanRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPendingActionPlanRollupAdjuster(Registry Flags, string Flag, double Percent) : IPendingActionPlanRollupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingActionPlanRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPendingActionPlanRollupAdjuster(List<double> Bounds, List<double> Rates) : IPendingActionPlanRollupStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingActionPlanRollup item, double valueValue)
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

public sealed record LabelPendingActionPlanRollupAdjuster(Dictionary<string, double> Factors) : IPendingActionPlanRollupStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PendingActionPlanRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pending action plan rollup steps in order and keeps a trace of each.</summary>
public sealed class PendingActionPlanRollupEngine(List<IPendingActionPlanRollupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingActionPlanRollupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingActionPlanRollupEngine DefaultPendingActionPlanRollupEngine(Registry flags)
    {
        return new PendingActionPlanRollupEngine(
            new List<IPendingActionPlanRollupStep>
            {
                new CapPendingActionPlanRollupAdjuster(5843.0),
                new FlaggedPendingActionPlanRollupAdjuster(flags, "pending-action-plan-rollup-pending", 7.9),
                new TieredPendingActionPlanRollupAdjuster(
                    new List<double> { 6964.0, 7295.0, 7723.0, 9402.0 },
                    new List<double> { 1.01, 1.14, 0.85, 1.17 }),
                new LabelPendingActionPlanRollupAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.77, ["retail"] = 0.99, ["west"] = 0.81 }),
            },
            new List<string>(),
            0.0,
            15003.3);
    }

    public double Evaluate(PendingActionPlanRollup item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PendingActionPlanRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
