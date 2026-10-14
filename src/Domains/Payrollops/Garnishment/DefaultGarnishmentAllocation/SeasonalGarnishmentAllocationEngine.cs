namespace Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal garnishment allocation calculation.</summary>
public interface ISeasonalGarnishmentAllocationModifier
{
    string Describe();
    double Adjust(SeasonalGarnishmentAllocation item, double valueValue);
}

public sealed record TieredSeasonalGarnishmentAllocationAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalGarnishmentAllocationModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalGarnishmentAllocation item, double valueValue)
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

public sealed record RoundingSeasonalGarnishmentAllocationAdjuster(double Step) : ISeasonalGarnishmentAllocationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalGarnishmentAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedSeasonalGarnishmentAllocationAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalGarnishmentAllocationModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalGarnishmentAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSeasonalGarnishmentAllocationAdjuster(double Minimum) : ISeasonalGarnishmentAllocationModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalGarnishmentAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapSeasonalGarnishmentAllocationAdjuster(double Cap) : ISeasonalGarnishmentAllocationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalGarnishmentAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SeasonalGarnishmentAllocationThresholdOffsetAdjuster(double Weight) : ISeasonalGarnishmentAllocationModifier
{
    public string Describe()
    {
        return $"plus threshold times {Weight}";
    }

    public double Adjust(SeasonalGarnishmentAllocation item, double valueValue)
    {
        return valueValue + item.Threshold * Weight;
    }
}

/// <summary>Runs the seasonal garnishment allocation steps in order and keeps a trace of each.</summary>
public sealed class SeasonalGarnishmentAllocationEngine(List<ISeasonalGarnishmentAllocationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalGarnishmentAllocationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalGarnishmentAllocationEngine DefaultSeasonalGarnishmentAllocationEngine(Registry flags)
    {
        return new SeasonalGarnishmentAllocationEngine(
            new List<ISeasonalGarnishmentAllocationModifier>
            {
                new TieredSeasonalGarnishmentAllocationAdjuster(
                    new List<double> { 2902.0, 4699.0, 5337.0, 6956.0 },
                    new List<double> { 1.1, 0.94, 1.01, 1.2 }),
                new RoundingSeasonalGarnishmentAllocationAdjuster(0.5),
                new FlaggedSeasonalGarnishmentAllocationAdjuster(
                    flags,
                    "seasonal-garnishment-allocation-regional",
                    -9.5),
                new MinimumSeasonalGarnishmentAllocationAdjuster(10.5),
                new CapSeasonalGarnishmentAllocationAdjuster(5296.0),
                new SeasonalGarnishmentAllocationThresholdOffsetAdjuster(0.36),
            },
            new List<string>(),
            3.4,
            57609.1);
    }

    public double Evaluate(SeasonalGarnishmentAllocation item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalGarnishmentAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
