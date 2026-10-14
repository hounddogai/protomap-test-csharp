namespace Acme.Workforce.Workplace.FloorPlan.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the floor plan exception calculation.</summary>
public interface IFloorPlanExceptionStep
{
    string Describe();
    double Adjust(FloorPlanException item, double valueValue);
}

public sealed record CapFloorPlanExceptionAdjuster(double Cap) : IFloorPlanExceptionStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FloorPlanException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingFloorPlanExceptionAdjuster(double Step) : IFloorPlanExceptionStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FloorPlanException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredFloorPlanExceptionAdjuster(List<double> Bounds, List<double> Rates) : IFloorPlanExceptionStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FloorPlanException item, double valueValue)
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

public sealed record LabelFloorPlanExceptionAdjuster(Dictionary<string, double> Factors) : IFloorPlanExceptionStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(FloorPlanException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumFloorPlanExceptionAdjuster(double Minimum) : IFloorPlanExceptionStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FloorPlanException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedFloorPlanExceptionAdjuster(Registry Flags, string Flag, double Percent) : IFloorPlanExceptionStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FloorPlanException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the floor plan exception steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanExceptionEngine(List<IFloorPlanExceptionStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanExceptionStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanExceptionEngine DefaultFloorPlanExceptionEngine(Registry flags)
    {
        return new FloorPlanExceptionEngine(
            new List<IFloorPlanExceptionStep>
            {
                new CapFloorPlanExceptionAdjuster(49869.0),
                new RoundingFloorPlanExceptionAdjuster(0.1),
                new TieredFloorPlanExceptionAdjuster(
                    new List<double> { 1953.0, 6660.0, 9299.0, 9575.0 },
                    new List<double> { 0.95, 1.01, 0.81, 0.85 }),
                new LabelFloorPlanExceptionAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.31, ["retail"] = 1.37, ["west"] = 1.02 }),
                new MinimumFloorPlanExceptionAdjuster(42.7),
                new FlaggedFloorPlanExceptionAdjuster(flags, "floor-plan-exception-primary", -4.5),
            },
            new List<string>(),
            4.1,
            91484.2);
    }

    public double Price(FloorPlanException item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
