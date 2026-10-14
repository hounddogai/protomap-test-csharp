namespace Acme.Workforce.Assets.Laptop.Adjustment;

/// <summary>One step of the laptop rule calculation.</summary>
public interface ILaptopRuleModifier
{
    string Describe();
    double Adjust(LaptopRule item, double valueValue);
}

public sealed record MinimumLaptopRuleAdjuster(double Minimum) : ILaptopRuleModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LaptopRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLaptopRuleAdjuster(double Percent) : ILaptopRuleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LaptopRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLaptopRuleAdjuster(double Cap) : ILaptopRuleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LaptopRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredLaptopRuleAdjuster(List<double> Bounds, List<double> Rates) : ILaptopRuleModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LaptopRule item, double valueValue)
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

/// <summary>Runs the laptop rule steps in order and keeps a trace of each.</summary>
public sealed class LaptopRuleEngine(List<ILaptopRuleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopRuleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopRuleEngine DefaultLaptopRuleEngine()
    {
        return new LaptopRuleEngine(
            new List<ILaptopRuleModifier>
            {
                new MinimumLaptopRuleAdjuster(36.5),
                new PercentLaptopRuleAdjuster(9.9),
                new CapLaptopRuleAdjuster(14645.0),
                new TieredLaptopRuleAdjuster(
                    new List<double> { 460.0, 1235.0, 4822.0 },
                    new List<double> { 0.88, 0.85, 0.9 }),
            },
            new List<string>(),
            0.9,
            30352.8);
    }

    public double Run(LaptopRule item)
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

    public Dictionary<string, double> EvaluateAll(List<LaptopRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
