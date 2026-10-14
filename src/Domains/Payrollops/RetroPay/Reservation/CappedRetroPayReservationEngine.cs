namespace Acme.Workforce.Payrollops.RetroPay.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped retro pay reservation calculation.</summary>
public interface ICappedRetroPayReservationModifier
{
    string Describe();
    double Adjust(CappedRetroPayReservation item, double valueValue);
}

public sealed record CappedRetroPayReservationHoursOffsetAdjuster(double Weight) : ICappedRetroPayReservationModifier
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(CappedRetroPayReservation item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record PercentCappedRetroPayReservationAdjuster(double Percent) : ICappedRetroPayReservationModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedRetroPayReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumCappedRetroPayReservationAdjuster(double Minimum) : ICappedRetroPayReservationModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedRetroPayReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingCappedRetroPayReservationAdjuster(double Step) : ICappedRetroPayReservationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedRetroPayReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedCappedRetroPayReservationAdjuster(Registry Flags, string Flag, double Percent) : ICappedRetroPayReservationModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedRetroPayReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped retro pay reservation steps in order and keeps a trace of each.</summary>
public sealed class CappedRetroPayReservationEngine(List<ICappedRetroPayReservationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedRetroPayReservationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedRetroPayReservationEngine DefaultCappedRetroPayReservationEngine(Registry flags)
    {
        return new CappedRetroPayReservationEngine(
            new List<ICappedRetroPayReservationModifier>
            {
                new CappedRetroPayReservationHoursOffsetAdjuster(1.71),
                new PercentCappedRetroPayReservationAdjuster(1.3),
                new MinimumCappedRetroPayReservationAdjuster(0.4),
                new RoundingCappedRetroPayReservationAdjuster(1.0),
                new FlaggedCappedRetroPayReservationAdjuster(flags, "capped-retro-pay-reservation-global", 2.6),
            },
            new List<string>(),
            0.8,
            29719.8);
    }

    public double Compute(CappedRetroPayReservation item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CappedRetroPayReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
