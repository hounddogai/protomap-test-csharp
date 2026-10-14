namespace Acme.Workforce.Talent.Mobility.Preference;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the seasonal mobility manifest calculation.</summary>
public interface ISeasonalMobilityManifestModifier
{
    string Describe();
    double Adjust(SeasonalMobilityManifest item, double valueValue);
}

public sealed record TieredSeasonalMobilityManifestAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalMobilityManifestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalMobilityManifest item, double valueValue)
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

public sealed record CapSeasonalMobilityManifestAdjuster(double Cap) : ISeasonalMobilityManifestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalMobilityManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumSeasonalMobilityManifestAdjuster(double Minimum) : ISeasonalMobilityManifestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalMobilityManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentSeasonalMobilityManifestAdjuster(double Percent) : ISeasonalMobilityManifestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalMobilityManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SeasonalMobilityManifestBudgetOffsetAdjuster(double Weight) : ISeasonalMobilityManifestModifier
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(SeasonalMobilityManifest item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record FlaggedSeasonalMobilityManifestAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalMobilityManifestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalMobilityManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the seasonal mobility manifest steps in order and keeps a trace of each.</summary>
public sealed class SeasonalMobilityManifestEngine(List<ISeasonalMobilityManifestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalMobilityManifestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalMobilityManifestEngine DefaultSeasonalMobilityManifestEngine(Registry flags)
    {
        return new SeasonalMobilityManifestEngine(
            new List<ISeasonalMobilityManifestModifier>
            {
                new TieredSeasonalMobilityManifestAdjuster(
                    new List<double> { 2649.0, 8107.0, 8883.0 },
                    new List<double> { 1.15, 1.02, 1.01 }),
                new CapSeasonalMobilityManifestAdjuster(30419.0),
                new MinimumSeasonalMobilityManifestAdjuster(28.0),
                new PercentSeasonalMobilityManifestAdjuster(-14.8),
                new SeasonalMobilityManifestBudgetOffsetAdjuster(0.69),
                new FlaggedSeasonalMobilityManifestAdjuster(flags, "seasonal-mobility-manifest-adaptive", -4.6),
            },
            new List<string>(),
            3.6,
            49776.7);
    }

    public double Evaluate(SeasonalMobilityManifest item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalMobilityManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
