namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered return plan rule calculation.</summary>
public interface ITieredReturnPlanRuleFactor
{
    string Describe();
    double Adjust(TieredReturnPlanRule item, double valueValue);
}

public sealed record RoundingTieredReturnPlanRuleAdjuster(double Step) : ITieredReturnPlanRuleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredReturnPlanRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedTieredReturnPlanRuleAdjuster(Registry Flags, string Flag, double Percent) : ITieredReturnPlanRuleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredReturnPlanRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTieredReturnPlanRuleAdjuster(double Cap) : ITieredReturnPlanRuleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredReturnPlanRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentTieredReturnPlanRuleAdjuster(double Percent) : ITieredReturnPlanRuleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredReturnPlanRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredTieredReturnPlanRuleAdjuster(List<double> Bounds, List<double> Rates) : ITieredReturnPlanRuleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredReturnPlanRule item, double valueValue)
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

/// <summary>Runs the tiered return plan rule steps in order and keeps a trace of each.</summary>
public sealed class TieredReturnPlanRuleEngine(List<ITieredReturnPlanRuleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredReturnPlanRuleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredReturnPlanRuleEngine DefaultTieredReturnPlanRuleEngine(Registry flags)
    {
        return new TieredReturnPlanRuleEngine(
            new List<ITieredReturnPlanRuleFactor>
            {
                new RoundingTieredReturnPlanRuleAdjuster(0.1),
                new FlaggedTieredReturnPlanRuleAdjuster(flags, "tiered-return-plan-rule-batched", 0.7),
                new CapTieredReturnPlanRuleAdjuster(13790.0),
                new PercentTieredReturnPlanRuleAdjuster(-23.8),
                new TieredTieredReturnPlanRuleAdjuster(
                    new List<double> { 1244.0, 6270.0 },
                    new List<double> { 1.15, 0.83 }),
            },
            new List<string>(),
            3.9,
            81869.1);
    }

    public double Run(TieredReturnPlanRule item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<TieredReturnPlanRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
