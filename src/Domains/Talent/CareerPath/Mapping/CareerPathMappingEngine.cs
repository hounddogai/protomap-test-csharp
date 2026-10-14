namespace Acme.Workforce.Talent.CareerPath.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the career path mapping calculation.</summary>
public interface ICareerPathMappingStep
{
    string Describe();
    double Adjust(CareerPathMapping item, double valueValue);
}

public sealed record RoundingCareerPathMappingAdjuster(double Step) : ICareerPathMappingStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CareerPathMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumCareerPathMappingAdjuster(double Minimum) : ICareerPathMappingStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CareerPathMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredCareerPathMappingAdjuster(List<double> Bounds, List<double> Rates) : ICareerPathMappingStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CareerPathMapping item, double valueValue)
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

public sealed record FlaggedCareerPathMappingAdjuster(Registry Flags, string Flag, double Percent) : ICareerPathMappingStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CareerPathMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapCareerPathMappingAdjuster(double Cap) : ICareerPathMappingStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CareerPathMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the career path mapping steps in order and keeps a trace of each.</summary>
public sealed class CareerPathMappingEngine(List<ICareerPathMappingStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathMappingStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathMappingEngine DefaultCareerPathMappingEngine(Registry flags)
    {
        return new CareerPathMappingEngine(
            new List<ICareerPathMappingStep>
            {
                new RoundingCareerPathMappingAdjuster(0.25),
                new MinimumCareerPathMappingAdjuster(28.9),
                new TieredCareerPathMappingAdjuster(
                    new List<double> { 150.0, 1185.0, 5308.0, 6632.0 },
                    new List<double> { 0.86, 0.89, 1.1, 0.85 }),
                new FlaggedCareerPathMappingAdjuster(flags, "career-path-mapping-rolling", 8.9),
                new CapCareerPathMappingAdjuster(9542.0),
            },
            new List<string>(),
            3.3,
            71227.9);
    }

    public double Evaluate(CareerPathMapping item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
