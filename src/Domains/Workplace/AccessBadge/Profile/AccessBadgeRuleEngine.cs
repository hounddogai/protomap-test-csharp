namespace Acme.Workforce.Workplace.AccessBadge.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge rule calculation.</summary>
public interface IAccessBadgeRuleFactor
{
    string Describe();
    double Adjust(AccessBadgeRule item, double valueValue);
}

public sealed record TieredAccessBadgeRuleAdjuster(List<double> Bounds, List<double> Rates) : IAccessBadgeRuleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccessBadgeRule item, double valueValue)
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

public sealed record CapAccessBadgeRuleAdjuster(double Cap) : IAccessBadgeRuleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccessBadgeRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumAccessBadgeRuleAdjuster(double Minimum) : IAccessBadgeRuleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccessBadgeRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedAccessBadgeRuleAdjuster(Registry Flags, string Flag, double Percent) : IAccessBadgeRuleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccessBadgeRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAccessBadgeRuleAdjuster(double Step) : IAccessBadgeRuleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccessBadgeRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record AccessBadgeRuleScoreOffsetAdjuster(double Weight) : IAccessBadgeRuleFactor
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(AccessBadgeRule item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

/// <summary>Runs the access badge rule steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeRuleEngine(List<IAccessBadgeRuleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeRuleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeRuleEngine DefaultAccessBadgeRuleEngine(Registry flags)
    {
        return new AccessBadgeRuleEngine(
            new List<IAccessBadgeRuleFactor>
            {
                new TieredAccessBadgeRuleAdjuster(
                    new List<double> { 341.0, 2079.0, 8411.0 },
                    new List<double> { 0.87, 0.88, 0.98 }),
                new CapAccessBadgeRuleAdjuster(1888.0),
                new MinimumAccessBadgeRuleAdjuster(25.2),
                new FlaggedAccessBadgeRuleAdjuster(flags, "access-badge-rule-weighted", 4.9),
                new RoundingAccessBadgeRuleAdjuster(5.0),
                new AccessBadgeRuleScoreOffsetAdjuster(1.78),
            },
            new List<string>(),
            4.0,
            61340.6);
    }

    public double Compute(AccessBadgeRule item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
