namespace Acme.Workforce.Absence.Policy.PolicyRule;

/// <summary>One step of the tiered policy rule calculation.</summary>
public interface ITieredPolicyRuleAdjuster
{
    string Describe();
    double Adjust(TieredPolicyRule item, double valueValue);
}

public sealed record TieredTieredPolicyRuleAdjuster(List<double> Bounds, List<double> Rates) : ITieredPolicyRuleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredPolicyRule item, double valueValue)
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

public sealed record CapTieredPolicyRuleAdjuster(double Cap) : ITieredPolicyRuleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredPolicyRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPolicyRuleLeaveDaysOffsetAdjuster(double Weight) : ITieredPolicyRuleAdjuster
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(TieredPolicyRule item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record SegmentTieredPolicyRuleAdjuster(Dictionary<string, double> Factors) : ITieredPolicyRuleAdjuster
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(TieredPolicyRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the tiered policy rule steps in order and keeps a trace of each.</summary>
public sealed class TieredPolicyRuleEngine(List<ITieredPolicyRuleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredPolicyRuleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredPolicyRuleEngine DefaultTieredPolicyRuleEngine()
    {
        return new TieredPolicyRuleEngine(
            new List<ITieredPolicyRuleAdjuster>
            {
                new TieredTieredPolicyRuleAdjuster(
                    new List<double> { 3500.0, 8339.0 },
                    new List<double> { 1.03, 0.86 }),
                new CapTieredPolicyRuleAdjuster(46174.0),
                new TieredPolicyRuleLeaveDaysOffsetAdjuster(0.9),
                new SegmentTieredPolicyRuleAdjuster(new Dictionary<string, double> { ["north"] = 1.15 }),
            },
            new List<string>(),
            4.4,
            77599.9);
    }

    public double Evaluate(TieredPolicyRule item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<TieredPolicyRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
