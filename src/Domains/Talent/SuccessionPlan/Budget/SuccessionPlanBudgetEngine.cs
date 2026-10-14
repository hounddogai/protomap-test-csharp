namespace Acme.Workforce.Talent.SuccessionPlan.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan budget calculation.</summary>
public interface ISuccessionPlanBudgetModifier
{
    string Describe();
    double Adjust(SuccessionPlanBudget item, double valueValue);
}

public sealed record NameSuccessionPlanBudgetAdjuster(Dictionary<string, double> Factors) : ISuccessionPlanBudgetModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(SuccessionPlanBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingSuccessionPlanBudgetAdjuster(double Step) : ISuccessionPlanBudgetModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SuccessionPlanBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredSuccessionPlanBudgetAdjuster(List<double> Bounds, List<double> Rates) : ISuccessionPlanBudgetModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SuccessionPlanBudget item, double valueValue)
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

public sealed record FlaggedSuccessionPlanBudgetAdjuster(Registry Flags, string Flag, double Percent) : ISuccessionPlanBudgetModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SuccessionPlanBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SuccessionPlanBudgetRateOffsetAdjuster(double Weight) : ISuccessionPlanBudgetModifier
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(SuccessionPlanBudget item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record CapSuccessionPlanBudgetAdjuster(double Cap) : ISuccessionPlanBudgetModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SuccessionPlanBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the succession plan budget steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanBudgetEngine(List<ISuccessionPlanBudgetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanBudgetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanBudgetEngine DefaultSuccessionPlanBudgetEngine(Registry flags)
    {
        return new SuccessionPlanBudgetEngine(
            new List<ISuccessionPlanBudgetModifier>
            {
                new NameSuccessionPlanBudgetAdjuster(new Dictionary<string, double> { ["central"] = 0.97 }),
                new RoundingSuccessionPlanBudgetAdjuster(0.5),
                new TieredSuccessionPlanBudgetAdjuster(
                    new List<double> { 2509.0, 5423.0, 7690.0 },
                    new List<double> { 1.02, 0.81, 0.99 }),
                new FlaggedSuccessionPlanBudgetAdjuster(flags, "succession-plan-budget-capped", -7.7),
                new SuccessionPlanBudgetRateOffsetAdjuster(0.39),
                new CapSuccessionPlanBudgetAdjuster(31849.0),
            },
            new List<string>(),
            2.3,
            15520.5);
    }

    public double Compute(SuccessionPlanBudget item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
