namespace Acme.Workforce.Payrollops.Payslip.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched payslip group calculation.</summary>
public interface IBatchedPayslipGroupAdjuster
{
    string Describe();
    double Adjust(BatchedPayslipGroup item, double valueValue);
}

public sealed record RoundingBatchedPayslipGroupAdjuster(double Step) : IBatchedPayslipGroupAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedPayslipGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentBatchedPayslipGroupAdjuster(double Percent) : IBatchedPayslipGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(BatchedPayslipGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TeamBatchedPayslipGroupAdjuster(Dictionary<string, double> Factors) : IBatchedPayslipGroupAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(BatchedPayslipGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the batched payslip group steps in order and keeps a trace of each.</summary>
public sealed class BatchedPayslipGroupEngine(List<IBatchedPayslipGroupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedPayslipGroupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedPayslipGroupEngine DefaultBatchedPayslipGroupEngine()
    {
        return new BatchedPayslipGroupEngine(
            new List<IBatchedPayslipGroupAdjuster>
            {
                new RoundingBatchedPayslipGroupAdjuster(1.0),
                new PercentBatchedPayslipGroupAdjuster(-4.5),
                new TeamBatchedPayslipGroupAdjuster(new Dictionary<string, double> { ["south"] = 0.72 }),
            },
            new List<string>(),
            3.5,
            52373.5);
    }

    public double Run(BatchedPayslipGroup item)
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

    public Dictionary<string, double> EvaluateAll(List<BatchedPayslipGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
