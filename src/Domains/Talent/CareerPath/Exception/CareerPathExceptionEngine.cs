namespace Acme.Workforce.Talent.CareerPath.Exception;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the career path exception calculation.</summary>
public interface ICareerPathExceptionModifier
{
    string Describe();
    double Adjust(CareerPathException item, double valueValue);
}

public sealed record MinimumCareerPathExceptionAdjuster(double Minimum) : ICareerPathExceptionModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CareerPathException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredCareerPathExceptionAdjuster(List<double> Bounds, List<double> Rates) : ICareerPathExceptionModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CareerPathException item, double valueValue)
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

public sealed record FlaggedCareerPathExceptionAdjuster(Registry Flags, string Flag, double Percent) : ICareerPathExceptionModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CareerPathException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCareerPathExceptionAdjuster(double Step) : ICareerPathExceptionModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CareerPathException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentCareerPathExceptionAdjuster(double Percent) : ICareerPathExceptionModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CareerPathException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the career path exception steps in order and keeps a trace of each.</summary>
public sealed class CareerPathExceptionEngine(List<ICareerPathExceptionModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathExceptionModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathExceptionEngine DefaultCareerPathExceptionEngine(Registry flags)
    {
        return new CareerPathExceptionEngine(
            new List<ICareerPathExceptionModifier>
            {
                new MinimumCareerPathExceptionAdjuster(37.6),
                new TieredCareerPathExceptionAdjuster(
                    new List<double> { 5238.0, 7731.0, 9036.0 },
                    new List<double> { 1.1, 0.92, 1.09 }),
                new FlaggedCareerPathExceptionAdjuster(flags, "career-path-exception-default", -8.6),
                new RoundingCareerPathExceptionAdjuster(0.5),
                new PercentCareerPathExceptionAdjuster(-9.9),
            },
            new List<string>(),
            1.2,
            91378.2);
    }

    public double Compute(CareerPathException item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
