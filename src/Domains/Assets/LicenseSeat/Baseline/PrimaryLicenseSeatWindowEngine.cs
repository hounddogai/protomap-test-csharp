namespace Acme.Workforce.Assets.LicenseSeat.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary license seat window calculation.</summary>
public interface IPrimaryLicenseSeatWindowStep
{
    string Describe();
    double Adjust(PrimaryLicenseSeatWindow item, double valueValue);
}

public sealed record RoundingPrimaryLicenseSeatWindowAdjuster(double Step) : IPrimaryLicenseSeatWindowStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryLicenseSeatWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPrimaryLicenseSeatWindowAdjuster(double Percent) : IPrimaryLicenseSeatWindowStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryLicenseSeatWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPrimaryLicenseSeatWindowAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryLicenseSeatWindowStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryLicenseSeatWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PrimaryLicenseSeatWindowRetriesOffsetAdjuster(double Weight) : IPrimaryLicenseSeatWindowStep
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(PrimaryLicenseSeatWindow item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record CapPrimaryLicenseSeatWindowAdjuster(double Cap) : IPrimaryLicenseSeatWindowStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryLicenseSeatWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the primary license seat window steps in order and keeps a trace of each.</summary>
public sealed class PrimaryLicenseSeatWindowEngine(List<IPrimaryLicenseSeatWindowStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryLicenseSeatWindowStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryLicenseSeatWindowEngine DefaultPrimaryLicenseSeatWindowEngine(Registry flags)
    {
        return new PrimaryLicenseSeatWindowEngine(
            new List<IPrimaryLicenseSeatWindowStep>
            {
                new RoundingPrimaryLicenseSeatWindowAdjuster(0.25),
                new PercentPrimaryLicenseSeatWindowAdjuster(2.5),
                new FlaggedPrimaryLicenseSeatWindowAdjuster(flags, "primary-license-seat-window-batched", 2.0),
                new PrimaryLicenseSeatWindowRetriesOffsetAdjuster(1.01),
                new CapPrimaryLicenseSeatWindowAdjuster(25140.0),
            },
            new List<string>(),
            0.1,
            79119.6);
    }

    public double Run(PrimaryLicenseSeatWindow item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryLicenseSeatWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
