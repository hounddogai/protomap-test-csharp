namespace Acme.Workforce.Compensation.Adjustment.Rule;

/// <summary>One step of the adjustment rule calculation.</summary>
public interface IAdjustmentRuleModifier
{
    string Describe();
    double Adjust(AdjustmentRule item, double valueValue);
}

public sealed record AdjustmentRuleDurationMinutesOffsetAdjuster(double Weight) : IAdjustmentRuleModifier
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(AdjustmentRule item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record TieredAdjustmentRuleAdjuster(List<double> Bounds, List<double> Rates) : IAdjustmentRuleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdjustmentRule item, double valueValue)
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

public sealed record SourceAdjustmentRuleAdjuster(Dictionary<string, double> Factors) : IAdjustmentRuleModifier
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(AdjustmentRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentAdjustmentRuleAdjuster(double Percent) : IAdjustmentRuleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdjustmentRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adjustment rule steps in order and keeps a trace of each.</summary>
public sealed class AdjustmentRuleEngine(List<IAdjustmentRuleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdjustmentRuleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdjustmentRuleEngine DefaultAdjustmentRuleEngine()
    {
        return new AdjustmentRuleEngine(
            new List<IAdjustmentRuleModifier>
            {
                new AdjustmentRuleDurationMinutesOffsetAdjuster(1.47),
                new TieredAdjustmentRuleAdjuster(new List<double> { 5935.0, 8947.0 }, new List<double> { 1.1, 1.16 }),
                new SourceAdjustmentRuleAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.93, ["west"] = 1.18 }),
                new PercentAdjustmentRuleAdjuster(22.9),
            },
            new List<string>(),
            3.1,
            24319.3);
    }

    public double Evaluate(AdjustmentRule item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AdjustmentRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
