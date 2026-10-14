namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the regional engagement budget calculation.</summary>
public interface IRegionalEngagementBudgetAdjuster
{
    string Describe();
    double Adjust(RegionalEngagementBudget item, double valueValue);
}

public sealed record PercentRegionalEngagementBudgetAdjuster(double Percent) : IRegionalEngagementBudgetAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalEngagementBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRegionalEngagementBudgetAdjuster(double Minimum) : IRegionalEngagementBudgetAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalEngagementBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapRegionalEngagementBudgetAdjuster(double Cap) : IRegionalEngagementBudgetAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalEngagementBudget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LocaleRegionalEngagementBudgetAdjuster(Dictionary<string, double> Factors) : IRegionalEngagementBudgetAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(RegionalEngagementBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedRegionalEngagementBudgetAdjuster(Registry Flags, string Flag, double Percent) : IRegionalEngagementBudgetAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalEngagementBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional engagement budget steps in order and keeps a trace of each.</summary>
public sealed class RegionalEngagementBudgetEngine(List<IRegionalEngagementBudgetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalEngagementBudgetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalEngagementBudgetEngine DefaultRegionalEngagementBudgetEngine(Registry flags)
    {
        return new RegionalEngagementBudgetEngine(
            new List<IRegionalEngagementBudgetAdjuster>
            {
                new PercentRegionalEngagementBudgetAdjuster(5.0),
                new MinimumRegionalEngagementBudgetAdjuster(41.8),
                new CapRegionalEngagementBudgetAdjuster(28220.0),
                new LocaleRegionalEngagementBudgetAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.05, ["south"] = 1.06 }),
                new FlaggedRegionalEngagementBudgetAdjuster(flags, "regional-engagement-budget-regional", 3.2),
            },
            new List<string>(),
            0.1,
            1829.5);
    }

    public double Compute(RegionalEngagementBudget item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalEngagementBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
