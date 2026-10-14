namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the adaptive reorg budget calculation.</summary>
public interface IAdaptiveReorgBudgetStep
{
    string Describe();
    double Adjust(AdaptiveReorgBudget item, double valueValue);
}

public sealed record MinimumAdaptiveReorgBudgetAdjuster(double Minimum) : IAdaptiveReorgBudgetStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveReorgBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAdaptiveReorgBudgetAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveReorgBudgetStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveReorgBudget item, double valueValue)
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

public sealed record FlaggedAdaptiveReorgBudgetAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveReorgBudgetStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveReorgBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CategoryAdaptiveReorgBudgetAdjuster(Dictionary<string, double> Factors) : IAdaptiveReorgBudgetStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(AdaptiveReorgBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the adaptive reorg budget steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveReorgBudgetEngine(List<IAdaptiveReorgBudgetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveReorgBudgetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveReorgBudgetEngine DefaultAdaptiveReorgBudgetEngine(Registry flags)
    {
        return new AdaptiveReorgBudgetEngine(
            new List<IAdaptiveReorgBudgetStep>
            {
                new MinimumAdaptiveReorgBudgetAdjuster(17.3),
                new TieredAdaptiveReorgBudgetAdjuster(
                    new List<double> { 3100.0, 5348.0 },
                    new List<double> { 0.96, 0.93 }),
                new FlaggedAdaptiveReorgBudgetAdjuster(flags, "adaptive-reorg-budget-manual", 8.2),
                new CategoryAdaptiveReorgBudgetAdjuster(new Dictionary<string, double> { ["central"] = 1.4 }),
            },
            new List<string>(),
            1.7,
            16144.0);
    }

    public double Compute(AdaptiveReorgBudget item)
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveReorgBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
