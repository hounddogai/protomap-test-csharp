namespace Acme.Workforce.Absence.Accrual.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual rule calculation.</summary>
public interface IAccrualRuleStep
{
    string Describe();
    double Adjust(AccrualRule item, double valueValue);
}

public sealed record TieredAccrualRuleAdjuster(List<double> Bounds, List<double> Rates) : IAccrualRuleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccrualRule item, double valueValue)
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

public sealed record AccrualRulePriorityOffsetAdjuster(double Weight) : IAccrualRuleStep
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(AccrualRule item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record PercentAccrualRuleAdjuster(double Percent) : IAccrualRuleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccrualRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SegmentAccrualRuleAdjuster(Dictionary<string, double> Factors) : IAccrualRuleStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(AccrualRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedAccrualRuleAdjuster(Registry Flags, string Flag, double Percent) : IAccrualRuleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccrualRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAccrualRuleAdjuster(double Minimum) : IAccrualRuleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccrualRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the accrual rule steps in order and keeps a trace of each.</summary>
public sealed class AccrualRuleEngine(List<IAccrualRuleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualRuleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualRuleEngine DefaultAccrualRuleEngine(Registry flags)
    {
        return new AccrualRuleEngine(
            new List<IAccrualRuleStep>
            {
                new TieredAccrualRuleAdjuster(new List<double> { 6777.0, 8354.0 }, new List<double> { 0.81, 0.97 }),
                new AccrualRulePriorityOffsetAdjuster(1.54),
                new PercentAccrualRuleAdjuster(-19.4),
                new SegmentAccrualRuleAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.35, ["east"] = 0.94 }),
                new FlaggedAccrualRuleAdjuster(flags, "accrual-rule-pending", -7.5),
                new MinimumAccrualRuleAdjuster(33.6),
            },
            new List<string>(),
            0.7,
            98338.7);
    }

    public double Run(AccrualRule item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<AccrualRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
