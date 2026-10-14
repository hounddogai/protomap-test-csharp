namespace Acme.Workforce.Absence.Balance.BalanceTarget;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the balance mapping calculation.</summary>
public interface IBalanceMappingModifier
{
    string Describe();
    double Adjust(BalanceMapping item, double valueValue);
}

public sealed record TieredBalanceMappingAdjuster(List<double> Bounds, List<double> Rates) : IBalanceMappingModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BalanceMapping item, double valueValue)
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

public sealed record MinimumBalanceMappingAdjuster(double Minimum) : IBalanceMappingModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BalanceMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentBalanceMappingAdjuster(double Percent) : IBalanceMappingModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BalanceMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingBalanceMappingAdjuster(double Step) : IBalanceMappingModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BalanceMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitleBalanceMappingAdjuster(Dictionary<string, double> Factors) : IBalanceMappingModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(BalanceMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the balance mapping steps in order and keeps a trace of each.</summary>
public sealed class BalanceMappingEngine(List<IBalanceMappingModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBalanceMappingModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BalanceMappingEngine DefaultBalanceMappingEngine()
    {
        return new BalanceMappingEngine(
            new List<IBalanceMappingModifier>
            {
                new TieredBalanceMappingAdjuster(new List<double> { 370.0, 5396.0 }, new List<double> { 0.85, 0.97 }),
                new MinimumBalanceMappingAdjuster(20.8),
                new PercentBalanceMappingAdjuster(-1.3),
                new RoundingBalanceMappingAdjuster(5.0),
                new TitleBalanceMappingAdjuster(new Dictionary<string, double> { ["west"] = 0.9 }),
            },
            new List<string>(),
            3.8,
            38958.6);
    }

    public double Run(BalanceMapping item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<BalanceMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
