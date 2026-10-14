namespace Acme.Workforce.Compensation.MeritCycle.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal merit cycle entry calculation.</summary>
public interface ISeasonalMeritCycleEntryAdjuster
{
    string Describe();
    double Adjust(SeasonalMeritCycleEntry item, double valueValue);
}

public sealed record TieredSeasonalMeritCycleEntryAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalMeritCycleEntryAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalMeritCycleEntry item, double valueValue)
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

public sealed record LabelSeasonalMeritCycleEntryAdjuster(Dictionary<string, double> Factors) : ISeasonalMeritCycleEntryAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(SeasonalMeritCycleEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedSeasonalMeritCycleEntryAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalMeritCycleEntryAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalMeritCycleEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentSeasonalMeritCycleEntryAdjuster(double Percent) : ISeasonalMeritCycleEntryAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalMeritCycleEntry item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSeasonalMeritCycleEntryAdjuster(double Cap) : ISeasonalMeritCycleEntryAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalMeritCycleEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the seasonal merit cycle entry steps in order and keeps a trace of each.</summary>
public sealed class SeasonalMeritCycleEntryEngine(List<ISeasonalMeritCycleEntryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalMeritCycleEntryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalMeritCycleEntryEngine DefaultSeasonalMeritCycleEntryEngine(Registry flags)
    {
        return new SeasonalMeritCycleEntryEngine(
            new List<ISeasonalMeritCycleEntryAdjuster>
            {
                new TieredSeasonalMeritCycleEntryAdjuster(
                    new List<double> { 810.0, 4074.0 },
                    new List<double> { 0.86, 1.11 }),
                new LabelSeasonalMeritCycleEntryAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.89, ["west"] = 1.18, ["north"] = 0.74 }),
                new FlaggedSeasonalMeritCycleEntryAdjuster(flags, "seasonal-merit-cycle-entry-lenient", -6.4),
                new PercentSeasonalMeritCycleEntryAdjuster(4.4),
                new CapSeasonalMeritCycleEntryAdjuster(14863.0),
            },
            new List<string>(),
            3.3,
            1158.3);
    }

    public double Compute(SeasonalMeritCycleEntry item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalMeritCycleEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
