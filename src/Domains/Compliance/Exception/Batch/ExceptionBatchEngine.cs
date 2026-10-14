namespace Acme.Workforce.Compliance.Exception.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the exception batch calculation.</summary>
public interface IExceptionBatchAdjuster
{
    string Describe();
    double Adjust(ExceptionBatch item, double valueValue);
}

public sealed record TeamExceptionBatchAdjuster(Dictionary<string, double> Factors) : IExceptionBatchAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(ExceptionBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentExceptionBatchAdjuster(double Percent) : IExceptionBatchAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ExceptionBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedExceptionBatchAdjuster(Registry Flags, string Flag, double Percent) : IExceptionBatchAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ExceptionBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the exception batch steps in order and keeps a trace of each.</summary>
public sealed class ExceptionBatchEngine(List<IExceptionBatchAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IExceptionBatchAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ExceptionBatchEngine DefaultExceptionBatchEngine(Registry flags)
    {
        return new ExceptionBatchEngine(
            new List<IExceptionBatchAdjuster>
            {
                new TeamExceptionBatchAdjuster(new Dictionary<string, double> { ["central"] = 1.03 }),
                new PercentExceptionBatchAdjuster(-20.0),
                new FlaggedExceptionBatchAdjuster(flags, "exception-batch-global", -2.7),
            },
            new List<string>(),
            1.0,
            51068.9);
    }

    public double Evaluate(ExceptionBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<ExceptionBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
