namespace Acme.Workforce.Talent.Nomination.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy nomination rollup calculation.</summary>
public interface ILegacyNominationRollupFactor
{
    string Describe();
    double Adjust(LegacyNominationRollup item, double valueValue);
}

public sealed record LegacyNominationRollupUpdatedAtOffsetAdjuster(double Weight) : ILegacyNominationRollupFactor
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(LegacyNominationRollup item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record PercentLegacyNominationRollupAdjuster(double Percent) : ILegacyNominationRollupFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyNominationRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLegacyNominationRollupAdjuster(double Step) : ILegacyNominationRollupFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyNominationRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredLegacyNominationRollupAdjuster(List<double> Bounds, List<double> Rates) : ILegacyNominationRollupFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyNominationRollup item, double valueValue)
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

public sealed record MinimumLegacyNominationRollupAdjuster(double Minimum) : ILegacyNominationRollupFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyNominationRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedLegacyNominationRollupAdjuster(Registry Flags, string Flag, double Percent) : ILegacyNominationRollupFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyNominationRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy nomination rollup steps in order and keeps a trace of each.</summary>
public sealed class LegacyNominationRollupEngine(List<ILegacyNominationRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyNominationRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyNominationRollupEngine DefaultLegacyNominationRollupEngine(Registry flags)
    {
        return new LegacyNominationRollupEngine(
            new List<ILegacyNominationRollupFactor>
            {
                new LegacyNominationRollupUpdatedAtOffsetAdjuster(0.66),
                new PercentLegacyNominationRollupAdjuster(-5.0),
                new RoundingLegacyNominationRollupAdjuster(0.1),
                new TieredLegacyNominationRollupAdjuster(
                    new List<double> { 1887.0, 3440.0 },
                    new List<double> { 1.13, 0.84 }),
                new MinimumLegacyNominationRollupAdjuster(13.1),
                new FlaggedLegacyNominationRollupAdjuster(flags, "legacy-nomination-rollup-manual", -2.7),
            },
            new List<string>(),
            3.3,
            51882.9);
    }

    public double Compute(LegacyNominationRollup item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyNominationRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
