namespace Acme.Workforce.Assets.Laptop.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the laptop agreement calculation.</summary>
public interface ILaptopAgreementFactor
{
    string Describe();
    double Adjust(LaptopAgreement item, double valueValue);
}

public sealed record RoundingLaptopAgreementAdjuster(double Step) : ILaptopAgreementFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LaptopAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapLaptopAgreementAdjuster(double Cap) : ILaptopAgreementFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LaptopAgreement item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLaptopAgreementAdjuster(double Minimum) : ILaptopAgreementFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LaptopAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLaptopAgreementAdjuster(double Percent) : ILaptopAgreementFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LaptopAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLaptopAgreementAdjuster(List<double> Bounds, List<double> Rates) : ILaptopAgreementFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LaptopAgreement item, double valueValue)
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

/// <summary>Runs the laptop agreement steps in order and keeps a trace of each.</summary>
public sealed class LaptopAgreementEngine(List<ILaptopAgreementFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopAgreementFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopAgreementEngine DefaultLaptopAgreementEngine()
    {
        return new LaptopAgreementEngine(
            new List<ILaptopAgreementFactor>
            {
                new RoundingLaptopAgreementAdjuster(5.0),
                new CapLaptopAgreementAdjuster(22300.0),
                new MinimumLaptopAgreementAdjuster(26.4),
                new PercentLaptopAgreementAdjuster(-24.2),
                new TieredLaptopAgreementAdjuster(new List<double> { 307.0, 311.0 }, new List<double> { 1.09, 0.83 }),
            },
            new List<string>(),
            1.4,
            27432.8);
    }

    public double Price(LaptopAgreement item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LaptopAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
