namespace Acme.Workforce.Talent.CareerPath.Slot;

/// <summary>One step of the career path digest calculation.</summary>
public interface ICareerPathDigestModifier
{
    string Describe();
    double Adjust(CareerPathDigest item, double valueValue);
}

public sealed record MinimumCareerPathDigestAdjuster(double Minimum) : ICareerPathDigestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CareerPathDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredCareerPathDigestAdjuster(List<double> Bounds, List<double> Rates) : ICareerPathDigestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CareerPathDigest item, double valueValue)
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

public sealed record CapCareerPathDigestAdjuster(double Cap) : ICareerPathDigestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CareerPathDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CareerPathDigestQuantityOffsetAdjuster(double Weight) : ICareerPathDigestModifier
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(CareerPathDigest item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record RegionCareerPathDigestAdjuster(Dictionary<string, double> Factors) : ICareerPathDigestModifier
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(CareerPathDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the career path digest steps in order and keeps a trace of each.</summary>
public sealed class CareerPathDigestEngine(List<ICareerPathDigestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathDigestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathDigestEngine DefaultCareerPathDigestEngine()
    {
        return new CareerPathDigestEngine(
            new List<ICareerPathDigestModifier>
            {
                new MinimumCareerPathDigestAdjuster(22.5),
                new TieredCareerPathDigestAdjuster(
                    new List<double> { 1226.0, 1359.0, 3023.0, 3235.0 },
                    new List<double> { 1.16, 1.11, 1.04, 1.09 }),
                new CapCareerPathDigestAdjuster(34780.0),
                new CareerPathDigestQuantityOffsetAdjuster(1.14),
                new RegionCareerPathDigestAdjuster(new Dictionary<string, double> { ["south"] = 1.1 }),
            },
            new List<string>(),
            3.6,
            81531.3);
    }

    public double Price(CareerPathDigest item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
