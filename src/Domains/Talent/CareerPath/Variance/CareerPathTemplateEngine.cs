namespace Acme.Workforce.Talent.CareerPath.Variance;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the career path template calculation.</summary>
public interface ICareerPathTemplateStep
{
    string Describe();
    double Adjust(CareerPathTemplate item, double valueValue);
}

public sealed record MinimumCareerPathTemplateAdjuster(double Minimum) : ICareerPathTemplateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CareerPathTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CareerPathTemplateRateOffsetAdjuster(double Weight) : ICareerPathTemplateStep
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(CareerPathTemplate item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record PercentCareerPathTemplateAdjuster(double Percent) : ICareerPathTemplateStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CareerPathTemplate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedCareerPathTemplateAdjuster(Registry Flags, string Flag, double Percent) : ICareerPathTemplateStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CareerPathTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the career path template steps in order and keeps a trace of each.</summary>
public sealed class CareerPathTemplateEngine(List<ICareerPathTemplateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathTemplateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathTemplateEngine DefaultCareerPathTemplateEngine(Registry flags)
    {
        return new CareerPathTemplateEngine(
            new List<ICareerPathTemplateStep>
            {
                new MinimumCareerPathTemplateAdjuster(30.9),
                new CareerPathTemplateRateOffsetAdjuster(0.79),
                new PercentCareerPathTemplateAdjuster(9.0),
                new FlaggedCareerPathTemplateAdjuster(flags, "career-path-template-scheduled", 2.8),
            },
            new List<string>(),
            3.0,
            34927.2);
    }

    public double Run(CareerPathTemplate item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
