namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal span profile calculation.</summary>
public interface ISeasonalSpanProfileModifier
{
    string Describe();
    double Adjust(SeasonalSpanProfile item, double valueValue);
}

public sealed record FlaggedSeasonalSpanProfileAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalSpanProfileModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalSpanProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSeasonalSpanProfileAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalSpanProfileModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalSpanProfile item, double valueValue)
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

public sealed record CapSeasonalSpanProfileAdjuster(double Cap) : ISeasonalSpanProfileModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalSpanProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumSeasonalSpanProfileAdjuster(double Minimum) : ISeasonalSpanProfileModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalSpanProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameSeasonalSpanProfileAdjuster(Dictionary<string, double> Factors) : ISeasonalSpanProfileModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(SeasonalSpanProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentSeasonalSpanProfileAdjuster(double Percent) : ISeasonalSpanProfileModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalSpanProfile item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the seasonal span profile steps in order and keeps a trace of each.</summary>
public sealed class SeasonalSpanProfileEngine(List<ISeasonalSpanProfileModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalSpanProfileModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalSpanProfileEngine DefaultSeasonalSpanProfileEngine(Registry flags)
    {
        return new SeasonalSpanProfileEngine(
            new List<ISeasonalSpanProfileModifier>
            {
                new FlaggedSeasonalSpanProfileAdjuster(flags, "seasonal-span-profile-manual", 0.3),
                new TieredSeasonalSpanProfileAdjuster(
                    new List<double> { 2505.0, 8685.0 },
                    new List<double> { 1.0, 0.87 }),
                new CapSeasonalSpanProfileAdjuster(31773.0),
                new MinimumSeasonalSpanProfileAdjuster(32.4),
                new NameSeasonalSpanProfileAdjuster(new Dictionary<string, double> { ["central"] = 1.13 }),
                new PercentSeasonalSpanProfileAdjuster(17.2),
            },
            new List<string>(),
            4.5,
            92403.3);
    }

    public double Run(SeasonalSpanProfile item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalSpanProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
