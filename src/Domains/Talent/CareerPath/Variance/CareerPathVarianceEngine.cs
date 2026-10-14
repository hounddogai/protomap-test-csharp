namespace Acme.Workforce.Talent.CareerPath.Variance;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the career path variance calculation.</summary>
public interface ICareerPathVarianceAdjuster
{
    string Describe();
    double Adjust(CareerPathVariance item, double valueValue);
}

public sealed record MinimumCareerPathVarianceAdjuster(double Minimum) : ICareerPathVarianceAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CareerPathVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CareerPathVarianceWindowDaysOffsetAdjuster(double Weight) : ICareerPathVarianceAdjuster
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(CareerPathVariance item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record PercentCareerPathVarianceAdjuster(double Percent) : ICareerPathVarianceAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CareerPathVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapCareerPathVarianceAdjuster(double Cap) : ICareerPathVarianceAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CareerPathVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedCareerPathVarianceAdjuster(Registry Flags, string Flag, double Percent) : ICareerPathVarianceAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CareerPathVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CategoryCareerPathVarianceAdjuster(Dictionary<string, double> Factors) : ICareerPathVarianceAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(CareerPathVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the career path variance steps in order and keeps a trace of each.</summary>
public sealed class CareerPathVarianceEngine(List<ICareerPathVarianceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathVarianceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathVarianceEngine DefaultCareerPathVarianceEngine(Registry flags)
    {
        return new CareerPathVarianceEngine(
            new List<ICareerPathVarianceAdjuster>
            {
                new MinimumCareerPathVarianceAdjuster(16.1),
                new CareerPathVarianceWindowDaysOffsetAdjuster(0.73),
                new PercentCareerPathVarianceAdjuster(-21.3),
                new CapCareerPathVarianceAdjuster(18096.0),
                new FlaggedCareerPathVarianceAdjuster(flags, "career-path-variance-pending", -2.2),
                new CategoryCareerPathVarianceAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.29, ["online"] = 1.22 }),
            },
            new List<string>(),
            0.5,
            28106.7);
    }

    public double Evaluate(CareerPathVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
