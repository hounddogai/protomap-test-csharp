namespace Acme.Workforce.Orgdesign.Reorg.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict reorg budget calculation.</summary>
public interface IStrictReorgBudgetStep
{
    string Describe();
    double Adjust(StrictReorgBudget item, double valueValue);
}

public sealed record ChannelStrictReorgBudgetAdjuster(Dictionary<string, double> Factors) : IStrictReorgBudgetStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(StrictReorgBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumStrictReorgBudgetAdjuster(double Minimum) : IStrictReorgBudgetStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictReorgBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredStrictReorgBudgetAdjuster(List<double> Bounds, List<double> Rates) : IStrictReorgBudgetStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictReorgBudget item, double valueValue)
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

public sealed record FlaggedStrictReorgBudgetAdjuster(Registry Flags, string Flag, double Percent) : IStrictReorgBudgetStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictReorgBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the strict reorg budget steps in order and keeps a trace of each.</summary>
public sealed class StrictReorgBudgetEngine(List<IStrictReorgBudgetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictReorgBudgetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictReorgBudgetEngine DefaultStrictReorgBudgetEngine(Registry flags)
    {
        return new StrictReorgBudgetEngine(
            new List<IStrictReorgBudgetStep>
            {
                new ChannelStrictReorgBudgetAdjuster(new Dictionary<string, double> { ["west"] = 1.02 }),
                new MinimumStrictReorgBudgetAdjuster(33.4),
                new TieredStrictReorgBudgetAdjuster(
                    new List<double> { 1692.0, 3585.0 },
                    new List<double> { 1.07, 1.07 }),
                new FlaggedStrictReorgBudgetAdjuster(flags, "strict-reorg-budget-legacy", -7.8),
            },
            new List<string>(),
            3.5,
            56751.5);
    }

    public double Price(StrictReorgBudget item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<StrictReorgBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
