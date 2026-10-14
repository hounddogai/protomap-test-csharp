namespace Acme.Workforce.Orgdesign.Reorg.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg bundle calculation.</summary>
public interface IReorgBundleStep
{
    string Describe();
    double Adjust(ReorgBundle item, double valueValue);
}

public sealed record PercentReorgBundleAdjuster(double Percent) : IReorgBundleStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReorgBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredReorgBundleAdjuster(List<double> Bounds, List<double> Rates) : IReorgBundleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReorgBundle item, double valueValue)
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

public sealed record RoundingReorgBundleAdjuster(double Step) : IReorgBundleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReorgBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ReorgBundleQuantityOffsetAdjuster(double Weight) : IReorgBundleStep
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(ReorgBundle item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record CapReorgBundleAdjuster(double Cap) : IReorgBundleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReorgBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedReorgBundleAdjuster(Registry Flags, string Flag, double Percent) : IReorgBundleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReorgBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the reorg bundle steps in order and keeps a trace of each.</summary>
public sealed class ReorgBundleEngine(List<IReorgBundleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgBundleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgBundleEngine DefaultReorgBundleEngine(Registry flags)
    {
        return new ReorgBundleEngine(
            new List<IReorgBundleStep>
            {
                new PercentReorgBundleAdjuster(-2.6),
                new TieredReorgBundleAdjuster(new List<double> { 1811.0, 5301.0 }, new List<double> { 1.18, 0.94 }),
                new RoundingReorgBundleAdjuster(0.1),
                new ReorgBundleQuantityOffsetAdjuster(1.26),
                new CapReorgBundleAdjuster(10963.0),
                new FlaggedReorgBundleAdjuster(flags, "reorg-bundle-global", -6.7),
            },
            new List<string>(),
            3.6,
            66089.8);
    }

    public double Price(ReorgBundle item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<ReorgBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
