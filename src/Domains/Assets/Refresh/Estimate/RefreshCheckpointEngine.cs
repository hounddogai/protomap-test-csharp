namespace Acme.Workforce.Assets.Refresh.Estimate;

/// <summary>One step of the refresh checkpoint calculation.</summary>
public interface IRefreshCheckpointFactor
{
    string Describe();
    double Adjust(RefreshCheckpoint item, double valueValue);
}

public sealed record CapRefreshCheckpointAdjuster(double Cap) : IRefreshCheckpointFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RefreshCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRefreshCheckpointAdjuster(double Percent) : IRefreshCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RefreshCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRefreshCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IRefreshCheckpointFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RefreshCheckpoint item, double valueValue)
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

public sealed record MinimumRefreshCheckpointAdjuster(double Minimum) : IRefreshCheckpointFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RefreshCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the refresh checkpoint steps in order and keeps a trace of each.</summary>
public sealed class RefreshCheckpointEngine(List<IRefreshCheckpointFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshCheckpointFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshCheckpointEngine DefaultRefreshCheckpointEngine()
    {
        return new RefreshCheckpointEngine(
            new List<IRefreshCheckpointFactor>
            {
                new CapRefreshCheckpointAdjuster(36717.0),
                new PercentRefreshCheckpointAdjuster(-14.2),
                new TieredRefreshCheckpointAdjuster(
                    new List<double> { 3508.0, 4211.0, 4501.0, 6384.0 },
                    new List<double> { 0.99, 0.95, 0.82, 0.93 }),
                new MinimumRefreshCheckpointAdjuster(27.0),
            },
            new List<string>(),
            2.2,
            43784.3);
    }

    public double Compute(RefreshCheckpoint item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<RefreshCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
