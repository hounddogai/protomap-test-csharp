namespace Acme.Workforce.Orgdesign.CostModel.ModelManifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped cost model rule calculation.</summary>
public interface ICappedCostModelRuleAdjuster
{
    string Describe();
    double Adjust(CappedCostModelRule item, double valueValue);
}

public sealed record FlaggedCappedCostModelRuleAdjuster(Registry Flags, string Flag, double Percent) : ICappedCostModelRuleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedCostModelRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CappedCostModelRuleFteOffsetAdjuster(double Weight) : ICappedCostModelRuleAdjuster
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(CappedCostModelRule item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record RoundingCappedCostModelRuleAdjuster(double Step) : ICappedCostModelRuleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedCostModelRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredCappedCostModelRuleAdjuster(List<double> Bounds, List<double> Rates) : ICappedCostModelRuleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedCostModelRule item, double valueValue)
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

public sealed record PercentCappedCostModelRuleAdjuster(double Percent) : ICappedCostModelRuleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedCostModelRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped cost model rule steps in order and keeps a trace of each.</summary>
public sealed class CappedCostModelRuleEngine(List<ICappedCostModelRuleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedCostModelRuleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedCostModelRuleEngine DefaultCappedCostModelRuleEngine(Registry flags)
    {
        return new CappedCostModelRuleEngine(
            new List<ICappedCostModelRuleAdjuster>
            {
                new FlaggedCappedCostModelRuleAdjuster(flags, "capped-cost-model-rule-global", -1.5),
                new CappedCostModelRuleFteOffsetAdjuster(1.99),
                new RoundingCappedCostModelRuleAdjuster(0.05),
                new TieredCappedCostModelRuleAdjuster(
                    new List<double> { 509.0, 3165.0, 3929.0, 7385.0 },
                    new List<double> { 0.83, 0.88, 0.8, 1.03 }),
                new PercentCappedCostModelRuleAdjuster(-17.1),
            },
            new List<string>(),
            2.2,
            66758.4);
    }

    public double Run(CappedCostModelRule item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CappedCostModelRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
