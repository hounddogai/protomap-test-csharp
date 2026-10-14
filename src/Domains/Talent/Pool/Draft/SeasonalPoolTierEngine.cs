namespace Acme.Workforce.Talent.Pool.Draft;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the seasonal pool tier calculation.</summary>
public interface ISeasonalPoolTierModifier
{
    string Describe();
    double Adjust(SeasonalPoolTier item, double valueValue);
}

public sealed record CategorySeasonalPoolTierAdjuster(Dictionary<string, double> Factors) : ISeasonalPoolTierModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(SeasonalPoolTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumSeasonalPoolTierAdjuster(double Minimum) : ISeasonalPoolTierModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalPoolTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentSeasonalPoolTierAdjuster(double Percent) : ISeasonalPoolTierModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalPoolTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SeasonalPoolTierUpdatedAtOffsetAdjuster(double Weight) : ISeasonalPoolTierModifier
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(SeasonalPoolTier item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record FlaggedSeasonalPoolTierAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalPoolTierModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalPoolTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the seasonal pool tier steps in order and keeps a trace of each.</summary>
public sealed class SeasonalPoolTierEngine(List<ISeasonalPoolTierModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalPoolTierModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalPoolTierEngine DefaultSeasonalPoolTierEngine(Registry flags)
    {
        return new SeasonalPoolTierEngine(
            new List<ISeasonalPoolTierModifier>
            {
                new CategorySeasonalPoolTierAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.9, ["central"] = 0.7, ["east"] = 1.06 }),
                new MinimumSeasonalPoolTierAdjuster(29.0),
                new PercentSeasonalPoolTierAdjuster(-6.9),
                new SeasonalPoolTierUpdatedAtOffsetAdjuster(1.77),
                new FlaggedSeasonalPoolTierAdjuster(flags, "seasonal-pool-tier-legacy", -9.5),
            },
            new List<string>(),
            1.0,
            72385.6);
    }

    public double Compute(SeasonalPoolTier item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalPoolTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
