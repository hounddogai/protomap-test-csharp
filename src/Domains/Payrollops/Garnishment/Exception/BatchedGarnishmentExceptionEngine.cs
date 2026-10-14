namespace Acme.Workforce.Payrollops.Garnishment.Exception;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched garnishment exception calculation.</summary>
public interface IBatchedGarnishmentExceptionStep
{
    string Describe();
    double Adjust(BatchedGarnishmentException item, double valueValue);
}

public sealed record CapBatchedGarnishmentExceptionAdjuster(double Cap) : IBatchedGarnishmentExceptionStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedGarnishmentException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedBatchedGarnishmentExceptionAdjuster(Registry Flags, string Flag, double Percent) : IBatchedGarnishmentExceptionStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedGarnishmentException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record BatchedGarnishmentExceptionRevisionOffsetAdjuster(double Weight) : IBatchedGarnishmentExceptionStep
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(BatchedGarnishmentException item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record PercentBatchedGarnishmentExceptionAdjuster(double Percent) : IBatchedGarnishmentExceptionStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedGarnishmentException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the batched garnishment exception steps in order and keeps a trace of each.</summary>
public sealed class BatchedGarnishmentExceptionEngine(List<IBatchedGarnishmentExceptionStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedGarnishmentExceptionStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedGarnishmentExceptionEngine DefaultBatchedGarnishmentExceptionEngine(Registry flags)
    {
        return new BatchedGarnishmentExceptionEngine(
            new List<IBatchedGarnishmentExceptionStep>
            {
                new CapBatchedGarnishmentExceptionAdjuster(29271.0),
                new FlaggedBatchedGarnishmentExceptionAdjuster(flags, "batched-garnishment-exception-default", 4.0),
                new BatchedGarnishmentExceptionRevisionOffsetAdjuster(1.58),
                new PercentBatchedGarnishmentExceptionAdjuster(-5.8),
            },
            new List<string>(),
            2.3,
            8654.0);
    }

    public double Compute(BatchedGarnishmentException item)
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

    public Dictionary<string, double> EvaluateAll(List<BatchedGarnishmentException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
