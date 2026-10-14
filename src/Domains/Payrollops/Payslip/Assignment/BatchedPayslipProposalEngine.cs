namespace Acme.Workforce.Payrollops.Payslip.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the batched payslip proposal calculation.</summary>
public interface IBatchedPayslipProposalAdjuster
{
    string Describe();
    double Adjust(BatchedPayslipProposal item, double valueValue);
}

public sealed record CapBatchedPayslipProposalAdjuster(double Cap) : IBatchedPayslipProposalAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(BatchedPayslipProposal item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record BatchedPayslipProposalBatchSizeOffsetAdjuster(double Weight) : IBatchedPayslipProposalAdjuster
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(BatchedPayslipProposal item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record FlaggedBatchedPayslipProposalAdjuster(Registry Flags, string Flag, double Percent) : IBatchedPayslipProposalAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(BatchedPayslipProposal item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingBatchedPayslipProposalAdjuster(double Step) : IBatchedPayslipProposalAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(BatchedPayslipProposal item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SegmentBatchedPayslipProposalAdjuster(Dictionary<string, double> Factors) : IBatchedPayslipProposalAdjuster
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(BatchedPayslipProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumBatchedPayslipProposalAdjuster(double Minimum) : IBatchedPayslipProposalAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(BatchedPayslipProposal item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the batched payslip proposal steps in order and keeps a trace of each.</summary>
public sealed class BatchedPayslipProposalEngine(List<IBatchedPayslipProposalAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IBatchedPayslipProposalAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static BatchedPayslipProposalEngine DefaultBatchedPayslipProposalEngine(Registry flags)
    {
        return new BatchedPayslipProposalEngine(
            new List<IBatchedPayslipProposalAdjuster>
            {
                new CapBatchedPayslipProposalAdjuster(47370.0),
                new BatchedPayslipProposalBatchSizeOffsetAdjuster(1.73),
                new FlaggedBatchedPayslipProposalAdjuster(flags, "batched-payslip-proposal-fallback", -0.4),
                new RoundingBatchedPayslipProposalAdjuster(5.0),
                new SegmentBatchedPayslipProposalAdjuster(new Dictionary<string, double> { ["north"] = 0.79 }),
                new MinimumBatchedPayslipProposalAdjuster(3.2),
            },
            new List<string>(),
            1.6,
            89929.1);
    }

    public double Evaluate(BatchedPayslipProposal item)
    {
        var current = item.EffectiveTotal();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<BatchedPayslipProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
