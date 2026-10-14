namespace Acme.Workforce.Orgdesign.Reorg.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted reorg rule calculation.</summary>
public interface IWeightedReorgRuleFactor
{
    string Describe();
    double Adjust(WeightedReorgRule item, double valueValue);
}

public sealed record WeightedReorgRuleVersionOffsetAdjuster(double Weight) : IWeightedReorgRuleFactor
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(WeightedReorgRule item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record PercentWeightedReorgRuleAdjuster(double Percent) : IWeightedReorgRuleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedReorgRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedWeightedReorgRuleAdjuster(Registry Flags, string Flag, double Percent) : IWeightedReorgRuleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedReorgRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingWeightedReorgRuleAdjuster(double Step) : IWeightedReorgRuleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedReorgRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the weighted reorg rule steps in order and keeps a trace of each.</summary>
public sealed class WeightedReorgRuleEngine(List<IWeightedReorgRuleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedReorgRuleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedReorgRuleEngine DefaultWeightedReorgRuleEngine(Registry flags)
    {
        return new WeightedReorgRuleEngine(
            new List<IWeightedReorgRuleFactor>
            {
                new WeightedReorgRuleVersionOffsetAdjuster(1.85),
                new PercentWeightedReorgRuleAdjuster(-18.8),
                new FlaggedWeightedReorgRuleAdjuster(flags, "weighted-reorg-rule-default", -8.7),
                new RoundingWeightedReorgRuleAdjuster(5.0),
            },
            new List<string>(),
            4.8,
            25392.4);
    }

    public double Compute(WeightedReorgRule item)
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

    public Dictionary<string, double> EvaluateAll(List<WeightedReorgRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
