namespace Acme.Workforce.Compensation.PayBand.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal pay band checkpoint calculation.</summary>
public interface ISeasonalPayBandCheckpointAdjuster
{
    string Describe();
    double Adjust(SeasonalPayBandCheckpoint item, double valueValue);
}

public sealed record SeasonalPayBandCheckpointWindowDaysOffsetAdjuster(double Weight) : ISeasonalPayBandCheckpointAdjuster
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(SeasonalPayBandCheckpoint item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record TieredSeasonalPayBandCheckpointAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalPayBandCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalPayBandCheckpoint item, double valueValue)
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

public sealed record RoundingSeasonalPayBandCheckpointAdjuster(double Step) : ISeasonalPayBandCheckpointAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalPayBandCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumSeasonalPayBandCheckpointAdjuster(double Minimum) : ISeasonalPayBandCheckpointAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalPayBandCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the seasonal pay band checkpoint steps in order and keeps a trace of each.</summary>
public sealed class SeasonalPayBandCheckpointEngine(List<ISeasonalPayBandCheckpointAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalPayBandCheckpointAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalPayBandCheckpointEngine DefaultSeasonalPayBandCheckpointEngine()
    {
        return new SeasonalPayBandCheckpointEngine(
            new List<ISeasonalPayBandCheckpointAdjuster>
            {
                new SeasonalPayBandCheckpointWindowDaysOffsetAdjuster(1.37),
                new TieredSeasonalPayBandCheckpointAdjuster(
                    new List<double> { 1322.0, 8935.0, 9665.0 },
                    new List<double> { 1.01, 1.13, 0.89 }),
                new RoundingSeasonalPayBandCheckpointAdjuster(0.5),
                new MinimumSeasonalPayBandCheckpointAdjuster(26.6),
            },
            new List<string>(),
            3.2,
            40038.1);
    }

    public double Run(SeasonalPayBandCheckpoint item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalPayBandCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
