namespace Acme.Workforce.Absence.Policy.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback policy budget calculation.</summary>
public interface IFallbackPolicyBudgetStep
{
    string Describe();
    double Adjust(FallbackPolicyBudget item, double valueValue);
}

public sealed record TieredFallbackPolicyBudgetAdjuster(List<double> Bounds, List<double> Rates) : IFallbackPolicyBudgetStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackPolicyBudget item, double valueValue)
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

public sealed record RoundingFallbackPolicyBudgetAdjuster(double Step) : IFallbackPolicyBudgetStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackPolicyBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FallbackPolicyBudgetEndsAtOffsetAdjuster(double Weight) : IFallbackPolicyBudgetStep
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(FallbackPolicyBudget item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record PercentFallbackPolicyBudgetAdjuster(double Percent) : IFallbackPolicyBudgetStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackPolicyBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedFallbackPolicyBudgetAdjuster(Registry Flags, string Flag, double Percent) : IFallbackPolicyBudgetStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackPolicyBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumFallbackPolicyBudgetAdjuster(double Minimum) : IFallbackPolicyBudgetStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackPolicyBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the fallback policy budget steps in order and keeps a trace of each.</summary>
public sealed class FallbackPolicyBudgetEngine(List<IFallbackPolicyBudgetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackPolicyBudgetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackPolicyBudgetEngine DefaultFallbackPolicyBudgetEngine(Registry flags)
    {
        return new FallbackPolicyBudgetEngine(
            new List<IFallbackPolicyBudgetStep>
            {
                new TieredFallbackPolicyBudgetAdjuster(
                    new List<double> { 2003.0, 9087.0 },
                    new List<double> { 0.85, 1.15 }),
                new RoundingFallbackPolicyBudgetAdjuster(5.0),
                new FallbackPolicyBudgetEndsAtOffsetAdjuster(0.54),
                new PercentFallbackPolicyBudgetAdjuster(21.3),
                new FlaggedFallbackPolicyBudgetAdjuster(flags, "fallback-policy-budget-adaptive", -4.5),
                new MinimumFallbackPolicyBudgetAdjuster(8.7),
            },
            new List<string>(),
            2.7,
            52781.9);
    }

    public double Run(FallbackPolicyBudget item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackPolicyBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
