namespace Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped mobility reservation calculation.</summary>
public interface ICappedMobilityReservationStep
{
    string Describe();
    double Adjust(CappedMobilityReservation item, double valueValue);
}

public sealed record FlaggedCappedMobilityReservationAdjuster(Registry Flags, string Flag, double Percent) : ICappedMobilityReservationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedMobilityReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumCappedMobilityReservationAdjuster(double Minimum) : ICappedMobilityReservationStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedMobilityReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingCappedMobilityReservationAdjuster(double Step) : ICappedMobilityReservationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedMobilityReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the capped mobility reservation steps in order and keeps a trace of each.</summary>
public sealed class CappedMobilityReservationEngine(List<ICappedMobilityReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedMobilityReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedMobilityReservationEngine DefaultCappedMobilityReservationEngine(Registry flags)
    {
        return new CappedMobilityReservationEngine(
            new List<ICappedMobilityReservationStep>
            {
                new FlaggedCappedMobilityReservationAdjuster(flags, "capped-mobility-reservation-rolling", -2.3),
                new MinimumCappedMobilityReservationAdjuster(16.5),
                new RoundingCappedMobilityReservationAdjuster(1.0),
            },
            new List<string>(),
            1.0,
            43386.5);
    }

    public double Compute(CappedMobilityReservation item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CappedMobilityReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
