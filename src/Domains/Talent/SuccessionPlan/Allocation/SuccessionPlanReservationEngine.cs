namespace Acme.Workforce.Talent.SuccessionPlan.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan reservation calculation.</summary>
public interface ISuccessionPlanReservationAdjuster
{
    string Describe();
    double Adjust(SuccessionPlanReservation item, double valueValue);
}

public sealed record PercentSuccessionPlanReservationAdjuster(double Percent) : ISuccessionPlanReservationAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SuccessionPlanReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSuccessionPlanReservationAdjuster(double Minimum) : ISuccessionPlanReservationAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SuccessionPlanReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredSuccessionPlanReservationAdjuster(List<double> Bounds, List<double> Rates) : ISuccessionPlanReservationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SuccessionPlanReservation item, double valueValue)
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

public sealed record FlaggedSuccessionPlanReservationAdjuster(Registry Flags, string Flag, double Percent) : ISuccessionPlanReservationAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SuccessionPlanReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSuccessionPlanReservationAdjuster(double Cap) : ISuccessionPlanReservationAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SuccessionPlanReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the succession plan reservation steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanReservationEngine(List<ISuccessionPlanReservationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanReservationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanReservationEngine DefaultSuccessionPlanReservationEngine(Registry flags)
    {
        return new SuccessionPlanReservationEngine(
            new List<ISuccessionPlanReservationAdjuster>
            {
                new PercentSuccessionPlanReservationAdjuster(2.1),
                new MinimumSuccessionPlanReservationAdjuster(13.1),
                new TieredSuccessionPlanReservationAdjuster(
                    new List<double> { 743.0, 965.0, 4340.0, 6576.0 },
                    new List<double> { 1.1, 0.98, 0.97, 1.06 }),
                new FlaggedSuccessionPlanReservationAdjuster(flags, "succession-plan-reservation-seasonal", -6.1),
                new CapSuccessionPlanReservationAdjuster(8709.0),
            },
            new List<string>(),
            3.4,
            58740.0);
    }

    public double Compute(SuccessionPlanReservation item)
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
