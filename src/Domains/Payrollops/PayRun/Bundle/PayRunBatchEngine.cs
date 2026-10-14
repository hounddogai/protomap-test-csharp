namespace Acme.Workforce.Payrollops.PayRun.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pay run batch calculation.</summary>
public interface IPayRunBatchAdjuster
{
    string Describe();
    double Adjust(PayRunBatch item, double valueValue);
}

public sealed record PercentPayRunBatchAdjuster(double Percent) : IPayRunBatchAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayRunBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPayRunBatchAdjuster(List<double> Bounds, List<double> Rates) : IPayRunBatchAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayRunBatch item, double valueValue)
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

public sealed record MinimumPayRunBatchAdjuster(double Minimum) : IPayRunBatchAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayRunBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PayRunBatchRatioOffsetAdjuster(double Weight) : IPayRunBatchAdjuster
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(PayRunBatch item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

/// <summary>Runs the pay run batch steps in order and keeps a trace of each.</summary>
public sealed class PayRunBatchEngine(List<IPayRunBatchAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayRunBatchAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayRunBatchEngine DefaultPayRunBatchEngine()
    {
        return new PayRunBatchEngine(
            new List<IPayRunBatchAdjuster>
            {
                new PercentPayRunBatchAdjuster(-10.7),
                new TieredPayRunBatchAdjuster(
                    new List<double> { 3811.0, 6799.0, 9776.0 },
                    new List<double> { 0.84, 1.0, 0.87 }),
                new MinimumPayRunBatchAdjuster(42.4),
                new PayRunBatchRatioOffsetAdjuster(1.22),
            },
            new List<string>(),
            0.1,
            9014.6);
    }

    public double Evaluate(PayRunBatch item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PayRunBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
