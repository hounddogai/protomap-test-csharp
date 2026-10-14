namespace Acme.Workforce.Orgdesign.Reorg.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default reorg plan calculation.</summary>
public interface IDefaultReorgPlanModifier
{
    string Describe();
    double Adjust(DefaultReorgPlan item, double valueValue);
}

public sealed record MinimumDefaultReorgPlanAdjuster(double Minimum) : IDefaultReorgPlanModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultReorgPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentDefaultReorgPlanAdjuster(double Percent) : IDefaultReorgPlanModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultReorgPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedDefaultReorgPlanAdjuster(Registry Flags, string Flag, double Percent) : IDefaultReorgPlanModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultReorgPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapDefaultReorgPlanAdjuster(double Cap) : IDefaultReorgPlanModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultReorgPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredDefaultReorgPlanAdjuster(List<double> Bounds, List<double> Rates) : IDefaultReorgPlanModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultReorgPlan item, double valueValue)
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

/// <summary>Runs the default reorg plan steps in order and keeps a trace of each.</summary>
public sealed class DefaultReorgPlanEngine(List<IDefaultReorgPlanModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultReorgPlanModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultReorgPlanEngine DefaultDefaultReorgPlanEngine(Registry flags)
    {
        return new DefaultReorgPlanEngine(
            new List<IDefaultReorgPlanModifier>
            {
                new MinimumDefaultReorgPlanAdjuster(27.3),
                new PercentDefaultReorgPlanAdjuster(14.2),
                new FlaggedDefaultReorgPlanAdjuster(flags, "default-reorg-plan-batched", -1.5),
                new CapDefaultReorgPlanAdjuster(38535.0),
                new TieredDefaultReorgPlanAdjuster(
                    new List<double> { 5084.0, 5635.0, 7610.0 },
                    new List<double> { 1.17, 1.0, 0.94 }),
            },
            new List<string>(),
            4.7,
            49407.4);
    }

    public double Evaluate(DefaultReorgPlan item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultReorgPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
