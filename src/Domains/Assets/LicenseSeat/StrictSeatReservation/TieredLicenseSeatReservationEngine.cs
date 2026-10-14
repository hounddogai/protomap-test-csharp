namespace Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered license seat reservation calculation.</summary>
public interface ITieredLicenseSeatReservationAdjuster
{
    string Describe();
    double Adjust(TieredLicenseSeatReservation item, double valueValue);
}

public sealed record TieredLicenseSeatReservationFteOffsetAdjuster(double Weight) : ITieredLicenseSeatReservationAdjuster
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(TieredLicenseSeatReservation item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record MinimumTieredLicenseSeatReservationAdjuster(double Minimum) : ITieredLicenseSeatReservationAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredLicenseSeatReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredTieredLicenseSeatReservationAdjuster(List<double> Bounds, List<double> Rates) : ITieredLicenseSeatReservationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredLicenseSeatReservation item, double valueValue)
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

public sealed record CategoryTieredLicenseSeatReservationAdjuster(Dictionary<string, double> Factors) : ITieredLicenseSeatReservationAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(TieredLicenseSeatReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedTieredLicenseSeatReservationAdjuster(Registry Flags, string Flag, double Percent) : ITieredLicenseSeatReservationAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredLicenseSeatReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the tiered license seat reservation steps in order and keeps a trace of each.</summary>
public sealed class TieredLicenseSeatReservationEngine(List<ITieredLicenseSeatReservationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredLicenseSeatReservationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredLicenseSeatReservationEngine DefaultTieredLicenseSeatReservationEngine(Registry flags)
    {
        return new TieredLicenseSeatReservationEngine(
            new List<ITieredLicenseSeatReservationAdjuster>
            {
                new TieredLicenseSeatReservationFteOffsetAdjuster(0.54),
                new MinimumTieredLicenseSeatReservationAdjuster(30.1),
                new TieredTieredLicenseSeatReservationAdjuster(
                    new List<double> { 3648.0, 3838.0, 4640.0, 4866.0 },
                    new List<double> { 1.18, 0.88, 0.98, 1.14 }),
                new CategoryTieredLicenseSeatReservationAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.35, ["west"] = 1.23 }),
                new FlaggedTieredLicenseSeatReservationAdjuster(
                    flags,
                    "tiered-license-seat-reservation-manual",
                    -6.9),
            },
            new List<string>(),
            0.2,
            27947.4);
    }

    public double Run(TieredLicenseSeatReservation item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TieredLicenseSeatReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
