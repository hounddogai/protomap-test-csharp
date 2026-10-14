namespace Acme.Workforce.Talent.Mobility.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy mobility rule calculation.</summary>
public interface ILegacyMobilityRuleStep
{
    string Describe();
    double Adjust(LegacyMobilityRule item, double valueValue);
}

public sealed record MinimumLegacyMobilityRuleAdjuster(double Minimum) : ILegacyMobilityRuleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyMobilityRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLegacyMobilityRuleAdjuster(double Percent) : ILegacyMobilityRuleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyMobilityRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedLegacyMobilityRuleAdjuster(Registry Flags, string Flag, double Percent) : ILegacyMobilityRuleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyMobilityRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLegacyMobilityRuleAdjuster(double Step) : ILegacyMobilityRuleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyMobilityRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the legacy mobility rule steps in order and keeps a trace of each.</summary>
public sealed class LegacyMobilityRuleEngine(List<ILegacyMobilityRuleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyMobilityRuleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyMobilityRuleEngine DefaultLegacyMobilityRuleEngine(Registry flags)
    {
        return new LegacyMobilityRuleEngine(
            new List<ILegacyMobilityRuleStep>
            {
                new MinimumLegacyMobilityRuleAdjuster(16.7),
                new PercentLegacyMobilityRuleAdjuster(-5.4),
                new FlaggedLegacyMobilityRuleAdjuster(flags, "legacy-mobility-rule-adaptive", 8.6),
                new RoundingLegacyMobilityRuleAdjuster(1.0),
            },
            new List<string>(),
            4.2,
            77514.6);
    }

    public double Price(LegacyMobilityRule item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyMobilityRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
