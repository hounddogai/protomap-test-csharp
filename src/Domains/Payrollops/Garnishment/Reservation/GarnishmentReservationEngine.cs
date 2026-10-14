namespace Acme.Workforce.Payrollops.Garnishment.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the garnishment reservation calculation.</summary>
public interface IGarnishmentReservationFactor
{
    string Describe();
    double Adjust(GarnishmentReservation item, double valueValue);
}

public sealed record RoundingGarnishmentReservationAdjuster(double Step) : IGarnishmentReservationFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GarnishmentReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelGarnishmentReservationAdjuster(Dictionary<string, double> Factors) : IGarnishmentReservationFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(GarnishmentReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedGarnishmentReservationAdjuster(Registry Flags, string Flag, double Percent) : IGarnishmentReservationFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GarnishmentReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapGarnishmentReservationAdjuster(double Cap) : IGarnishmentReservationFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GarnishmentReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the garnishment reservation steps in order and keeps a trace of each.</summary>
public sealed class GarnishmentReservationEngine(List<IGarnishmentReservationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGarnishmentReservationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GarnishmentReservationEngine DefaultGarnishmentReservationEngine(Registry flags)
    {
        return new GarnishmentReservationEngine(
            new List<IGarnishmentReservationFactor>
            {
                new RoundingGarnishmentReservationAdjuster(0.25),
                new LabelGarnishmentReservationAdjuster(new Dictionary<string, double> { ["east"] = 1.17 }),
                new FlaggedGarnishmentReservationAdjuster(flags, "garnishment-reservation-default", -2.2),
                new CapGarnishmentReservationAdjuster(9261.0),
            },
            new List<string>(),
            1.2,
            94790.4);
    }

    public double Compute(GarnishmentReservation item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<GarnishmentReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
