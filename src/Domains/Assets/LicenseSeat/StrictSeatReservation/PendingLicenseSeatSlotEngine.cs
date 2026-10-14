namespace Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending license seat slot calculation.</summary>
public interface IPendingLicenseSeatSlotModifier
{
    string Describe();
    double Adjust(PendingLicenseSeatSlot item, double valueValue);
}

public sealed record RoundingPendingLicenseSeatSlotAdjuster(double Step) : IPendingLicenseSeatSlotModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingLicenseSeatSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CategoryPendingLicenseSeatSlotAdjuster(Dictionary<string, double> Factors) : IPendingLicenseSeatSlotModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(PendingLicenseSeatSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPendingLicenseSeatSlotAdjuster(double Cap) : IPendingLicenseSeatSlotModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingLicenseSeatSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPendingLicenseSeatSlotAdjuster(List<double> Bounds, List<double> Rates) : IPendingLicenseSeatSlotModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingLicenseSeatSlot item, double valueValue)
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

public sealed record MinimumPendingLicenseSeatSlotAdjuster(double Minimum) : IPendingLicenseSeatSlotModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingLicenseSeatSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pending license seat slot steps in order and keeps a trace of each.</summary>
public sealed class PendingLicenseSeatSlotEngine(List<IPendingLicenseSeatSlotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingLicenseSeatSlotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingLicenseSeatSlotEngine DefaultPendingLicenseSeatSlotEngine()
    {
        return new PendingLicenseSeatSlotEngine(
            new List<IPendingLicenseSeatSlotModifier>
            {
                new RoundingPendingLicenseSeatSlotAdjuster(0.05),
                new CategoryPendingLicenseSeatSlotAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.34, ["retail"] = 0.9 }),
                new CapPendingLicenseSeatSlotAdjuster(36097.0),
                new TieredPendingLicenseSeatSlotAdjuster(
                    new List<double> { 7488.0, 7935.0 },
                    new List<double> { 0.84, 1.18 }),
                new MinimumPendingLicenseSeatSlotAdjuster(6.5),
            },
            new List<string>(),
            0.4,
            73085.8);
    }

    public double Compute(PendingLicenseSeatSlot item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PendingLicenseSeatSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
