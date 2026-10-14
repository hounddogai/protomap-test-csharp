namespace Acme.Workforce.Talent.Pool.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict pool rule calculation.</summary>
public interface IStrictPoolRuleFactor
{
    string Describe();
    double Adjust(StrictPoolRule item, double valueValue);
}

public sealed record RegionStrictPoolRuleAdjuster(Dictionary<string, double> Factors) : IStrictPoolRuleFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(StrictPoolRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumStrictPoolRuleAdjuster(double Minimum) : IStrictPoolRuleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictPoolRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record StrictPoolRuleDurationMinutesOffsetAdjuster(double Weight) : IStrictPoolRuleFactor
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(StrictPoolRule item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record RoundingStrictPoolRuleAdjuster(double Step) : IStrictPoolRuleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictPoolRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredStrictPoolRuleAdjuster(List<double> Bounds, List<double> Rates) : IStrictPoolRuleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictPoolRule item, double valueValue)
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

/// <summary>Runs the strict pool rule steps in order and keeps a trace of each.</summary>
public sealed class StrictPoolRuleEngine(List<IStrictPoolRuleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictPoolRuleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictPoolRuleEngine DefaultStrictPoolRuleEngine()
    {
        return new StrictPoolRuleEngine(
            new List<IStrictPoolRuleFactor>
            {
                new RegionStrictPoolRuleAdjuster(new Dictionary<string, double> { ["online"] = 1.13 }),
                new MinimumStrictPoolRuleAdjuster(40.3),
                new StrictPoolRuleDurationMinutesOffsetAdjuster(0.16),
                new RoundingStrictPoolRuleAdjuster(5.0),
                new TieredStrictPoolRuleAdjuster(
                    new List<double> { 2545.0, 3433.0 },
                    new List<double> { 1.01, 1.13 }),
            },
            new List<string>(),
            3.1,
            7123.4);
    }

    public double Run(StrictPoolRule item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictPoolRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
