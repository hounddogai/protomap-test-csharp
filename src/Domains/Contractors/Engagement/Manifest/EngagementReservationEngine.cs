namespace Acme.Workforce.Contractors.Engagement.Manifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the engagement reservation calculation.</summary>
public interface IEngagementReservationStep
{
    string Describe();
    double Adjust(EngagementReservation item, double valueValue);
}

public sealed record CapEngagementReservationAdjuster(double Cap) : IEngagementReservationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EngagementReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredEngagementReservationAdjuster(List<double> Bounds, List<double> Rates) : IEngagementReservationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EngagementReservation item, double valueValue)
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

public sealed record FlaggedEngagementReservationAdjuster(Registry Flags, string Flag, double Percent) : IEngagementReservationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EngagementReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record EngagementReservationRatioOffsetAdjuster(double Weight) : IEngagementReservationStep
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(EngagementReservation item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

/// <summary>Runs the engagement reservation steps in order and keeps a trace of each.</summary>
public sealed class EngagementReservationEngine(List<IEngagementReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementReservationEngine DefaultEngagementReservationEngine(Registry flags)
    {
        return new EngagementReservationEngine(
            new List<IEngagementReservationStep>
            {
                new CapEngagementReservationAdjuster(37061.0),
                new TieredEngagementReservationAdjuster(
                    new List<double> { 2993.0, 8198.0 },
                    new List<double> { 0.97, 0.81 }),
                new FlaggedEngagementReservationAdjuster(flags, "engagement-reservation-tiered", -4.2),
                new EngagementReservationRatioOffsetAdjuster(1.4),
            },
            new List<string>(),
            2.5,
            28529.5);
    }

    public double Evaluate(EngagementReservation item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<EngagementReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
