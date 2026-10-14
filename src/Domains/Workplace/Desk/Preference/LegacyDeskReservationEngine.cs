namespace Acme.Workforce.Workplace.Desk.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy desk reservation calculation.</summary>
public interface ILegacyDeskReservationStep
{
    string Describe();
    double Adjust(LegacyDeskReservation item, double valueValue);
}

public sealed record FlaggedLegacyDeskReservationAdjuster(Registry Flags, string Flag, double Percent) : ILegacyDeskReservationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyDeskReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLegacyDeskReservationAdjuster(List<double> Bounds, List<double> Rates) : ILegacyDeskReservationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyDeskReservation item, double valueValue)
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

public sealed record PercentLegacyDeskReservationAdjuster(double Percent) : ILegacyDeskReservationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyDeskReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy desk reservation steps in order and keeps a trace of each.</summary>
public sealed class LegacyDeskReservationEngine(List<ILegacyDeskReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyDeskReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyDeskReservationEngine DefaultLegacyDeskReservationEngine(Registry flags)
    {
        return new LegacyDeskReservationEngine(
            new List<ILegacyDeskReservationStep>
            {
                new FlaggedLegacyDeskReservationAdjuster(flags, "legacy-desk-reservation-default", -1.0),
                new TieredLegacyDeskReservationAdjuster(
                    new List<double> { 5781.0, 7170.0 },
                    new List<double> { 0.95, 0.84 }),
                new PercentLegacyDeskReservationAdjuster(17.0),
            },
            new List<string>(),
            1.2,
            12397.5);
    }

    public double Price(LegacyDeskReservation item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyDeskReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
