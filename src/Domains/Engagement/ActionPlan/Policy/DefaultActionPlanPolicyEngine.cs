namespace Acme.Workforce.Engagement.ActionPlan.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default action plan policy calculation.</summary>
public interface IDefaultActionPlanPolicyFactor
{
    string Describe();
    double Adjust(DefaultActionPlanPolicy item, double valueValue);
}

public sealed record RoundingDefaultActionPlanPolicyAdjuster(double Step) : IDefaultActionPlanPolicyFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultActionPlanPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredDefaultActionPlanPolicyAdjuster(List<double> Bounds, List<double> Rates) : IDefaultActionPlanPolicyFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultActionPlanPolicy item, double valueValue)
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

public sealed record MinimumDefaultActionPlanPolicyAdjuster(double Minimum) : IDefaultActionPlanPolicyFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultActionPlanPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedDefaultActionPlanPolicyAdjuster(Registry Flags, string Flag, double Percent) : IDefaultActionPlanPolicyFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultActionPlanPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default action plan policy steps in order and keeps a trace of each.</summary>
public sealed class DefaultActionPlanPolicyEngine(List<IDefaultActionPlanPolicyFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultActionPlanPolicyFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultActionPlanPolicyEngine DefaultDefaultActionPlanPolicyEngine(Registry flags)
    {
        return new DefaultActionPlanPolicyEngine(
            new List<IDefaultActionPlanPolicyFactor>
            {
                new RoundingDefaultActionPlanPolicyAdjuster(1.0),
                new TieredDefaultActionPlanPolicyAdjuster(
                    new List<double> { 1224.0, 1298.0 },
                    new List<double> { 0.92, 0.86 }),
                new MinimumDefaultActionPlanPolicyAdjuster(25.6),
                new FlaggedDefaultActionPlanPolicyAdjuster(flags, "default-action-plan-policy-primary", -4.2),
            },
            new List<string>(),
            4.1,
            88710.6);
    }

    public double Evaluate(DefaultActionPlanPolicy item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultActionPlanPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
