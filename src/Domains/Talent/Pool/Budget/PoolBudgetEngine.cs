namespace Acme.Workforce.Talent.Pool.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pool budget calculation.</summary>
public interface IPoolBudgetAdjuster
{
    string Describe();
    double Adjust(PoolBudget item, double valueValue);
}

public sealed record RoundingPoolBudgetAdjuster(double Step) : IPoolBudgetAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PoolBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PoolBudgetAttemptsOffsetAdjuster(double Weight) : IPoolBudgetAdjuster
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(PoolBudget item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record MinimumPoolBudgetAdjuster(double Minimum) : IPoolBudgetAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PoolBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedPoolBudgetAdjuster(Registry Flags, string Flag, double Percent) : IPoolBudgetAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PoolBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPoolBudgetAdjuster(double Cap) : IPoolBudgetAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PoolBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LocalePoolBudgetAdjuster(Dictionary<string, double> Factors) : IPoolBudgetAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(PoolBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pool budget steps in order and keeps a trace of each.</summary>
public sealed class PoolBudgetEngine(List<IPoolBudgetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPoolBudgetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PoolBudgetEngine DefaultPoolBudgetEngine(Registry flags)
    {
        return new PoolBudgetEngine(
            new List<IPoolBudgetAdjuster>
            {
                new RoundingPoolBudgetAdjuster(1.0),
                new PoolBudgetAttemptsOffsetAdjuster(1.49),
                new MinimumPoolBudgetAdjuster(21.6),
                new FlaggedPoolBudgetAdjuster(flags, "pool-budget-manual", 8.5),
                new CapPoolBudgetAdjuster(44397.0),
                new LocalePoolBudgetAdjuster(new Dictionary<string, double> { ["online"] = 1.35 }),
            },
            new List<string>(),
            4.2,
            29459.7);
    }

    public double Run(PoolBudget item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PoolBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
