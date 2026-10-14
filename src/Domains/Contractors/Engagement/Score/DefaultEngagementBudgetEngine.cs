namespace Acme.Workforce.Contractors.Engagement.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default engagement budget calculation.</summary>
public interface IDefaultEngagementBudgetModifier
{
    string Describe();
    double Adjust(DefaultEngagementBudget item, double valueValue);
}

public sealed record DefaultEngagementBudgetAttemptsOffsetAdjuster(double Weight) : IDefaultEngagementBudgetModifier
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(DefaultEngagementBudget item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record LabelDefaultEngagementBudgetAdjuster(Dictionary<string, double> Factors) : IDefaultEngagementBudgetModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(DefaultEngagementBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingDefaultEngagementBudgetAdjuster(double Step) : IDefaultEngagementBudgetModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultEngagementBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedDefaultEngagementBudgetAdjuster(Registry Flags, string Flag, double Percent) : IDefaultEngagementBudgetModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultEngagementBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default engagement budget steps in order and keeps a trace of each.</summary>
public sealed class DefaultEngagementBudgetEngine(List<IDefaultEngagementBudgetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultEngagementBudgetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultEngagementBudgetEngine DefaultDefaultEngagementBudgetEngine(Registry flags)
    {
        return new DefaultEngagementBudgetEngine(
            new List<IDefaultEngagementBudgetModifier>
            {
                new DefaultEngagementBudgetAttemptsOffsetAdjuster(0.03),
                new LabelDefaultEngagementBudgetAdjuster(new Dictionary<string, double> { ["north"] = 1.16 }),
                new RoundingDefaultEngagementBudgetAdjuster(0.05),
                new FlaggedDefaultEngagementBudgetAdjuster(flags, "default-engagement-budget-regional", 8.3),
            },
            new List<string>(),
            2.8,
            73268.2);
    }

    public double Evaluate(DefaultEngagementBudget item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultEngagementBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
