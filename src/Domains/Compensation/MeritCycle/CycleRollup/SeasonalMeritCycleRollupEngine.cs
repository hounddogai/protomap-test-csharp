namespace Acme.Workforce.Compensation.MeritCycle.CycleRollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal merit cycle rollup calculation.</summary>
public interface ISeasonalMeritCycleRollupModifier
{
    string Describe();
    double Adjust(SeasonalMeritCycleRollup item, double valueValue);
}

public sealed record PercentSeasonalMeritCycleRollupAdjuster(double Percent) : ISeasonalMeritCycleRollupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalMeritCycleRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSeasonalMeritCycleRollupAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalMeritCycleRollupModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalMeritCycleRollup item, double valueValue)
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

public sealed record MinimumSeasonalMeritCycleRollupAdjuster(double Minimum) : ISeasonalMeritCycleRollupModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalMeritCycleRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleSeasonalMeritCycleRollupAdjuster(Dictionary<string, double> Factors) : ISeasonalMeritCycleRollupModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(SeasonalMeritCycleRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedSeasonalMeritCycleRollupAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalMeritCycleRollupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalMeritCycleRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingSeasonalMeritCycleRollupAdjuster(double Step) : ISeasonalMeritCycleRollupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalMeritCycleRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the seasonal merit cycle rollup steps in order and keeps a trace of each.</summary>
public sealed class SeasonalMeritCycleRollupEngine(List<ISeasonalMeritCycleRollupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalMeritCycleRollupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalMeritCycleRollupEngine DefaultSeasonalMeritCycleRollupEngine(Registry flags)
    {
        return new SeasonalMeritCycleRollupEngine(
            new List<ISeasonalMeritCycleRollupModifier>
            {
                new PercentSeasonalMeritCycleRollupAdjuster(-18.8),
                new TieredSeasonalMeritCycleRollupAdjuster(
                    new List<double> { 1573.0, 2760.0, 4810.0 },
                    new List<double> { 0.89, 0.95, 0.95 }),
                new MinimumSeasonalMeritCycleRollupAdjuster(8.3),
                new TitleSeasonalMeritCycleRollupAdjuster(new Dictionary<string, double> { ["retail"] = 1.19 }),
                new FlaggedSeasonalMeritCycleRollupAdjuster(flags, "seasonal-merit-cycle-rollup-default", 1.3),
                new RoundingSeasonalMeritCycleRollupAdjuster(0.05),
            },
            new List<string>(),
            1.2,
            71423.7);
    }

    public double Price(SeasonalMeritCycleRollup item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalMeritCycleRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
