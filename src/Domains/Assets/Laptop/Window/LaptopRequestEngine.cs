namespace Acme.Workforce.Assets.Laptop.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the laptop request calculation.</summary>
public interface ILaptopRequestStep
{
    string Describe();
    double Adjust(LaptopRequest item, double valueValue);
}

public sealed record PercentLaptopRequestAdjuster(double Percent) : ILaptopRequestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LaptopRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLaptopRequestAdjuster(List<double> Bounds, List<double> Rates) : ILaptopRequestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LaptopRequest item, double valueValue)
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

public sealed record RoundingLaptopRequestAdjuster(double Step) : ILaptopRequestStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LaptopRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapLaptopRequestAdjuster(double Cap) : ILaptopRequestStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LaptopRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LaptopRequestCeilingOffsetAdjuster(double Weight) : ILaptopRequestStep
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(LaptopRequest item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record LabelLaptopRequestAdjuster(Dictionary<string, double> Factors) : ILaptopRequestStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(LaptopRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the laptop request steps in order and keeps a trace of each.</summary>
public sealed class LaptopRequestEngine(List<ILaptopRequestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopRequestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopRequestEngine DefaultLaptopRequestEngine()
    {
        return new LaptopRequestEngine(
            new List<ILaptopRequestStep>
            {
                new PercentLaptopRequestAdjuster(-19.8),
                new TieredLaptopRequestAdjuster(
                    new List<double> { 25.0, 1750.0, 7493.0, 7951.0 },
                    new List<double> { 1.13, 1.05, 0.91, 0.82 }),
                new RoundingLaptopRequestAdjuster(0.05),
                new CapLaptopRequestAdjuster(20342.0),
                new LaptopRequestCeilingOffsetAdjuster(1.38),
                new LabelLaptopRequestAdjuster(new Dictionary<string, double> { ["online"] = 0.87 }),
            },
            new List<string>(),
            1.3,
            58961.7);
    }

    public double Compute(LaptopRequest item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<LaptopRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
