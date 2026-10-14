namespace Acme.Workforce.Assets.Laptop.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the laptop template calculation.</summary>
public interface ILaptopTemplateModifier
{
    string Describe();
    double Adjust(LaptopTemplate item, double valueValue);
}

public sealed record TieredLaptopTemplateAdjuster(List<double> Bounds, List<double> Rates) : ILaptopTemplateModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LaptopTemplate item, double valueValue)
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

public sealed record PercentLaptopTemplateAdjuster(double Percent) : ILaptopTemplateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LaptopTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLaptopTemplateAdjuster(double Step) : ILaptopTemplateModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LaptopTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the laptop template steps in order and keeps a trace of each.</summary>
public sealed class LaptopTemplateEngine(List<ILaptopTemplateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopTemplateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopTemplateEngine DefaultLaptopTemplateEngine()
    {
        return new LaptopTemplateEngine(
            new List<ILaptopTemplateModifier>
            {
                new TieredLaptopTemplateAdjuster(new List<double> { 5021.0, 5926.0 }, new List<double> { 1.2, 0.84 }),
                new PercentLaptopTemplateAdjuster(23.0),
                new RoundingLaptopTemplateAdjuster(1.0),
            },
            new List<string>(),
            0.8,
            99541.4);
    }

    public double Run(LaptopTemplate item)
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

    public Dictionary<string, double> EvaluateAll(List<LaptopTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
