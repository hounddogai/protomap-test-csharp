namespace Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the action plan variance calculation.</summary>
public interface IActionPlanVarianceStep
{
    string Describe();
    double Adjust(ActionPlanVariance item, double valueValue);
}

public sealed record PercentActionPlanVarianceAdjuster(double Percent) : IActionPlanVarianceStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ActionPlanVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredActionPlanVarianceAdjuster(List<double> Bounds, List<double> Rates) : IActionPlanVarianceStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ActionPlanVariance item, double valueValue)
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

public sealed record FlaggedActionPlanVarianceAdjuster(Registry Flags, string Flag, double Percent) : IActionPlanVarianceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ActionPlanVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the action plan variance steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanVarianceEngine(List<IActionPlanVarianceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanVarianceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanVarianceEngine DefaultActionPlanVarianceEngine(Registry flags)
    {
        return new ActionPlanVarianceEngine(
            new List<IActionPlanVarianceStep>
            {
                new PercentActionPlanVarianceAdjuster(4.3),
                new TieredActionPlanVarianceAdjuster(
                    new List<double> { 1905.0, 5464.0, 7014.0, 7768.0 },
                    new List<double> { 1.07, 1.02, 0.92, 0.9 }),
                new FlaggedActionPlanVarianceAdjuster(flags, "action-plan-variance-adaptive", 7.7),
            },
            new List<string>(),
            4.5,
            48791.5);
    }

    public double Run(ActionPlanVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
