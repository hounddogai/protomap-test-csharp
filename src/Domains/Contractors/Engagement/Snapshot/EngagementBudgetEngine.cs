namespace Acme.Workforce.Contractors.Engagement.Snapshot;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the engagement budget calculation.</summary>
public interface IEngagementBudgetStep
{
    string Describe();
    double Adjust(EngagementBudget item, double valueValue);
}

public sealed record TitleEngagementBudgetAdjuster(Dictionary<string, double> Factors) : IEngagementBudgetStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(EngagementBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record EngagementBudgetBatchSizeOffsetAdjuster(double Weight) : IEngagementBudgetStep
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(EngagementBudget item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record PercentEngagementBudgetAdjuster(double Percent) : IEngagementBudgetStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EngagementBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredEngagementBudgetAdjuster(List<double> Bounds, List<double> Rates) : IEngagementBudgetStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EngagementBudget item, double valueValue)
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

public sealed record FlaggedEngagementBudgetAdjuster(Registry Flags, string Flag, double Percent) : IEngagementBudgetStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EngagementBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapEngagementBudgetAdjuster(double Cap) : IEngagementBudgetStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EngagementBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the engagement budget steps in order and keeps a trace of each.</summary>
public sealed class EngagementBudgetEngine(List<IEngagementBudgetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementBudgetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementBudgetEngine DefaultEngagementBudgetEngine(Registry flags)
    {
        return new EngagementBudgetEngine(
            new List<IEngagementBudgetStep>
            {
                new TitleEngagementBudgetAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.92, ["central"] = 1.24 }),
                new EngagementBudgetBatchSizeOffsetAdjuster(1.51),
                new PercentEngagementBudgetAdjuster(1.0),
                new TieredEngagementBudgetAdjuster(
                    new List<double> { 1326.0, 1801.0, 3541.0, 8818.0 },
                    new List<double> { 1.18, 0.9, 1.16, 0.83 }),
                new FlaggedEngagementBudgetAdjuster(flags, "engagement-budget-scheduled", 2.5),
                new CapEngagementBudgetAdjuster(19576.0),
            },
            new List<string>(),
            1.1,
            84739.4);
    }

    public double Run(EngagementBudget item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<EngagementBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
