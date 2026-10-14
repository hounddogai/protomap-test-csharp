namespace Acme.Workforce.Contractors.Engagement.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual engagement rule calculation.</summary>
public interface IManualEngagementRuleAdjuster
{
    string Describe();
    double Adjust(ManualEngagementRule item, double valueValue);
}

public sealed record RoundingManualEngagementRuleAdjuster(double Step) : IManualEngagementRuleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualEngagementRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedManualEngagementRuleAdjuster(Registry Flags, string Flag, double Percent) : IManualEngagementRuleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualEngagementRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredManualEngagementRuleAdjuster(List<double> Bounds, List<double> Rates) : IManualEngagementRuleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualEngagementRule item, double valueValue)
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

public sealed record MinimumManualEngagementRuleAdjuster(double Minimum) : IManualEngagementRuleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualEngagementRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentManualEngagementRuleAdjuster(double Percent) : IManualEngagementRuleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualEngagementRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the manual engagement rule steps in order and keeps a trace of each.</summary>
public sealed class ManualEngagementRuleEngine(List<IManualEngagementRuleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualEngagementRuleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualEngagementRuleEngine DefaultManualEngagementRuleEngine(Registry flags)
    {
        return new ManualEngagementRuleEngine(
            new List<IManualEngagementRuleAdjuster>
            {
                new RoundingManualEngagementRuleAdjuster(0.05),
                new FlaggedManualEngagementRuleAdjuster(flags, "manual-engagement-rule-adaptive", 4.8),
                new TieredManualEngagementRuleAdjuster(
                    new List<double> { 1061.0, 1663.0 },
                    new List<double> { 0.98, 1.11 }),
                new MinimumManualEngagementRuleAdjuster(24.1),
                new PercentManualEngagementRuleAdjuster(8.1),
            },
            new List<string>(),
            1.0,
            13835.9);
    }

    public double Price(ManualEngagementRule item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ManualEngagementRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
