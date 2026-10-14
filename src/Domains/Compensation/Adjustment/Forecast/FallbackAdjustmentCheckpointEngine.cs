namespace Acme.Workforce.Compensation.Adjustment.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback adjustment checkpoint calculation.</summary>
public interface IFallbackAdjustmentCheckpointModifier
{
    string Describe();
    double Adjust(FallbackAdjustmentCheckpoint item, double valueValue);
}

public sealed record TieredFallbackAdjustmentCheckpointAdjuster(List<double> Bounds, List<double> Rates) : IFallbackAdjustmentCheckpointModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackAdjustmentCheckpoint item, double valueValue)
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

public sealed record NameFallbackAdjustmentCheckpointAdjuster(Dictionary<string, double> Factors) : IFallbackAdjustmentCheckpointModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(FallbackAdjustmentCheckpoint item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentFallbackAdjustmentCheckpointAdjuster(double Percent) : IFallbackAdjustmentCheckpointModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackAdjustmentCheckpoint item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumFallbackAdjustmentCheckpointAdjuster(double Minimum) : IFallbackAdjustmentCheckpointModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackAdjustmentCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the fallback adjustment checkpoint steps in order and keeps a trace of each.</summary>
public sealed class FallbackAdjustmentCheckpointEngine(List<IFallbackAdjustmentCheckpointModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackAdjustmentCheckpointModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackAdjustmentCheckpointEngine DefaultFallbackAdjustmentCheckpointEngine()
    {
        return new FallbackAdjustmentCheckpointEngine(
            new List<IFallbackAdjustmentCheckpointModifier>
            {
                new TieredFallbackAdjustmentCheckpointAdjuster(
                    new List<double> { 1173.0, 5190.0, 6768.0, 9256.0 },
                    new List<double> { 1.1, 0.9, 1.17, 1.01 }),
                new NameFallbackAdjustmentCheckpointAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.83, ["north"] = 0.97 }),
                new PercentFallbackAdjustmentCheckpointAdjuster(18.8),
                new MinimumFallbackAdjustmentCheckpointAdjuster(27.5),
            },
            new List<string>(),
            3.7,
            4684.5);
    }

    public double Compute(FallbackAdjustmentCheckpoint item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackAdjustmentCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
