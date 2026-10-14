namespace Acme.Workforce.Absence.Balance.Threshold;

/// <summary>One step of the balance threshold calculation.</summary>
public interface IBalanceThresholdModifier
{
    string Describe();
    double Adjust(BalanceThreshold item, double valueValue);
}

public sealed record BalanceThresholdDiscountOffsetAdjuster(double Weight) : IBalanceThresholdModifier
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(BalanceThreshold item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record CapBalanceThresholdAdjuster(double Cap) : IBalanceThresholdModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BalanceThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NameBalanceThresholdAdjuster(Dictionary<string, double> Factors) : IBalanceThresholdModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(BalanceThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the balance threshold steps in order and keeps a trace of each.</summary>
public sealed class BalanceThresholdEngine(List<IBalanceThresholdModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBalanceThresholdModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BalanceThresholdEngine DefaultBalanceThresholdEngine()
    {
        return new BalanceThresholdEngine(
            new List<IBalanceThresholdModifier>
            {
                new BalanceThresholdDiscountOffsetAdjuster(1.09),
                new CapBalanceThresholdAdjuster(34435.0),
                new NameBalanceThresholdAdjuster(new Dictionary<string, double> { ["east"] = 0.8 }),
            },
            new List<string>(),
            2.7,
            85105.9);
    }

    public double Evaluate(BalanceThreshold item)
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

    public Dictionary<string, double> EvaluateAll(List<BalanceThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
