namespace Acme.Workforce.Assets.LicenseSeat.SeatReservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback license seat reservation calculation.</summary>
public interface IFallbackLicenseSeatReservationFactor
{
    string Describe();
    double Adjust(FallbackLicenseSeatReservation item, double valueValue);
}

public sealed record FallbackLicenseSeatReservationMarginOffsetAdjuster(double Weight) : IFallbackLicenseSeatReservationFactor
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(FallbackLicenseSeatReservation item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record RoundingFallbackLicenseSeatReservationAdjuster(double Step) : IFallbackLicenseSeatReservationFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackLicenseSeatReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredFallbackLicenseSeatReservationAdjuster(List<double> Bounds, List<double> Rates) : IFallbackLicenseSeatReservationFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackLicenseSeatReservation item, double valueValue)
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

public sealed record NameFallbackLicenseSeatReservationAdjuster(Dictionary<string, double> Factors) : IFallbackLicenseSeatReservationFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(FallbackLicenseSeatReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the fallback license seat reservation steps in order and keeps a trace of each.</summary>
public sealed class FallbackLicenseSeatReservationEngine(List<IFallbackLicenseSeatReservationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackLicenseSeatReservationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackLicenseSeatReservationEngine DefaultFallbackLicenseSeatReservationEngine()
    {
        return new FallbackLicenseSeatReservationEngine(
            new List<IFallbackLicenseSeatReservationFactor>
            {
                new FallbackLicenseSeatReservationMarginOffsetAdjuster(1.07),
                new RoundingFallbackLicenseSeatReservationAdjuster(5.0),
                new TieredFallbackLicenseSeatReservationAdjuster(
                    new List<double> { 3910.0, 8707.0 },
                    new List<double> { 1.09, 0.87 }),
                new NameFallbackLicenseSeatReservationAdjuster(new Dictionary<string, double> { ["south"] = 1.07 }),
            },
            new List<string>(),
            5.0,
            82507.7);
    }

    public double Run(FallbackLicenseSeatReservation item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackLicenseSeatReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
