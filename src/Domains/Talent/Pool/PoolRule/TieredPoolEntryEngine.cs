namespace Acme.Workforce.Talent.Pool.PoolRule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered pool entry calculation.</summary>
public interface ITieredPoolEntryStep
{
    string Describe();
    double Adjust(TieredPoolEntry item, double valueValue);
}

public sealed record PercentTieredPoolEntryAdjuster(double Percent) : ITieredPoolEntryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredPoolEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTieredPoolEntryAdjuster(double Step) : ITieredPoolEntryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredPoolEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedTieredPoolEntryAdjuster(Registry Flags, string Flag, double Percent) : ITieredPoolEntryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredPoolEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CategoryTieredPoolEntryAdjuster(Dictionary<string, double> Factors) : ITieredPoolEntryStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(TieredPoolEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPoolEntryMaximumOffsetAdjuster(double Weight) : ITieredPoolEntryStep
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(TieredPoolEntry item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

/// <summary>Runs the tiered pool entry steps in order and keeps a trace of each.</summary>
public sealed class TieredPoolEntryEngine(List<ITieredPoolEntryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredPoolEntryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredPoolEntryEngine DefaultTieredPoolEntryEngine(Registry flags)
    {
        return new TieredPoolEntryEngine(
            new List<ITieredPoolEntryStep>
            {
                new PercentTieredPoolEntryAdjuster(-16.2),
                new RoundingTieredPoolEntryAdjuster(0.5),
                new FlaggedTieredPoolEntryAdjuster(flags, "tiered-pool-entry-manual", -3.2),
                new CategoryTieredPoolEntryAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.08, ["retail"] = 0.93 }),
                new TieredPoolEntryMaximumOffsetAdjuster(0.34),
            },
            new List<string>(),
            3.4,
            33917.1);
    }

    public double Run(TieredPoolEntry item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<TieredPoolEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
