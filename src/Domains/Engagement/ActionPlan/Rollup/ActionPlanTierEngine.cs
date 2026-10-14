namespace Acme.Workforce.Engagement.ActionPlan.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan tier calculation.</summary>
public interface IActionPlanTierModifier
{
    string Describe();
    double Adjust(ActionPlanTier item, double valueValue);
}

public sealed record PercentActionPlanTierAdjuster(double Percent) : IActionPlanTierModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ActionPlanTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapActionPlanTierAdjuster(double Cap) : IActionPlanTierModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ActionPlanTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredActionPlanTierAdjuster(List<double> Bounds, List<double> Rates) : IActionPlanTierModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ActionPlanTier item, double valueValue)
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

public sealed record RoundingActionPlanTierAdjuster(double Step) : IActionPlanTierModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ActionPlanTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the action plan tier steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanTierEngine(List<IActionPlanTierModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanTierModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanTierEngine DefaultActionPlanTierEngine()
    {
        return new ActionPlanTierEngine(
            new List<IActionPlanTierModifier>
            {
                new PercentActionPlanTierAdjuster(6.0),
                new CapActionPlanTierAdjuster(6741.0),
                new TieredActionPlanTierAdjuster(new List<double> { 1008.0, 2133.0 }, new List<double> { 0.8, 1.08 }),
                new RoundingActionPlanTierAdjuster(0.5),
            },
            new List<string>(),
            0.7,
            7561.2);
    }

    public double Price(ActionPlanTier item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
