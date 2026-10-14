namespace Acme.Workforce.Contractors.Offboarding.Batch;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the offboarding rule calculation.</summary>
public interface IOffboardingRuleModifier
{
    string Describe();
    double Adjust(OffboardingRule item, double valueValue);
}

public sealed record FlaggedOffboardingRuleAdjuster(Registry Flags, string Flag, double Percent) : IOffboardingRuleModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(OffboardingRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumOffboardingRuleAdjuster(double Minimum) : IOffboardingRuleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(OffboardingRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentOffboardingRuleAdjuster(double Percent) : IOffboardingRuleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OffboardingRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapOffboardingRuleAdjuster(double Cap) : IOffboardingRuleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(OffboardingRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredOffboardingRuleAdjuster(List<double> Bounds, List<double> Rates) : IOffboardingRuleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OffboardingRule item, double valueValue)
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

public sealed record LabelOffboardingRuleAdjuster(Dictionary<string, double> Factors) : IOffboardingRuleModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(OffboardingRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the offboarding rule steps in order and keeps a trace of each.</summary>
public sealed class OffboardingRuleEngine(List<IOffboardingRuleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOffboardingRuleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OffboardingRuleEngine DefaultOffboardingRuleEngine(Registry flags)
    {
        return new OffboardingRuleEngine(
            new List<IOffboardingRuleModifier>
            {
                new FlaggedOffboardingRuleAdjuster(flags, "offboarding-rule-tiered", 6.7),
                new MinimumOffboardingRuleAdjuster(11.8),
                new PercentOffboardingRuleAdjuster(-7.7),
                new CapOffboardingRuleAdjuster(43959.0),
                new TieredOffboardingRuleAdjuster(
                    new List<double> { 2355.0, 5431.0, 8802.0 },
                    new List<double> { 0.94, 1.06, 0.87 }),
                new LabelOffboardingRuleAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.91, ["central"] = 1.15 }),
            },
            new List<string>(),
            1.0,
            39861.5);
    }

    public double Evaluate(OffboardingRule item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<OffboardingRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
