namespace Acme.Workforce.Compensation.MeritCycle.CycleReview;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal merit cycle profile calculation.</summary>
public interface ISeasonalMeritCycleProfileModifier
{
    string Describe();
    double Adjust(SeasonalMeritCycleProfile item, double valueValue);
}

public sealed record MinimumSeasonalMeritCycleProfileAdjuster(double Minimum) : ISeasonalMeritCycleProfileModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalMeritCycleProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingSeasonalMeritCycleProfileAdjuster(double Step) : ISeasonalMeritCycleProfileModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalMeritCycleProfile item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapSeasonalMeritCycleProfileAdjuster(double Cap) : ISeasonalMeritCycleProfileModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalMeritCycleProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredSeasonalMeritCycleProfileAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalMeritCycleProfileModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalMeritCycleProfile item, double valueValue)
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

public sealed record FlaggedSeasonalMeritCycleProfileAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalMeritCycleProfileModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalMeritCycleProfile item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TeamSeasonalMeritCycleProfileAdjuster(Dictionary<string, double> Factors) : ISeasonalMeritCycleProfileModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(SeasonalMeritCycleProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the seasonal merit cycle profile steps in order and keeps a trace of each.</summary>
public sealed class SeasonalMeritCycleProfileEngine(List<ISeasonalMeritCycleProfileModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalMeritCycleProfileModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalMeritCycleProfileEngine DefaultSeasonalMeritCycleProfileEngine(Registry flags)
    {
        return new SeasonalMeritCycleProfileEngine(
            new List<ISeasonalMeritCycleProfileModifier>
            {
                new MinimumSeasonalMeritCycleProfileAdjuster(22.4),
                new RoundingSeasonalMeritCycleProfileAdjuster(1.0),
                new CapSeasonalMeritCycleProfileAdjuster(39666.0),
                new TieredSeasonalMeritCycleProfileAdjuster(
                    new List<double> { 6691.0, 8628.0 },
                    new List<double> { 1.02, 1.18 }),
                new FlaggedSeasonalMeritCycleProfileAdjuster(flags, "seasonal-merit-cycle-profile-pending", -5.9),
                new TeamSeasonalMeritCycleProfileAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.85, ["west"] = 1.02, ["north"] = 1.27 }),
            },
            new List<string>(),
            4.6,
            31445.0);
    }

    public double Evaluate(SeasonalMeritCycleProfile item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalMeritCycleProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
