namespace Acme.Workforce.Workplace.FloorPlan.Ledger;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the floor plan baseline calculation.</summary>
public interface IFloorPlanBaselineModifier
{
    string Describe();
    double Adjust(FloorPlanBaseline item, double valueValue);
}

public sealed record FloorPlanBaselineScoreOffsetAdjuster(double Weight) : IFloorPlanBaselineModifier
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(FloorPlanBaseline item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record TieredFloorPlanBaselineAdjuster(List<double> Bounds, List<double> Rates) : IFloorPlanBaselineModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FloorPlanBaseline item, double valueValue)
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

public sealed record PercentFloorPlanBaselineAdjuster(double Percent) : IFloorPlanBaselineModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FloorPlanBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapFloorPlanBaselineAdjuster(double Cap) : IFloorPlanBaselineModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FloorPlanBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedFloorPlanBaselineAdjuster(Registry Flags, string Flag, double Percent) : IFloorPlanBaselineModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FloorPlanBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the floor plan baseline steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanBaselineEngine(List<IFloorPlanBaselineModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanBaselineModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanBaselineEngine DefaultFloorPlanBaselineEngine(Registry flags)
    {
        return new FloorPlanBaselineEngine(
            new List<IFloorPlanBaselineModifier>
            {
                new FloorPlanBaselineScoreOffsetAdjuster(1.5),
                new TieredFloorPlanBaselineAdjuster(
                    new List<double> { 713.0, 3851.0, 8355.0, 9630.0 },
                    new List<double> { 0.99, 1.2, 0.85, 0.89 }),
                new PercentFloorPlanBaselineAdjuster(21.6),
                new CapFloorPlanBaselineAdjuster(19178.0),
                new FlaggedFloorPlanBaselineAdjuster(flags, "floor-plan-baseline-seasonal", -4.0),
            },
            new List<string>(),
            3.0,
            23310.9);
    }

    public double Price(FloorPlanBaseline item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
