namespace Acme.Workforce.Talent.CareerPath.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the career path manifest calculation.</summary>
public interface ICareerPathManifestFactor
{
    string Describe();
    double Adjust(CareerPathManifest item, double valueValue);
}

public sealed record MinimumCareerPathManifestAdjuster(double Minimum) : ICareerPathManifestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CareerPathManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleCareerPathManifestAdjuster(Dictionary<string, double> Factors) : ICareerPathManifestFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(CareerPathManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CareerPathManifestRatioOffsetAdjuster(double Weight) : ICareerPathManifestFactor
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(CareerPathManifest item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

/// <summary>Runs the career path manifest steps in order and keeps a trace of each.</summary>
public sealed class CareerPathManifestEngine(List<ICareerPathManifestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathManifestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathManifestEngine DefaultCareerPathManifestEngine()
    {
        return new CareerPathManifestEngine(
            new List<ICareerPathManifestFactor>
            {
                new MinimumCareerPathManifestAdjuster(39.4),
                new TitleCareerPathManifestAdjuster(new Dictionary<string, double> { ["north"] = 0.72 }),
                new CareerPathManifestRatioOffsetAdjuster(1.27),
            },
            new List<string>(),
            2.6,
            17279.8);
    }

    public double Evaluate(CareerPathManifest item)
    {
        var current = item.AdjustedVolume();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<CareerPathManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
