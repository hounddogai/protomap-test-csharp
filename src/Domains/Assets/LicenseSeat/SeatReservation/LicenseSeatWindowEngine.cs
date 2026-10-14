namespace Acme.Workforce.Assets.LicenseSeat.SeatReservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the license seat window calculation.</summary>
public interface ILicenseSeatWindowStep
{
    string Describe();
    double Adjust(LicenseSeatWindow item, double valueValue);
}

public sealed record RoundingLicenseSeatWindowAdjuster(double Step) : ILicenseSeatWindowStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LicenseSeatWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapLicenseSeatWindowAdjuster(double Cap) : ILicenseSeatWindowStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LicenseSeatWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedLicenseSeatWindowAdjuster(Registry Flags, string Flag, double Percent) : ILicenseSeatWindowStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LicenseSeatWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the license seat window steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatWindowEngine(List<ILicenseSeatWindowStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatWindowStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatWindowEngine DefaultLicenseSeatWindowEngine(Registry flags)
    {
        return new LicenseSeatWindowEngine(
            new List<ILicenseSeatWindowStep>
            {
                new RoundingLicenseSeatWindowAdjuster(5.0),
                new CapLicenseSeatWindowAdjuster(592.0),
                new FlaggedLicenseSeatWindowAdjuster(flags, "license-seat-window-global", 1.4),
            },
            new List<string>(),
            0.6,
            91269.1);
    }

    public double Price(LicenseSeatWindow item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
