namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary floor plan reservation calculation.</summary>
public interface IPrimaryFloorPlanReservationAdjuster
{
    string Describe();
    double Adjust(PrimaryFloorPlanReservation item, double valueValue);
}

public sealed record CapPrimaryFloorPlanReservationAdjuster(double Cap) : IPrimaryFloorPlanReservationAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryFloorPlanReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPrimaryFloorPlanReservationAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryFloorPlanReservationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryFloorPlanReservation item, double valueValue)
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

public sealed record RoundingPrimaryFloorPlanReservationAdjuster(double Step) : IPrimaryFloorPlanReservationAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryFloorPlanReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the primary floor plan reservation steps in order and keeps a trace of each.</summary>
public sealed class PrimaryFloorPlanReservationEngine(List<IPrimaryFloorPlanReservationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryFloorPlanReservationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryFloorPlanReservationEngine DefaultPrimaryFloorPlanReservationEngine()
    {
        return new PrimaryFloorPlanReservationEngine(
            new List<IPrimaryFloorPlanReservationAdjuster>
            {
                new CapPrimaryFloorPlanReservationAdjuster(5795.0),
                new TieredPrimaryFloorPlanReservationAdjuster(
                    new List<double> { 284.0, 5659.0, 5994.0, 7413.0 },
                    new List<double> { 0.87, 0.92, 0.82, 0.87 }),
                new RoundingPrimaryFloorPlanReservationAdjuster(1.0),
            },
            new List<string>(),
            0.5,
            2974.2);
    }

    public double Run(PrimaryFloorPlanReservation item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryFloorPlanReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
