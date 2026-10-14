namespace Acme.Workforce.Engagement.ActionPlan.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered action plan reservation calculation.</summary>
public interface ITieredActionPlanReservationModifier
{
    string Describe();
    double Adjust(TieredActionPlanReservation item, double valueValue);
}

public sealed record FlaggedTieredActionPlanReservationAdjuster(Registry Flags, string Flag, double Percent) : ITieredActionPlanReservationModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredActionPlanReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumTieredActionPlanReservationAdjuster(double Minimum) : ITieredActionPlanReservationModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredActionPlanReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredTieredActionPlanReservationAdjuster(List<double> Bounds, List<double> Rates) : ITieredActionPlanReservationModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredActionPlanReservation item, double valueValue)
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

public sealed record CapTieredActionPlanReservationAdjuster(double Cap) : ITieredActionPlanReservationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredActionPlanReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the tiered action plan reservation steps in order and keeps a trace of each.</summary>
public sealed class TieredActionPlanReservationEngine(List<ITieredActionPlanReservationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredActionPlanReservationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredActionPlanReservationEngine DefaultTieredActionPlanReservationEngine(Registry flags)
    {
        return new TieredActionPlanReservationEngine(
            new List<ITieredActionPlanReservationModifier>
            {
                new FlaggedTieredActionPlanReservationAdjuster(flags, "tiered-action-plan-reservation-tiered", -5.9),
                new MinimumTieredActionPlanReservationAdjuster(21.5),
                new TieredTieredActionPlanReservationAdjuster(
                    new List<double> { 2587.0, 3246.0, 4527.0, 9719.0 },
                    new List<double> { 0.82, 0.91, 0.9, 1.01 }),
                new CapTieredActionPlanReservationAdjuster(5238.0),
            },
            new List<string>(),
            1.0,
            2533.4);
    }

    public double Run(TieredActionPlanReservation item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TieredActionPlanReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
