namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global adjustment rule calculation.</summary>
public interface IGlobalAdjustmentRuleAdjuster
{
    string Describe();
    double Adjust(GlobalAdjustmentRule item, double valueValue);
}

public sealed record GlobalAdjustmentRuleEndsAtOffsetAdjuster(double Weight) : IGlobalAdjustmentRuleAdjuster
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(GlobalAdjustmentRule item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record LabelGlobalAdjustmentRuleAdjuster(Dictionary<string, double> Factors) : IGlobalAdjustmentRuleAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(GlobalAdjustmentRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingGlobalAdjustmentRuleAdjuster(double Step) : IGlobalAdjustmentRuleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalAdjustmentRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the global adjustment rule steps in order and keeps a trace of each.</summary>
public sealed class GlobalAdjustmentRuleEngine(List<IGlobalAdjustmentRuleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalAdjustmentRuleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalAdjustmentRuleEngine DefaultGlobalAdjustmentRuleEngine()
    {
        return new GlobalAdjustmentRuleEngine(
            new List<IGlobalAdjustmentRuleAdjuster>
            {
                new GlobalAdjustmentRuleEndsAtOffsetAdjuster(1.18),
                new LabelGlobalAdjustmentRuleAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.03, ["central"] = 0.8 }),
                new RoundingGlobalAdjustmentRuleAdjuster(0.5),
            },
            new List<string>(),
            3.5,
            2828.7);
    }

    public double Compute(GlobalAdjustmentRule item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalAdjustmentRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
