namespace Acme.Workforce.Workplace.Visitor.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal visitor checkpoint calculation.</summary>
public interface ISeasonalVisitorCheckpointStep
{
    string Describe();
    double Adjust(SeasonalVisitorCheckpoint item, double valueValue);
}

public sealed record SeasonalVisitorCheckpointWindowDaysOffsetAdjuster(double Weight) : ISeasonalVisitorCheckpointStep
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(SeasonalVisitorCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record PercentSeasonalVisitorCheckpointAdjuster(double Percent) : ISeasonalVisitorCheckpointStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalVisitorCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocaleSeasonalVisitorCheckpointAdjuster(Dictionary<string, double> Factors) : ISeasonalVisitorCheckpointStep
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(SeasonalVisitorCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedSeasonalVisitorCheckpointAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalVisitorCheckpointStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalVisitorCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumSeasonalVisitorCheckpointAdjuster(double Minimum) : ISeasonalVisitorCheckpointStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalVisitorCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapSeasonalVisitorCheckpointAdjuster(double Cap) : ISeasonalVisitorCheckpointStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalVisitorCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the seasonal visitor checkpoint steps in order and keeps a trace of each.</summary>
public sealed class SeasonalVisitorCheckpointEngine(List<ISeasonalVisitorCheckpointStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalVisitorCheckpointStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalVisitorCheckpointEngine DefaultSeasonalVisitorCheckpointEngine(Registry flags)
    {
        return new SeasonalVisitorCheckpointEngine(
            new List<ISeasonalVisitorCheckpointStep>
            {
                new SeasonalVisitorCheckpointWindowDaysOffsetAdjuster(1.5),
                new PercentSeasonalVisitorCheckpointAdjuster(21.2),
                new LocaleSeasonalVisitorCheckpointAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.94, ["west"] = 1.14, ["east"] = 1.06 }),
                new FlaggedSeasonalVisitorCheckpointAdjuster(flags, "seasonal-visitor-checkpoint-batched", -4.8),
                new MinimumSeasonalVisitorCheckpointAdjuster(32.5),
                new CapSeasonalVisitorCheckpointAdjuster(8650.0),
            },
            new List<string>(),
            0.7,
            64470.5);
    }

    public double Price(SeasonalVisitorCheckpoint item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalVisitorCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
