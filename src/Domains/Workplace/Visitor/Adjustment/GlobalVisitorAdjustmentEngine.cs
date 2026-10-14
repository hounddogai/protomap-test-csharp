namespace Acme.Workforce.Workplace.Visitor.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global visitor adjustment calculation.</summary>
public interface IGlobalVisitorAdjustmentModifier
{
    string Describe();
    double Adjust(GlobalVisitorAdjustment item, double valueValue);
}

public sealed record TieredGlobalVisitorAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IGlobalVisitorAdjustmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalVisitorAdjustment item, double valueValue)
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

public sealed record MinimumGlobalVisitorAdjustmentAdjuster(double Minimum) : IGlobalVisitorAdjustmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalVisitorAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentGlobalVisitorAdjustmentAdjuster(double Percent) : IGlobalVisitorAdjustmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalVisitorAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingGlobalVisitorAdjustmentAdjuster(double Step) : IGlobalVisitorAdjustmentModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalVisitorAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the global visitor adjustment steps in order and keeps a trace of each.</summary>
public sealed class GlobalVisitorAdjustmentEngine(List<IGlobalVisitorAdjustmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalVisitorAdjustmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalVisitorAdjustmentEngine DefaultGlobalVisitorAdjustmentEngine()
    {
        return new GlobalVisitorAdjustmentEngine(
            new List<IGlobalVisitorAdjustmentModifier>
            {
                new TieredGlobalVisitorAdjustmentAdjuster(
                    new List<double> { 3112.0, 5985.0 },
                    new List<double> { 0.96, 1.03 }),
                new MinimumGlobalVisitorAdjustmentAdjuster(40.9),
                new PercentGlobalVisitorAdjustmentAdjuster(11.9),
                new RoundingGlobalVisitorAdjustmentAdjuster(1.0),
            },
            new List<string>(),
            0.2,
            23880.8);
    }

    public double Price(GlobalVisitorAdjustment item)
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

    public Dictionary<string, double> EvaluateAll(List<GlobalVisitorAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
