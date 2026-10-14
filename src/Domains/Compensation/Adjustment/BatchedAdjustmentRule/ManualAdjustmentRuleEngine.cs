namespace Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual adjustment rule calculation.</summary>
public interface IManualAdjustmentRuleAdjuster
{
    string Describe();
    double Adjust(ManualAdjustmentRule item, double valueValue);
}

public sealed record RoundingManualAdjustmentRuleAdjuster(double Step) : IManualAdjustmentRuleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualAdjustmentRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapManualAdjustmentRuleAdjuster(double Cap) : IManualAdjustmentRuleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualAdjustmentRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record DepartmentManualAdjustmentRuleAdjuster(Dictionary<string, double> Factors) : IManualAdjustmentRuleAdjuster
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(ManualAdjustmentRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredManualAdjustmentRuleAdjuster(List<double> Bounds, List<double> Rates) : IManualAdjustmentRuleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualAdjustmentRule item, double valueValue)
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

public sealed record MinimumManualAdjustmentRuleAdjuster(double Minimum) : IManualAdjustmentRuleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualAdjustmentRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the manual adjustment rule steps in order and keeps a trace of each.</summary>
public sealed class ManualAdjustmentRuleEngine(List<IManualAdjustmentRuleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualAdjustmentRuleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualAdjustmentRuleEngine DefaultManualAdjustmentRuleEngine()
    {
        return new ManualAdjustmentRuleEngine(
            new List<IManualAdjustmentRuleAdjuster>
            {
                new RoundingManualAdjustmentRuleAdjuster(0.1),
                new CapManualAdjustmentRuleAdjuster(29683.0),
                new DepartmentManualAdjustmentRuleAdjuster(new Dictionary<string, double> { ["east"] = 1.04 }),
                new TieredManualAdjustmentRuleAdjuster(
                    new List<double> { 1385.0, 7666.0, 9360.0 },
                    new List<double> { 0.98, 1.0, 1.07 }),
                new MinimumManualAdjustmentRuleAdjuster(8.7),
            },
            new List<string>(),
            3.6,
            57296.1);
    }

    public double Run(ManualAdjustmentRule item)
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

    public Dictionary<string, double> EvaluateAll(List<ManualAdjustmentRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
