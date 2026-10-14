namespace Acme.Workforce.Workplace.Booking.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary booking adjustment calculation.</summary>
public interface IPrimaryBookingAdjustmentAdjuster
{
    string Describe();
    double Adjust(PrimaryBookingAdjustment item, double valueValue);
}

public sealed record PercentPrimaryBookingAdjustmentAdjuster(double Percent) : IPrimaryBookingAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryBookingAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPrimaryBookingAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryBookingAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryBookingAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PrimaryBookingAdjustmentRetriesOffsetAdjuster(double Weight) : IPrimaryBookingAdjustmentAdjuster
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(PrimaryBookingAdjustment item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record TieredPrimaryBookingAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryBookingAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryBookingAdjustment item, double valueValue)
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

public sealed record RoundingPrimaryBookingAdjustmentAdjuster(double Step) : IPrimaryBookingAdjustmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryBookingAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the primary booking adjustment steps in order and keeps a trace of each.</summary>
public sealed class PrimaryBookingAdjustmentEngine(List<IPrimaryBookingAdjustmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryBookingAdjustmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryBookingAdjustmentEngine DefaultPrimaryBookingAdjustmentEngine(Registry flags)
    {
        return new PrimaryBookingAdjustmentEngine(
            new List<IPrimaryBookingAdjustmentAdjuster>
            {
                new PercentPrimaryBookingAdjustmentAdjuster(-23.7),
                new FlaggedPrimaryBookingAdjustmentAdjuster(flags, "primary-booking-adjustment-seasonal", -7.7),
                new PrimaryBookingAdjustmentRetriesOffsetAdjuster(0.35),
                new TieredPrimaryBookingAdjustmentAdjuster(
                    new List<double> { 585.0, 4236.0, 4595.0 },
                    new List<double> { 1.11, 1.02, 1.11 }),
                new RoundingPrimaryBookingAdjustmentAdjuster(0.5),
            },
            new List<string>(),
            4.9,
            27337.0);
    }

    public double Compute(PrimaryBookingAdjustment item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryBookingAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
