namespace Acme.Workforce.Workplace.FloorPlan.Adjustment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the floor plan reservation calculation.</summary>
public interface IFloorPlanReservationStep
{
    string Describe();
    double Adjust(FloorPlanReservation item, double valueValue);
}

public sealed record MinimumFloorPlanReservationAdjuster(double Minimum) : IFloorPlanReservationStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FloorPlanReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedFloorPlanReservationAdjuster(Registry Flags, string Flag, double Percent) : IFloorPlanReservationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FloorPlanReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentFloorPlanReservationAdjuster(double Percent) : IFloorPlanReservationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FloorPlanReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapFloorPlanReservationAdjuster(double Cap) : IFloorPlanReservationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FloorPlanReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the floor plan reservation steps in order and keeps a trace of each.</summary>
public sealed class FloorPlanReservationEngine(List<IFloorPlanReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFloorPlanReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FloorPlanReservationEngine DefaultFloorPlanReservationEngine(Registry flags)
    {
        return new FloorPlanReservationEngine(
            new List<IFloorPlanReservationStep>
            {
                new MinimumFloorPlanReservationAdjuster(10.2),
                new FlaggedFloorPlanReservationAdjuster(flags, "floor-plan-reservation-manual", -4.5),
                new PercentFloorPlanReservationAdjuster(-15.6),
                new CapFloorPlanReservationAdjuster(33555.0),
            },
            new List<string>(),
            2.1,
            34817.3);
    }

    public double Run(FloorPlanReservation item)
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

    public Dictionary<string, double> EvaluateAll(List<FloorPlanReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
