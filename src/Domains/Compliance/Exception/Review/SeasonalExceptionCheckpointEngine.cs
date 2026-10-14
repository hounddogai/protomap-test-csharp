namespace Acme.Workforce.Compliance.Exception.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal exception checkpoint calculation.</summary>
public interface ISeasonalExceptionCheckpointFactor
{
    string Describe();
    double Adjust(SeasonalExceptionCheckpoint item, double valueValue);
}

public sealed record FlaggedSeasonalExceptionCheckpointAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalExceptionCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalExceptionCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSeasonalExceptionCheckpointAdjuster(double Cap) : ISeasonalExceptionCheckpointFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalExceptionCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredSeasonalExceptionCheckpointAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalExceptionCheckpointFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalExceptionCheckpoint item, double valueValue)
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

public sealed record MinimumSeasonalExceptionCheckpointAdjuster(double Minimum) : ISeasonalExceptionCheckpointFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalExceptionCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingSeasonalExceptionCheckpointAdjuster(double Step) : ISeasonalExceptionCheckpointFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalExceptionCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the seasonal exception checkpoint steps in order and keeps a trace of each.</summary>
public sealed class SeasonalExceptionCheckpointEngine(List<ISeasonalExceptionCheckpointFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalExceptionCheckpointFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalExceptionCheckpointEngine DefaultSeasonalExceptionCheckpointEngine(Registry flags)
    {
        return new SeasonalExceptionCheckpointEngine(
            new List<ISeasonalExceptionCheckpointFactor>
            {
                new FlaggedSeasonalExceptionCheckpointAdjuster(flags, "seasonal-exception-checkpoint-pending", 2.1),
                new CapSeasonalExceptionCheckpointAdjuster(43519.0),
                new TieredSeasonalExceptionCheckpointAdjuster(
                    new List<double> { 438.0, 2611.0, 5452.0 },
                    new List<double> { 1.13, 1.14, 0.97 }),
                new MinimumSeasonalExceptionCheckpointAdjuster(3.6),
                new RoundingSeasonalExceptionCheckpointAdjuster(1.0),
            },
            new List<string>(),
            0.7,
            42776.8);
    }

    public double Price(SeasonalExceptionCheckpoint item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalExceptionCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
