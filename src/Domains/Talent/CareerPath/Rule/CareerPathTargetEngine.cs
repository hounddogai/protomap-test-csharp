namespace Acme.Workforce.Talent.CareerPath.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the career path target calculation.</summary>
public interface ICareerPathTargetModifier
{
    string Describe();
    double Adjust(CareerPathTarget item, double valueValue);
}

public sealed record TieredCareerPathTargetAdjuster(List<double> Bounds, List<double> Rates) : ICareerPathTargetModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CareerPathTarget item, double valueValue)
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

public sealed record CapCareerPathTargetAdjuster(double Cap) : ICareerPathTargetModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CareerPathTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentCareerPathTargetAdjuster(double Percent) : ICareerPathTargetModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CareerPathTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the career path target steps in order and keeps a trace of each.</summary>
public sealed class CareerPathTargetEngine(List<ICareerPathTargetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathTargetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathTargetEngine DefaultCareerPathTargetEngine()
    {
        return new CareerPathTargetEngine(
            new List<ICareerPathTargetModifier>
            {
                new TieredCareerPathTargetAdjuster(
                    new List<double> { 8056.0, 8191.0, 9561.0 },
                    new List<double> { 0.98, 1.02, 1.2 }),
                new CapCareerPathTargetAdjuster(30563.0),
                new PercentCareerPathTargetAdjuster(9.4),
            },
            new List<string>(),
            0.6,
            4907.5);
    }

    public double Price(CareerPathTarget item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
