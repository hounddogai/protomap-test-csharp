namespace Acme.Workforce.Talent.CareerPath.Rule;

/// <summary>One step of the career path profile calculation.</summary>
public interface ICareerPathProfileModifier
{
    string Describe();
    double Adjust(CareerPathProfile item, double valueValue);
}

public sealed record CapCareerPathProfileAdjuster(double Cap) : ICareerPathProfileModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CareerPathProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredCareerPathProfileAdjuster(List<double> Bounds, List<double> Rates) : ICareerPathProfileModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CareerPathProfile item, double valueValue)
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

public sealed record MinimumCareerPathProfileAdjuster(double Minimum) : ICareerPathProfileModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CareerPathProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CareerPathProfileRatingOffsetAdjuster(double Weight) : ICareerPathProfileModifier
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(CareerPathProfile item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

/// <summary>Runs the career path profile steps in order and keeps a trace of each.</summary>
public sealed class CareerPathProfileEngine(List<ICareerPathProfileModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathProfileModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathProfileEngine DefaultCareerPathProfileEngine()
    {
        return new CareerPathProfileEngine(
            new List<ICareerPathProfileModifier>
            {
                new CapCareerPathProfileAdjuster(35762.0),
                new TieredCareerPathProfileAdjuster(
                    new List<double> { 5235.0, 6819.0, 7590.0, 8792.0 },
                    new List<double> { 1.13, 0.93, 0.89, 0.91 }),
                new MinimumCareerPathProfileAdjuster(35.5),
                new CareerPathProfileRatingOffsetAdjuster(1.88),
            },
            new List<string>(),
            1.5,
            58331.2);
    }

    public double Run(CareerPathProfile item)
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
