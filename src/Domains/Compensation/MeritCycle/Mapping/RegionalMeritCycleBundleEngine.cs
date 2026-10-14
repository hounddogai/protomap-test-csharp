namespace Acme.Workforce.Compensation.MeritCycle.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional merit cycle bundle calculation.</summary>
public interface IRegionalMeritCycleBundleAdjuster
{
    string Describe();
    double Adjust(RegionalMeritCycleBundle item, double valueValue);
}

public sealed record TeamRegionalMeritCycleBundleAdjuster(Dictionary<string, double> Factors) : IRegionalMeritCycleBundleAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(RegionalMeritCycleBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingRegionalMeritCycleBundleAdjuster(double Step) : IRegionalMeritCycleBundleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalMeritCycleBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentRegionalMeritCycleBundleAdjuster(double Percent) : IRegionalMeritCycleBundleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalMeritCycleBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRegionalMeritCycleBundleAdjuster(List<double> Bounds, List<double> Rates) : IRegionalMeritCycleBundleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalMeritCycleBundle item, double valueValue)
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

public sealed record FlaggedRegionalMeritCycleBundleAdjuster(Registry Flags, string Flag, double Percent) : IRegionalMeritCycleBundleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalMeritCycleBundle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional merit cycle bundle steps in order and keeps a trace of each.</summary>
public sealed class RegionalMeritCycleBundleEngine(List<IRegionalMeritCycleBundleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalMeritCycleBundleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalMeritCycleBundleEngine DefaultRegionalMeritCycleBundleEngine(Registry flags)
    {
        return new RegionalMeritCycleBundleEngine(
            new List<IRegionalMeritCycleBundleAdjuster>
            {
                new TeamRegionalMeritCycleBundleAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.97, ["retail"] = 0.71 }),
                new RoundingRegionalMeritCycleBundleAdjuster(5.0),
                new PercentRegionalMeritCycleBundleAdjuster(-4.9),
                new TieredRegionalMeritCycleBundleAdjuster(
                    new List<double> { 3074.0, 8490.0 },
                    new List<double> { 1.18, 0.98 }),
                new FlaggedRegionalMeritCycleBundleAdjuster(flags, "regional-merit-cycle-bundle-capped", -9.4),
            },
            new List<string>(),
            1.4,
            24465.3);
    }

    public double Compute(RegionalMeritCycleBundle item)
    {
        var current = item.AdjustedTotal();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<RegionalMeritCycleBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
