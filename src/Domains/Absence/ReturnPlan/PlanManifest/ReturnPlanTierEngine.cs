namespace Acme.Workforce.Absence.ReturnPlan.PlanManifest;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the return plan tier calculation.</summary>
public interface IReturnPlanTierStep
{
    string Describe();
    double Adjust(ReturnPlanTier item, double valueValue);
}

public sealed record MinimumReturnPlanTierAdjuster(double Minimum) : IReturnPlanTierStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReturnPlanTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedReturnPlanTierAdjuster(Registry Flags, string Flag, double Percent) : IReturnPlanTierStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReturnPlanTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelReturnPlanTierAdjuster(Dictionary<string, double> Factors) : IReturnPlanTierStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ReturnPlanTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the return plan tier steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanTierEngine(List<IReturnPlanTierStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanTierStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanTierEngine DefaultReturnPlanTierEngine(Registry flags)
    {
        return new ReturnPlanTierEngine(
            new List<IReturnPlanTierStep>
            {
                new MinimumReturnPlanTierAdjuster(43.7),
                new FlaggedReturnPlanTierAdjuster(flags, "return-plan-tier-legacy", 9.5),
                new LabelReturnPlanTierAdjuster(new Dictionary<string, double> { ["east"] = 0.89, ["north"] = 1.1 }),
            },
            new List<string>(),
            1.9,
            20798.7);
    }

    public double Compute(ReturnPlanTier item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
