namespace Acme.Workforce.Engagement.TownHall.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal town hall claim calculation.</summary>
public interface ISeasonalTownHallClaimAdjuster
{
    string Describe();
    double Adjust(SeasonalTownHallClaim item, double valueValue);
}

public sealed record CapSeasonalTownHallClaimAdjuster(double Cap) : ISeasonalTownHallClaimAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalTownHallClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SourceSeasonalTownHallClaimAdjuster(Dictionary<string, double> Factors) : ISeasonalTownHallClaimAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(SeasonalTownHallClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedSeasonalTownHallClaimAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalTownHallClaimAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalTownHallClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSeasonalTownHallClaimAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalTownHallClaimAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalTownHallClaim item, double valueValue)
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

public sealed record SeasonalTownHallClaimAttemptsOffsetAdjuster(double Weight) : ISeasonalTownHallClaimAdjuster
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(SeasonalTownHallClaim item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record MinimumSeasonalTownHallClaimAdjuster(double Minimum) : ISeasonalTownHallClaimAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalTownHallClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the seasonal town hall claim steps in order and keeps a trace of each.</summary>
public sealed class SeasonalTownHallClaimEngine(List<ISeasonalTownHallClaimAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalTownHallClaimAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalTownHallClaimEngine DefaultSeasonalTownHallClaimEngine(Registry flags)
    {
        return new SeasonalTownHallClaimEngine(
            new List<ISeasonalTownHallClaimAdjuster>
            {
                new CapSeasonalTownHallClaimAdjuster(42043.0),
                new SourceSeasonalTownHallClaimAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.86, ["east"] = 0.99 }),
                new FlaggedSeasonalTownHallClaimAdjuster(flags, "seasonal-town-hall-claim-seasonal", -6.9),
                new TieredSeasonalTownHallClaimAdjuster(
                    new List<double> { 2704.0, 6673.0 },
                    new List<double> { 0.8, 1.07 }),
                new SeasonalTownHallClaimAttemptsOffsetAdjuster(0.81),
                new MinimumSeasonalTownHallClaimAdjuster(9.0),
            },
            new List<string>(),
            2.8,
            16565.8);
    }

    public double Price(SeasonalTownHallClaim item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalTownHallClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
