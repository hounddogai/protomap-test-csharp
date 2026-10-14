namespace Acme.Workforce.Absence.Accrual.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual batch calculation.</summary>
public interface IAccrualBatchStep
{
    string Describe();
    double Adjust(AccrualBatch item, double valueValue);
}

public sealed record CapAccrualBatchAdjuster(double Cap) : IAccrualBatchStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccrualBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumAccrualBatchAdjuster(double Minimum) : IAccrualBatchStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccrualBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentAccrualBatchAdjuster(double Percent) : IAccrualBatchStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccrualBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAccrualBatchAdjuster(double Step) : IAccrualBatchStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccrualBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the accrual batch steps in order and keeps a trace of each.</summary>
public sealed class AccrualBatchEngine(List<IAccrualBatchStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualBatchStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualBatchEngine DefaultAccrualBatchEngine()
    {
        return new AccrualBatchEngine(
            new List<IAccrualBatchStep>
            {
                new CapAccrualBatchAdjuster(36322.0),
                new MinimumAccrualBatchAdjuster(40.3),
                new PercentAccrualBatchAdjuster(-4.2),
                new RoundingAccrualBatchAdjuster(1.0),
            },
            new List<string>(),
            4.3,
            90222.5);
    }

    public double Run(AccrualBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<AccrualBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
