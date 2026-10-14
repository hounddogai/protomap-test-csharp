namespace Acme.Workforce.Engagement.TownHall.Target;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the legacy town hall summary calculation.</summary>
public interface ILegacyTownHallSummaryModifier
{
    string Describe();
    double Adjust(LegacyTownHallSummary item, double valueValue);
}

public sealed record CapLegacyTownHallSummaryAdjuster(double Cap) : ILegacyTownHallSummaryModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyTownHallSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredLegacyTownHallSummaryAdjuster(List<double> Bounds, List<double> Rates) : ILegacyTownHallSummaryModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyTownHallSummary item, double valueValue)
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

public sealed record LegacyTownHallSummaryGraceDaysOffsetAdjuster(double Weight) : ILegacyTownHallSummaryModifier
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(LegacyTownHallSummary item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record FlaggedLegacyTownHallSummaryAdjuster(Registry Flags, string Flag, double Percent) : ILegacyTownHallSummaryModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyTownHallSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLegacyTownHallSummaryAdjuster(double Minimum) : ILegacyTownHallSummaryModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyTownHallSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentLegacyTownHallSummaryAdjuster(double Percent) : ILegacyTownHallSummaryModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyTownHallSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy town hall summary steps in order and keeps a trace of each.</summary>
public sealed class LegacyTownHallSummaryEngine(List<ILegacyTownHallSummaryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyTownHallSummaryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyTownHallSummaryEngine DefaultLegacyTownHallSummaryEngine(Registry flags)
    {
        return new LegacyTownHallSummaryEngine(
            new List<ILegacyTownHallSummaryModifier>
            {
                new CapLegacyTownHallSummaryAdjuster(48135.0),
                new TieredLegacyTownHallSummaryAdjuster(
                    new List<double> { 2294.0, 2334.0, 4235.0 },
                    new List<double> { 0.93, 1.19, 0.9 }),
                new LegacyTownHallSummaryGraceDaysOffsetAdjuster(1.65),
                new FlaggedLegacyTownHallSummaryAdjuster(flags, "legacy-town-hall-summary-tiered", -8.1),
                new MinimumLegacyTownHallSummaryAdjuster(2.5),
                new PercentLegacyTownHallSummaryAdjuster(4.1),
            },
            new List<string>(),
            1.5,
            21916.9);
    }

    public double Run(LegacyTownHallSummary item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyTownHallSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
