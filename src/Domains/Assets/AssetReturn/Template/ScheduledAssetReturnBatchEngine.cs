namespace Acme.Workforce.Assets.AssetReturn.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled asset return batch calculation.</summary>
public interface IScheduledAssetReturnBatchStep
{
    string Describe();
    double Adjust(ScheduledAssetReturnBatch item, double valueValue);
}

public sealed record ScheduledAssetReturnBatchDurationMinutesOffsetAdjuster(double Weight) : IScheduledAssetReturnBatchStep
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(ScheduledAssetReturnBatch item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record PercentScheduledAssetReturnBatchAdjuster(double Percent) : IScheduledAssetReturnBatchStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledAssetReturnBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapScheduledAssetReturnBatchAdjuster(double Cap) : IScheduledAssetReturnBatchStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledAssetReturnBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the scheduled asset return batch steps in order and keeps a trace of each.</summary>
public sealed class ScheduledAssetReturnBatchEngine(List<IScheduledAssetReturnBatchStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledAssetReturnBatchStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledAssetReturnBatchEngine DefaultScheduledAssetReturnBatchEngine()
    {
        return new ScheduledAssetReturnBatchEngine(
            new List<IScheduledAssetReturnBatchStep>
            {
                new ScheduledAssetReturnBatchDurationMinutesOffsetAdjuster(0.61),
                new PercentScheduledAssetReturnBatchAdjuster(22.1),
                new CapScheduledAssetReturnBatchAdjuster(6840.0),
            },
            new List<string>(),
            4.8,
            53423.0);
    }

    public double Evaluate(ScheduledAssetReturnBatch item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledAssetReturnBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
