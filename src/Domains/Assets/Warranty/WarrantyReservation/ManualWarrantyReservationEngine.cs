namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual warranty reservation calculation.</summary>
public interface IManualWarrantyReservationAdjuster
{
    string Describe();
    double Adjust(ManualWarrantyReservation item, double valueValue);
}

public sealed record PercentManualWarrantyReservationAdjuster(double Percent) : IManualWarrantyReservationAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualWarrantyReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedManualWarrantyReservationAdjuster(Registry Flags, string Flag, double Percent) : IManualWarrantyReservationAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualWarrantyReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumManualWarrantyReservationAdjuster(double Minimum) : IManualWarrantyReservationAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualWarrantyReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapManualWarrantyReservationAdjuster(double Cap) : IManualWarrantyReservationAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualWarrantyReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ManualWarrantyReservationMaximumOffsetAdjuster(double Weight) : IManualWarrantyReservationAdjuster
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(ManualWarrantyReservation item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record TieredManualWarrantyReservationAdjuster(List<double> Bounds, List<double> Rates) : IManualWarrantyReservationAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualWarrantyReservation item, double valueValue)
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

/// <summary>Runs the manual warranty reservation steps in order and keeps a trace of each.</summary>
public sealed class ManualWarrantyReservationEngine(List<IManualWarrantyReservationAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualWarrantyReservationAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualWarrantyReservationEngine DefaultManualWarrantyReservationEngine(Registry flags)
    {
        return new ManualWarrantyReservationEngine(
            new List<IManualWarrantyReservationAdjuster>
            {
                new PercentManualWarrantyReservationAdjuster(1.2),
                new FlaggedManualWarrantyReservationAdjuster(flags, "manual-warranty-reservation-legacy", 3.3),
                new MinimumManualWarrantyReservationAdjuster(30.6),
                new CapManualWarrantyReservationAdjuster(30288.0),
                new ManualWarrantyReservationMaximumOffsetAdjuster(1.68),
                new TieredManualWarrantyReservationAdjuster(
                    new List<double> { 3491.0, 3668.0, 7114.0, 7124.0 },
                    new List<double> { 1.17, 1.15, 1.05, 1.01 }),
            },
            new List<string>(),
            3.0,
            5944.7);
    }

    public double Compute(ManualWarrantyReservation item)
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

    public Dictionary<string, double> EvaluateAll(List<ManualWarrantyReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
