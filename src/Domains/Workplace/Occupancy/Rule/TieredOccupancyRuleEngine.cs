namespace Acme.Workforce.Workplace.Occupancy.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered occupancy rule calculation.</summary>
public interface ITieredOccupancyRuleAdjuster
{
    string Describe();
    double Adjust(TieredOccupancyRule item, double valueValue);
}

public sealed record PercentTieredOccupancyRuleAdjuster(double Percent) : ITieredOccupancyRuleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredOccupancyRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapTieredOccupancyRuleAdjuster(double Cap) : ITieredOccupancyRuleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TieredOccupancyRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedTieredOccupancyRuleAdjuster(Registry Flags, string Flag, double Percent) : ITieredOccupancyRuleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredOccupancyRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTieredOccupancyRuleAdjuster(double Step) : ITieredOccupancyRuleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredOccupancyRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the tiered occupancy rule steps in order and keeps a trace of each.</summary>
public sealed class TieredOccupancyRuleEngine(List<ITieredOccupancyRuleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredOccupancyRuleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredOccupancyRuleEngine DefaultTieredOccupancyRuleEngine(Registry flags)
    {
        return new TieredOccupancyRuleEngine(
            new List<ITieredOccupancyRuleAdjuster>
            {
                new PercentTieredOccupancyRuleAdjuster(-24.5),
                new CapTieredOccupancyRuleAdjuster(47700.0),
                new FlaggedTieredOccupancyRuleAdjuster(flags, "tiered-occupancy-rule-strict", 4.1),
                new RoundingTieredOccupancyRuleAdjuster(5.0),
            },
            new List<string>(),
            1.1,
            44680.3);
    }

    public double Compute(TieredOccupancyRule item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<TieredOccupancyRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
