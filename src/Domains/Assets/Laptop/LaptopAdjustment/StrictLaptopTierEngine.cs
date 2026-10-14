namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict laptop tier calculation.</summary>
public interface IStrictLaptopTierStep
{
    string Describe();
    double Adjust(StrictLaptopTier item, double valueValue);
}

public sealed record CapStrictLaptopTierAdjuster(double Cap) : IStrictLaptopTierStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictLaptopTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentStrictLaptopTierAdjuster(double Percent) : IStrictLaptopTierStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictLaptopTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record StrictLaptopTierRetriesOffsetAdjuster(double Weight) : IStrictLaptopTierStep
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(StrictLaptopTier item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record TieredStrictLaptopTierAdjuster(List<double> Bounds, List<double> Rates) : IStrictLaptopTierStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictLaptopTier item, double valueValue)
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

public sealed record FlaggedStrictLaptopTierAdjuster(Registry Flags, string Flag, double Percent) : IStrictLaptopTierStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictLaptopTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the strict laptop tier steps in order and keeps a trace of each.</summary>
public sealed class StrictLaptopTierEngine(List<IStrictLaptopTierStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictLaptopTierStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictLaptopTierEngine DefaultStrictLaptopTierEngine(Registry flags)
    {
        return new StrictLaptopTierEngine(
            new List<IStrictLaptopTierStep>
            {
                new CapStrictLaptopTierAdjuster(22241.0),
                new PercentStrictLaptopTierAdjuster(21.7),
                new StrictLaptopTierRetriesOffsetAdjuster(1.92),
                new TieredStrictLaptopTierAdjuster(
                    new List<double> { 4472.0, 9958.0 },
                    new List<double> { 1.12, 1.02 }),
                new FlaggedStrictLaptopTierAdjuster(flags, "strict-laptop-tier-fallback", 9.2),
            },
            new List<string>(),
            1.2,
            36404.9);
    }

    public double Price(StrictLaptopTier item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<StrictLaptopTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
