namespace Acme.Workforce.Payrollops.Deduction.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict deduction rule calculation.</summary>
public interface IStrictDeductionRuleAdjuster
{
    string Describe();
    double Adjust(StrictDeductionRule item, double valueValue);
}

public sealed record LabelStrictDeductionRuleAdjuster(Dictionary<string, double> Factors) : IStrictDeductionRuleAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(StrictDeductionRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record StrictDeductionRuleDurationMinutesOffsetAdjuster(double Weight) : IStrictDeductionRuleAdjuster
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(StrictDeductionRule item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record RoundingStrictDeductionRuleAdjuster(double Step) : IStrictDeductionRuleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictDeductionRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentStrictDeductionRuleAdjuster(double Percent) : IStrictDeductionRuleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictDeductionRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredStrictDeductionRuleAdjuster(List<double> Bounds, List<double> Rates) : IStrictDeductionRuleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictDeductionRule item, double valueValue)
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

/// <summary>Runs the strict deduction rule steps in order and keeps a trace of each.</summary>
public sealed class StrictDeductionRuleEngine(List<IStrictDeductionRuleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictDeductionRuleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictDeductionRuleEngine DefaultStrictDeductionRuleEngine()
    {
        return new StrictDeductionRuleEngine(
            new List<IStrictDeductionRuleAdjuster>
            {
                new LabelStrictDeductionRuleAdjuster(new Dictionary<string, double> { ["west"] = 0.98 }),
                new StrictDeductionRuleDurationMinutesOffsetAdjuster(1.13),
                new RoundingStrictDeductionRuleAdjuster(0.25),
                new PercentStrictDeductionRuleAdjuster(-23.8),
                new TieredStrictDeductionRuleAdjuster(
                    new List<double> { 2889.0, 4766.0, 5917.0, 9272.0 },
                    new List<double> { 0.98, 0.87, 1.06, 0.92 }),
            },
            new List<string>(),
            2.5,
            16456.2);
    }

    public double Run(StrictDeductionRule item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictDeductionRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
