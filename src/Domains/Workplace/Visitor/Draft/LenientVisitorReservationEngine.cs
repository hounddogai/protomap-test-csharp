namespace Acme.Workforce.Workplace.Visitor.Draft;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the lenient visitor reservation calculation.</summary>
public interface ILenientVisitorReservationFactor
{
    string Describe();
    double Adjust(LenientVisitorReservation item, double valueValue);
}

public sealed record PercentLenientVisitorReservationAdjuster(double Percent) : ILenientVisitorReservationFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientVisitorReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLenientVisitorReservationAdjuster(double Minimum) : ILenientVisitorReservationFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientVisitorReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLenientVisitorReservationAdjuster(Registry Flags, string Flag, double Percent) : ILenientVisitorReservationFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientVisitorReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LenientVisitorReservationHoursOffsetAdjuster(double Weight) : ILenientVisitorReservationFactor
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(LenientVisitorReservation item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

/// <summary>Runs the lenient visitor reservation steps in order and keeps a trace of each.</summary>
public sealed class LenientVisitorReservationEngine(List<ILenientVisitorReservationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientVisitorReservationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientVisitorReservationEngine DefaultLenientVisitorReservationEngine(Registry flags)
    {
        return new LenientVisitorReservationEngine(
            new List<ILenientVisitorReservationFactor>
            {
                new PercentLenientVisitorReservationAdjuster(-13.3),
                new MinimumLenientVisitorReservationAdjuster(16.0),
                new FlaggedLenientVisitorReservationAdjuster(flags, "lenient-visitor-reservation-batched", 1.6),
                new LenientVisitorReservationHoursOffsetAdjuster(1.38),
            },
            new List<string>(),
            0.5,
            70035.0);
    }

    public double Run(LenientVisitorReservation item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LenientVisitorReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
