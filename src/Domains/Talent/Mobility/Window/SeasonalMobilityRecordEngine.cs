namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal mobility record calculation.</summary>
public interface ISeasonalMobilityRecordFactor
{
    string Describe();
    double Adjust(SeasonalMobilityRecord item, double valueValue);
}

public sealed record FlaggedSeasonalMobilityRecordAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalMobilityRecordFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalMobilityRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSeasonalMobilityRecordAdjuster(double Cap) : ISeasonalMobilityRecordFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalMobilityRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingSeasonalMobilityRecordAdjuster(double Step) : ISeasonalMobilityRecordFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalMobilityRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumSeasonalMobilityRecordAdjuster(double Minimum) : ISeasonalMobilityRecordFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalMobilityRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentSeasonalMobilityRecordAdjuster(double Percent) : ISeasonalMobilityRecordFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalMobilityRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the seasonal mobility record steps in order and keeps a trace of each.</summary>
public sealed class SeasonalMobilityRecordEngine(List<ISeasonalMobilityRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalMobilityRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalMobilityRecordEngine DefaultSeasonalMobilityRecordEngine(Registry flags)
    {
        return new SeasonalMobilityRecordEngine(
            new List<ISeasonalMobilityRecordFactor>
            {
                new FlaggedSeasonalMobilityRecordAdjuster(flags, "seasonal-mobility-record-rolling", -7.8),
                new CapSeasonalMobilityRecordAdjuster(5202.0),
                new RoundingSeasonalMobilityRecordAdjuster(0.25),
                new MinimumSeasonalMobilityRecordAdjuster(44.2),
                new PercentSeasonalMobilityRecordAdjuster(10.4),
            },
            new List<string>(),
            1.0,
            94664.0);
    }

    public double Price(SeasonalMobilityRecord item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalMobilityRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
