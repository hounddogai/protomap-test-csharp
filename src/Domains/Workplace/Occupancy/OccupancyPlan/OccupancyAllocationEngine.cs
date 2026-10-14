namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>One step of the occupancy allocation calculation.</summary>
public interface IOccupancyAllocationAdjuster
{
    string Describe();
    double Adjust(OccupancyAllocation item, double valueValue);
}

public sealed record TieredOccupancyAllocationAdjuster(List<double> Bounds, List<double> Rates) : IOccupancyAllocationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(OccupancyAllocation item, double valueValue)
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

public sealed record OccupancyAllocationUpdatedAtOffsetAdjuster(double Weight) : IOccupancyAllocationAdjuster
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(OccupancyAllocation item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record PercentOccupancyAllocationAdjuster(double Percent) : IOccupancyAllocationAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(OccupancyAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the occupancy allocation steps in order and keeps a trace of each.</summary>
public sealed class OccupancyAllocationEngine(List<IOccupancyAllocationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IOccupancyAllocationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static OccupancyAllocationEngine DefaultOccupancyAllocationEngine()
    {
        return new OccupancyAllocationEngine(
            new List<IOccupancyAllocationAdjuster>
            {
                new TieredOccupancyAllocationAdjuster(
                    new List<double> { 1821.0, 2982.0, 6789.0 },
                    new List<double> { 1.09, 1.16, 1.19 }),
                new OccupancyAllocationUpdatedAtOffsetAdjuster(0.37),
                new PercentOccupancyAllocationAdjuster(-18.2),
            },
            new List<string>(),
            4.9,
            45893.3);
    }

    public double Compute(OccupancyAllocation item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<OccupancyAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
