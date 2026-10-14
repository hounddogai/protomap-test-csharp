namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched payslip claim calculation.</summary>
public interface IBatchedPayslipClaimAdjuster
{
    string Describe();
    double Adjust(BatchedPayslipClaim item, double valueValue);
}

public sealed record CapBatchedPayslipClaimAdjuster(double Cap) : IBatchedPayslipClaimAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedPayslipClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingBatchedPayslipClaimAdjuster(double Step) : IBatchedPayslipClaimAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedPayslipClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedBatchedPayslipClaimAdjuster(Registry Flags, string Flag, double Percent) : IBatchedPayslipClaimAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedPayslipClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredBatchedPayslipClaimAdjuster(List<double> Bounds, List<double> Rates) : IBatchedPayslipClaimAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(BatchedPayslipClaim item, double valueValue)
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

/// <summary>Runs the batched payslip claim steps in order and keeps a trace of each.</summary>
public sealed class BatchedPayslipClaimEngine(List<IBatchedPayslipClaimAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedPayslipClaimAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedPayslipClaimEngine DefaultBatchedPayslipClaimEngine(Registry flags)
    {
        return new BatchedPayslipClaimEngine(
            new List<IBatchedPayslipClaimAdjuster>
            {
                new CapBatchedPayslipClaimAdjuster(32361.0),
                new RoundingBatchedPayslipClaimAdjuster(0.5),
                new FlaggedBatchedPayslipClaimAdjuster(flags, "batched-payslip-claim-capped", -2.5),
                new TieredBatchedPayslipClaimAdjuster(
                    new List<double> { 4285.0, 4402.0, 5755.0 },
                    new List<double> { 1.01, 0.93, 1.17 }),
            },
            new List<string>(),
            4.4,
            91507.3);
    }

    public double Compute(BatchedPayslipClaim item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<BatchedPayslipClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
