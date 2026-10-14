namespace Acme.Workforce.Assets.Laptop.Draft;

/// <summary>One step of the laptop mapping calculation.</summary>
public interface ILaptopMappingFactor
{
    string Describe();
    double Adjust(LaptopMapping item, double valueValue);
}

public sealed record MinimumLaptopMappingAdjuster(double Minimum) : ILaptopMappingFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LaptopMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredLaptopMappingAdjuster(List<double> Bounds, List<double> Rates) : ILaptopMappingFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LaptopMapping item, double valueValue)
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

public sealed record CapLaptopMappingAdjuster(double Cap) : ILaptopMappingFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LaptopMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentLaptopMappingAdjuster(double Percent) : ILaptopMappingFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LaptopMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentLaptopMappingAdjuster(Dictionary<string, double> Factors) : ILaptopMappingFactor
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(LaptopMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the laptop mapping steps in order and keeps a trace of each.</summary>
public sealed class LaptopMappingEngine(List<ILaptopMappingFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopMappingFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopMappingEngine DefaultLaptopMappingEngine()
    {
        return new LaptopMappingEngine(
            new List<ILaptopMappingFactor>
            {
                new MinimumLaptopMappingAdjuster(19.9),
                new TieredLaptopMappingAdjuster(
                    new List<double> { 2083.0, 7756.0, 8657.0 },
                    new List<double> { 0.97, 0.86, 1.05 }),
                new CapLaptopMappingAdjuster(26757.0),
                new PercentLaptopMappingAdjuster(18.6),
                new DepartmentLaptopMappingAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.09, ["retail"] = 0.89 }),
            },
            new List<string>(),
            3.0,
            49723.6);
    }

    public double Run(LaptopMapping item)
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

    public Dictionary<string, double> EvaluateAll(List<LaptopMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
