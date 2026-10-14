namespace Acme.Workforce.Assets.LicenseSeat.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the license seat reservation calculation.</summary>
public interface ILicenseSeatReservationFactor
{
    string Describe();
    double Adjust(LicenseSeatReservation item, double valueValue);
}

public sealed record LicenseSeatReservationLeaveDaysOffsetAdjuster(double Weight) : ILicenseSeatReservationFactor
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(LicenseSeatReservation item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record TitleLicenseSeatReservationAdjuster(Dictionary<string, double> Factors) : ILicenseSeatReservationFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LicenseSeatReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumLicenseSeatReservationAdjuster(double Minimum) : ILicenseSeatReservationFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LicenseSeatReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the license seat reservation steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatReservationEngine(List<ILicenseSeatReservationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatReservationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatReservationEngine DefaultLicenseSeatReservationEngine()
    {
        return new LicenseSeatReservationEngine(
            new List<ILicenseSeatReservationFactor>
            {
                new LicenseSeatReservationLeaveDaysOffsetAdjuster(0.05),
                new TitleLicenseSeatReservationAdjuster(new Dictionary<string, double> { ["retail"] = 1.18 }),
                new MinimumLicenseSeatReservationAdjuster(10.1),
            },
            new List<string>(),
            1.3,
            87856.8);
    }

    public double Evaluate(LicenseSeatReservation item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
