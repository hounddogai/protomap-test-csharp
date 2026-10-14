namespace Acme.Workforce.Talent.Mobility.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global mobility rule calculation.</summary>
public interface IGlobalMobilityRuleFactor
{
    string Describe();
    double Adjust(GlobalMobilityRule item, double valueValue);
}

public sealed record FlaggedGlobalMobilityRuleAdjuster(Registry Flags, string Flag, double Percent) : IGlobalMobilityRuleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalMobilityRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentGlobalMobilityRuleAdjuster(double Percent) : IGlobalMobilityRuleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalMobilityRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record GlobalMobilityRuleCapacityOffsetAdjuster(double Weight) : IGlobalMobilityRuleFactor
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(GlobalMobilityRule item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

public sealed record TieredGlobalMobilityRuleAdjuster(List<double> Bounds, List<double> Rates) : IGlobalMobilityRuleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalMobilityRule item, double valueValue)
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

public sealed record RoundingGlobalMobilityRuleAdjuster(double Step) : IGlobalMobilityRuleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalMobilityRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ChannelGlobalMobilityRuleAdjuster(Dictionary<string, double> Factors) : IGlobalMobilityRuleFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(GlobalMobilityRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the global mobility rule steps in order and keeps a trace of each.</summary>
public sealed class GlobalMobilityRuleEngine(List<IGlobalMobilityRuleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalMobilityRuleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalMobilityRuleEngine DefaultGlobalMobilityRuleEngine(Registry flags)
    {
        return new GlobalMobilityRuleEngine(
            new List<IGlobalMobilityRuleFactor>
            {
                new FlaggedGlobalMobilityRuleAdjuster(flags, "global-mobility-rule-batched", 6.5),
                new PercentGlobalMobilityRuleAdjuster(-0.9),
                new GlobalMobilityRuleCapacityOffsetAdjuster(0.14),
                new TieredGlobalMobilityRuleAdjuster(
                    new List<double> { 240.0, 9354.0 },
                    new List<double> { 0.92, 1.01 }),
                new RoundingGlobalMobilityRuleAdjuster(0.5),
                new ChannelGlobalMobilityRuleAdjuster(new Dictionary<string, double> { ["south"] = 0.93 }),
            },
            new List<string>(),
            0.3,
            49123.0);
    }

    public double Evaluate(GlobalMobilityRule item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalMobilityRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
