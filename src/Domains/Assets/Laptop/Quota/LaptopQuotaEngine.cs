namespace Acme.Workforce.Assets.Laptop.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the laptop quota calculation.</summary>
public interface ILaptopQuotaAdjuster
{
    string Describe();
    double Adjust(LaptopQuota item, double valueValue);
}

public sealed record RegionLaptopQuotaAdjuster(Dictionary<string, double> Factors) : ILaptopQuotaAdjuster
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(LaptopQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumLaptopQuotaAdjuster(double Minimum) : ILaptopQuotaAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LaptopQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingLaptopQuotaAdjuster(double Step) : ILaptopQuotaAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LaptopQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedLaptopQuotaAdjuster(Registry Flags, string Flag, double Percent) : ILaptopQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LaptopQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLaptopQuotaAdjuster(double Cap) : ILaptopQuotaAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LaptopQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredLaptopQuotaAdjuster(List<double> Bounds, List<double> Rates) : ILaptopQuotaAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LaptopQuota item, double valueValue)
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

/// <summary>Runs the laptop quota steps in order and keeps a trace of each.</summary>
public sealed class LaptopQuotaEngine(List<ILaptopQuotaAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopQuotaAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopQuotaEngine DefaultLaptopQuotaEngine(Registry flags)
    {
        return new LaptopQuotaEngine(
            new List<ILaptopQuotaAdjuster>
            {
                new RegionLaptopQuotaAdjuster(new Dictionary<string, double> { ["north"] = 1.11, ["east"] = 1.21 }),
                new MinimumLaptopQuotaAdjuster(14.1),
                new RoundingLaptopQuotaAdjuster(0.25),
                new FlaggedLaptopQuotaAdjuster(flags, "laptop-quota-scheduled", -1.6),
                new CapLaptopQuotaAdjuster(37910.0),
                new TieredLaptopQuotaAdjuster(new List<double> { 8432.0, 8661.0 }, new List<double> { 0.99, 1.1 }),
            },
            new List<string>(),
            1.2,
            57189.0);
    }

    public double Price(LaptopQuota item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LaptopQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
