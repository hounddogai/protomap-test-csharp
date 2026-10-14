namespace Acme.Workforce.Assets.Refresh.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh batch calculation.</summary>
public interface IRefreshBatchModifier
{
    string Describe();
    double Adjust(RefreshBatch item, double valueValue);
}

public sealed record CapRefreshBatchAdjuster(double Cap) : IRefreshBatchModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RefreshBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredRefreshBatchAdjuster(List<double> Bounds, List<double> Rates) : IRefreshBatchModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RefreshBatch item, double valueValue)
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

public sealed record MinimumRefreshBatchAdjuster(double Minimum) : IRefreshBatchModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RefreshBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentRefreshBatchAdjuster(double Percent) : IRefreshBatchModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RefreshBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRefreshBatchAdjuster(double Step) : IRefreshBatchModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RefreshBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ChannelRefreshBatchAdjuster(Dictionary<string, double> Factors) : IRefreshBatchModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(RefreshBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the refresh batch steps in order and keeps a trace of each.</summary>
public sealed class RefreshBatchEngine(List<IRefreshBatchModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshBatchModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshBatchEngine DefaultRefreshBatchEngine()
    {
        return new RefreshBatchEngine(
            new List<IRefreshBatchModifier>
            {
                new CapRefreshBatchAdjuster(38444.0),
                new TieredRefreshBatchAdjuster(
                    new List<double> { 1877.0, 1878.0, 2995.0, 9778.0 },
                    new List<double> { 1.08, 1.14, 0.93, 1.16 }),
                new MinimumRefreshBatchAdjuster(39.7),
                new PercentRefreshBatchAdjuster(1.9),
                new RoundingRefreshBatchAdjuster(0.5),
                new ChannelRefreshBatchAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.01, ["retail"] = 0.83, ["south"] = 1.24 }),
            },
            new List<string>(),
            2.2,
            11648.9);
    }

    public double Price(RefreshBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<RefreshBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
