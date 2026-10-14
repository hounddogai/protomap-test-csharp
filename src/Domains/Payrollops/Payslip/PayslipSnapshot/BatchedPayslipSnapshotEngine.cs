namespace Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched payslip snapshot calculation.</summary>
public interface IBatchedPayslipSnapshotFactor
{
    string Describe();
    double Adjust(BatchedPayslipSnapshot item, double valueValue);
}

public sealed record CapBatchedPayslipSnapshotAdjuster(double Cap) : IBatchedPayslipSnapshotFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedPayslipSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredBatchedPayslipSnapshotAdjuster(List<double> Bounds, List<double> Rates) : IBatchedPayslipSnapshotFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedPayslipSnapshot item, double valueValue)
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

public sealed record RegionBatchedPayslipSnapshotAdjuster(Dictionary<string, double> Factors) : IBatchedPayslipSnapshotFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(BatchedPayslipSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record BatchedPayslipSnapshotHoursOffsetAdjuster(double Weight) : IBatchedPayslipSnapshotFactor
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(BatchedPayslipSnapshot item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

/// <summary>Runs the batched payslip snapshot steps in order and keeps a trace of each.</summary>
public sealed class BatchedPayslipSnapshotEngine(List<IBatchedPayslipSnapshotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedPayslipSnapshotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedPayslipSnapshotEngine DefaultBatchedPayslipSnapshotEngine()
    {
        return new BatchedPayslipSnapshotEngine(
            new List<IBatchedPayslipSnapshotFactor>
            {
                new CapBatchedPayslipSnapshotAdjuster(7100.0),
                new TieredBatchedPayslipSnapshotAdjuster(
                    new List<double> { 5901.0, 7586.0 },
                    new List<double> { 0.91, 0.84 }),
                new RegionBatchedPayslipSnapshotAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.24, ["south"] = 0.9 }),
                new BatchedPayslipSnapshotHoursOffsetAdjuster(0.56),
            },
            new List<string>(),
            0.4,
            61436.2);
    }

    public double Evaluate(BatchedPayslipSnapshot item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedPayslipSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
