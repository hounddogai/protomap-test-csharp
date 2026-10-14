namespace Acme.Workforce.Compensation.PayBand.Reservation;

/// <summary>One step of the default pay band reservation calculation.</summary>
public interface IDefaultPayBandReservationFactor
{
    string Describe();
    double Adjust(DefaultPayBandReservation item, double valueValue);
}

public sealed record MinimumDefaultPayBandReservationAdjuster(double Minimum) : IDefaultPayBandReservationFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultPayBandReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredDefaultPayBandReservationAdjuster(List<double> Bounds, List<double> Rates) : IDefaultPayBandReservationFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultPayBandReservation item, double valueValue)
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

public sealed record CapDefaultPayBandReservationAdjuster(double Cap) : IDefaultPayBandReservationFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultPayBandReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentDefaultPayBandReservationAdjuster(double Percent) : IDefaultPayBandReservationFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultPayBandReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default pay band reservation steps in order and keeps a trace of each.</summary>
public sealed class DefaultPayBandReservationEngine(List<IDefaultPayBandReservationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultPayBandReservationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultPayBandReservationEngine DefaultDefaultPayBandReservationEngine()
    {
        return new DefaultPayBandReservationEngine(
            new List<IDefaultPayBandReservationFactor>
            {
                new MinimumDefaultPayBandReservationAdjuster(42.7),
                new TieredDefaultPayBandReservationAdjuster(
                    new List<double> { 2829.0, 7614.0 },
                    new List<double> { 1.12, 1.05 }),
                new CapDefaultPayBandReservationAdjuster(12826.0),
                new PercentDefaultPayBandReservationAdjuster(-6.9),
            },
            new List<string>(),
            3.2,
            83848.5);
    }

    public double Compute(DefaultPayBandReservation item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultPayBandReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
