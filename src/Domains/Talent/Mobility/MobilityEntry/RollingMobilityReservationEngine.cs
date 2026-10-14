namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling mobility reservation calculation.</summary>
public interface IRollingMobilityReservationModifier
{
    string Describe();
    double Adjust(RollingMobilityReservation item, double valueValue);
}

public sealed record RollingMobilityReservationGraceDaysOffsetAdjuster(double Weight) : IRollingMobilityReservationModifier
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(RollingMobilityReservation item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record CapRollingMobilityReservationAdjuster(double Cap) : IRollingMobilityReservationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingMobilityReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedRollingMobilityReservationAdjuster(Registry Flags, string Flag, double Percent) : IRollingMobilityReservationModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingMobilityReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleRollingMobilityReservationAdjuster(Dictionary<string, double> Factors) : IRollingMobilityReservationModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RollingMobilityReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredRollingMobilityReservationAdjuster(List<double> Bounds, List<double> Rates) : IRollingMobilityReservationModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingMobilityReservation item, double valueValue)
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

/// <summary>Runs the rolling mobility reservation steps in order and keeps a trace of each.</summary>
public sealed class RollingMobilityReservationEngine(List<IRollingMobilityReservationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingMobilityReservationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingMobilityReservationEngine DefaultRollingMobilityReservationEngine(Registry flags)
    {
        return new RollingMobilityReservationEngine(
            new List<IRollingMobilityReservationModifier>
            {
                new RollingMobilityReservationGraceDaysOffsetAdjuster(0.25),
                new CapRollingMobilityReservationAdjuster(49890.0),
                new FlaggedRollingMobilityReservationAdjuster(flags, "rolling-mobility-reservation-global", 1.4),
                new TitleRollingMobilityReservationAdjuster(new Dictionary<string, double> { ["west"] = 1.37 }),
                new TieredRollingMobilityReservationAdjuster(
                    new List<double> { 1813.0, 7911.0, 9731.0 },
                    new List<double> { 0.84, 0.81, 0.85 }),
            },
            new List<string>(),
            1.9,
            14531.7);
    }

    public double Evaluate(RollingMobilityReservation item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RollingMobilityReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
