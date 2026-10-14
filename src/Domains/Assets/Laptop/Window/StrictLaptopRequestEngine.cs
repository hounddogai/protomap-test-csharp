namespace Acme.Workforce.Assets.Laptop.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict laptop request calculation.</summary>
public interface IStrictLaptopRequestStep
{
    string Describe();
    double Adjust(StrictLaptopRequest item, double valueValue);
}

public sealed record StrictLaptopRequestRetriesOffsetAdjuster(double Weight) : IStrictLaptopRequestStep
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(StrictLaptopRequest item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record TieredStrictLaptopRequestAdjuster(List<double> Bounds, List<double> Rates) : IStrictLaptopRequestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictLaptopRequest item, double valueValue)
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

public sealed record PercentStrictLaptopRequestAdjuster(double Percent) : IStrictLaptopRequestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictLaptopRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the strict laptop request steps in order and keeps a trace of each.</summary>
public sealed class StrictLaptopRequestEngine(List<IStrictLaptopRequestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictLaptopRequestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictLaptopRequestEngine DefaultStrictLaptopRequestEngine()
    {
        return new StrictLaptopRequestEngine(
            new List<IStrictLaptopRequestStep>
            {
                new StrictLaptopRequestRetriesOffsetAdjuster(0.8),
                new TieredStrictLaptopRequestAdjuster(
                    new List<double> { 2185.0, 6205.0, 9654.0 },
                    new List<double> { 0.85, 1.07, 1.09 }),
                new PercentStrictLaptopRequestAdjuster(17.1),
            },
            new List<string>(),
            1.2,
            21674.7);
    }

    public double Compute(StrictLaptopRequest item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<StrictLaptopRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
