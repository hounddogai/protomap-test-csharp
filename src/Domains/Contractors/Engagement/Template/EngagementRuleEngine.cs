namespace Acme.Workforce.Contractors.Engagement.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the engagement rule calculation.</summary>
public interface IEngagementRuleAdjuster
{
    string Describe();
    double Adjust(EngagementRule item, double valueValue);
}

public sealed record NameEngagementRuleAdjuster(Dictionary<string, double> Factors) : IEngagementRuleAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(EngagementRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedEngagementRuleAdjuster(Registry Flags, string Flag, double Percent) : IEngagementRuleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EngagementRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentEngagementRuleAdjuster(double Percent) : IEngagementRuleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EngagementRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredEngagementRuleAdjuster(List<double> Bounds, List<double> Rates) : IEngagementRuleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EngagementRule item, double valueValue)
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

public sealed record MinimumEngagementRuleAdjuster(double Minimum) : IEngagementRuleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EngagementRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingEngagementRuleAdjuster(double Step) : IEngagementRuleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(EngagementRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the engagement rule steps in order and keeps a trace of each.</summary>
public sealed class EngagementRuleEngine(List<IEngagementRuleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementRuleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementRuleEngine DefaultEngagementRuleEngine(Registry flags)
    {
        return new EngagementRuleEngine(
            new List<IEngagementRuleAdjuster>
            {
                new NameEngagementRuleAdjuster(new Dictionary<string, double> { ["west"] = 1.07 }),
                new FlaggedEngagementRuleAdjuster(flags, "engagement-rule-tiered", 9.6),
                new PercentEngagementRuleAdjuster(1.0),
                new TieredEngagementRuleAdjuster(
                    new List<double> { 3089.0, 4476.0, 6416.0, 8398.0 },
                    new List<double> { 1.16, 0.98, 0.84, 0.81 }),
                new MinimumEngagementRuleAdjuster(46.1),
                new RoundingEngagementRuleAdjuster(0.05),
            },
            new List<string>(),
            2.1,
            95447.6);
    }

    public double Compute(EngagementRule item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<EngagementRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
