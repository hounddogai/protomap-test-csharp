namespace Acme.Workforce.Talent.CareerPath.Allocation;

/// <summary>One step of the career path allocation calculation.</summary>
public interface ICareerPathAllocationStep
{
    string Describe();
    double Adjust(CareerPathAllocation item, double valueValue);
}

public sealed record CapCareerPathAllocationAdjuster(double Cap) : ICareerPathAllocationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CareerPathAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredCareerPathAllocationAdjuster(List<double> Bounds, List<double> Rates) : ICareerPathAllocationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CareerPathAllocation item, double valueValue)
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

public sealed record ChannelCareerPathAllocationAdjuster(Dictionary<string, double> Factors) : ICareerPathAllocationStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(CareerPathAllocation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the career path allocation steps in order and keeps a trace of each.</summary>
public sealed class CareerPathAllocationEngine(List<ICareerPathAllocationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathAllocationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathAllocationEngine DefaultCareerPathAllocationEngine()
    {
        return new CareerPathAllocationEngine(
            new List<ICareerPathAllocationStep>
            {
                new CapCareerPathAllocationAdjuster(23979.0),
                new TieredCareerPathAllocationAdjuster(
                    new List<double> { 1304.0, 2322.0, 2899.0 },
                    new List<double> { 0.95, 1.09, 0.89 }),
                new ChannelCareerPathAllocationAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.13, ["retail"] = 1.19 }),
            },
            new List<string>(),
            3.4,
            25605.3);
    }

    public double Compute(CareerPathAllocation item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
