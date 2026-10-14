namespace Acme.Workforce.Compensation.MeritCycle.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered merit cycle checkpoint calculation.</summary>
public interface ITieredMeritCycleCheckpointAdjuster
{
    string Describe();
    double Adjust(TieredMeritCycleCheckpoint item, double valueValue);
}

public sealed record PercentTieredMeritCycleCheckpointAdjuster(double Percent) : ITieredMeritCycleCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredMeritCycleCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredMeritCycleCheckpointCeilingOffsetAdjuster(double Weight) : ITieredMeritCycleCheckpointAdjuster
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(TieredMeritCycleCheckpoint item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record TitleTieredMeritCycleCheckpointAdjuster(Dictionary<string, double> Factors) : ITieredMeritCycleCheckpointAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(TieredMeritCycleCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredTieredMeritCycleCheckpointAdjuster(List<double> Bounds, List<double> Rates) : ITieredMeritCycleCheckpointAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredMeritCycleCheckpoint item, double valueValue)
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

public sealed record MinimumTieredMeritCycleCheckpointAdjuster(double Minimum) : ITieredMeritCycleCheckpointAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredMeritCycleCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the tiered merit cycle checkpoint steps in order and keeps a trace of each.</summary>
public sealed class TieredMeritCycleCheckpointEngine(List<ITieredMeritCycleCheckpointAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredMeritCycleCheckpointAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredMeritCycleCheckpointEngine DefaultTieredMeritCycleCheckpointEngine()
    {
        return new TieredMeritCycleCheckpointEngine(
            new List<ITieredMeritCycleCheckpointAdjuster>
            {
                new PercentTieredMeritCycleCheckpointAdjuster(12.7),
                new TieredMeritCycleCheckpointCeilingOffsetAdjuster(1.4),
                new TitleTieredMeritCycleCheckpointAdjuster(new Dictionary<string, double> { ["west"] = 0.8 }),
                new TieredTieredMeritCycleCheckpointAdjuster(
                    new List<double> { 404.0, 1886.0, 7253.0 },
                    new List<double> { 0.87, 0.94, 1.01 }),
                new MinimumTieredMeritCycleCheckpointAdjuster(13.5),
            },
            new List<string>(),
            0.6,
            87629.1);
    }

    public double Run(TieredMeritCycleCheckpoint item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TieredMeritCycleCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
