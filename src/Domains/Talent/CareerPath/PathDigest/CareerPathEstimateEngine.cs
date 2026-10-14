namespace Acme.Workforce.Talent.CareerPath.PathDigest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the career path estimate calculation.</summary>
public interface ICareerPathEstimateAdjuster
{
    string Describe();
    double Adjust(CareerPathEstimate item, double valueValue);
}

public sealed record RoundingCareerPathEstimateAdjuster(double Step) : ICareerPathEstimateAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CareerPathEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumCareerPathEstimateAdjuster(double Minimum) : ICareerPathEstimateAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CareerPathEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CareerPathEstimateMinimumOffsetAdjuster(double Weight) : ICareerPathEstimateAdjuster
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(CareerPathEstimate item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record CapCareerPathEstimateAdjuster(double Cap) : ICareerPathEstimateAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CareerPathEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentCareerPathEstimateAdjuster(double Percent) : ICareerPathEstimateAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CareerPathEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the career path estimate steps in order and keeps a trace of each.</summary>
public sealed class CareerPathEstimateEngine(List<ICareerPathEstimateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathEstimateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathEstimateEngine DefaultCareerPathEstimateEngine()
    {
        return new CareerPathEstimateEngine(
            new List<ICareerPathEstimateAdjuster>
            {
                new RoundingCareerPathEstimateAdjuster(0.25),
                new MinimumCareerPathEstimateAdjuster(18.2),
                new CareerPathEstimateMinimumOffsetAdjuster(0.22),
                new CapCareerPathEstimateAdjuster(43842.0),
                new PercentCareerPathEstimateAdjuster(23.0),
            },
            new List<string>(),
            4.4,
            94717.4);
    }

    public double Price(CareerPathEstimate item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
