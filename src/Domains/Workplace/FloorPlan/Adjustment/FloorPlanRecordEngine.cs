namespace Acme.Workforce.Workplace.FloorPlan.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the floor plan record calculation.</summary>
public interface IFloorPlanRecordAdjuster
{
    string Describe();
    double Adjust(FloorPlanRecord item, double valueValue);
}

public sealed record RoundingFloorPlanRecordAdjuster(double Step) : IFloorPlanRecordAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FloorPlanRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumFloorPlanRecordAdjuster(double Minimum) : IFloorPlanRecordAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FloorPlanRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredFloorPlanRecordAdjuster(List<double> Bounds, List<double> Rates) : IFloorPlanRecordAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FloorPlanRecord item, double valueValue)
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

public sealed record PercentFloorPlanRecordAdjuster(double Percent) : IFloorPlanRecordAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FloorPlanRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the floor plan record steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanRecordEngine(List<IFloorPlanRecordAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanRecordAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanRecordEngine DefaultFloorPlanRecordEngine()
    {
        return new FloorPlanRecordEngine(
            new List<IFloorPlanRecordAdjuster>
            {
                new RoundingFloorPlanRecordAdjuster(5.0),
                new MinimumFloorPlanRecordAdjuster(12.7),
                new TieredFloorPlanRecordAdjuster(
                    new List<double> { 1687.0, 9195.0 },
                    new List<double> { 1.17, 0.99 }),
                new PercentFloorPlanRecordAdjuster(0.9),
            },
            new List<string>(),
            0.5,
            66370.2);
    }

    public double Compute(FloorPlanRecord item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
