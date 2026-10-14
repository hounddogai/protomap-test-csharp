namespace Acme.Workforce.Compensation.PayBand.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay band reservation calculation.</summary>
public interface IPayBandReservationStep
{
    string Describe();
    double Adjust(PayBandReservation item, double valueValue);
}

public sealed record RoundingPayBandReservationAdjuster(double Step) : IPayBandReservationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayBandReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapPayBandReservationAdjuster(double Cap) : IPayBandReservationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayBandReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPayBandReservationAdjuster(List<double> Bounds, List<double> Rates) : IPayBandReservationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayBandReservation item, double valueValue)
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

public sealed record PercentPayBandReservationAdjuster(double Percent) : IPayBandReservationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayBandReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPayBandReservationAdjuster(double Minimum) : IPayBandReservationStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayBandReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pay band reservation steps in order and keeps a trace of each.</summary>
public sealed class PayBandReservationEngine(List<IPayBandReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandReservationEngine DefaultPayBandReservationEngine()
    {
        return new PayBandReservationEngine(
            new List<IPayBandReservationStep>
            {
                new RoundingPayBandReservationAdjuster(0.05),
                new CapPayBandReservationAdjuster(44169.0),
                new TieredPayBandReservationAdjuster(
                    new List<double> { 3871.0, 6216.0, 6530.0, 6856.0 },
                    new List<double> { 1.03, 0.82, 1.16, 0.88 }),
                new PercentPayBandReservationAdjuster(-3.1),
                new MinimumPayBandReservationAdjuster(11.7),
            },
            new List<string>(),
            4.6,
            31817.7);
    }

    public double Run(PayBandReservation item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
