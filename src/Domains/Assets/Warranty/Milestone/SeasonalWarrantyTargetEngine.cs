namespace Acme.Workforce.Assets.Warranty.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal warranty target calculation.</summary>
public interface ISeasonalWarrantyTargetFactor
{
    string Describe();
    double Adjust(SeasonalWarrantyTarget item, double valueValue);
}

public sealed record FlaggedSeasonalWarrantyTargetAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalWarrantyTargetFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalWarrantyTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentSeasonalWarrantyTargetAdjuster(double Percent) : ISeasonalWarrantyTargetFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalWarrantyTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SeasonalWarrantyTargetFloorValueOffsetAdjuster(double Weight) : ISeasonalWarrantyTargetFactor
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(SeasonalWarrantyTarget item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record RoundingSeasonalWarrantyTargetAdjuster(double Step) : ISeasonalWarrantyTargetFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalWarrantyTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapSeasonalWarrantyTargetAdjuster(double Cap) : ISeasonalWarrantyTargetFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalWarrantyTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CountrySeasonalWarrantyTargetAdjuster(Dictionary<string, double> Factors) : ISeasonalWarrantyTargetFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(SeasonalWarrantyTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the seasonal warranty target steps in order and keeps a trace of each.</summary>
public sealed class SeasonalWarrantyTargetEngine(List<ISeasonalWarrantyTargetFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalWarrantyTargetFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalWarrantyTargetEngine DefaultSeasonalWarrantyTargetEngine(Registry flags)
    {
        return new SeasonalWarrantyTargetEngine(
            new List<ISeasonalWarrantyTargetFactor>
            {
                new FlaggedSeasonalWarrantyTargetAdjuster(flags, "seasonal-warranty-target-scheduled", -6.0),
                new PercentSeasonalWarrantyTargetAdjuster(-6.3),
                new SeasonalWarrantyTargetFloorValueOffsetAdjuster(1.24),
                new RoundingSeasonalWarrantyTargetAdjuster(0.05),
                new CapSeasonalWarrantyTargetAdjuster(13923.0),
                new CountrySeasonalWarrantyTargetAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.8, ["west"] = 1.28, ["north"] = 0.81 }),
            },
            new List<string>(),
            4.7,
            84740.9);
    }

    public double Run(SeasonalWarrantyTarget item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalWarrantyTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
