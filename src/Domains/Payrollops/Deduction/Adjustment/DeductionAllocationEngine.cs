namespace Acme.Workforce.Payrollops.Deduction.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the deduction allocation calculation.</summary>
public interface IDeductionAllocationFactor
{
    string Describe();
    double Adjust(DeductionAllocation item, double valueValue);
}

public sealed record FlaggedDeductionAllocationAdjuster(Registry Flags, string Flag, double Percent) : IDeductionAllocationFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeductionAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentDeductionAllocationAdjuster(double Percent) : IDeductionAllocationFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeductionAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DeductionAllocationCreatedAtOffsetAdjuster(double Weight) : IDeductionAllocationFactor
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(DeductionAllocation item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

/// <summary>Runs the deduction allocation steps in order and keeps a trace of each.</summary>
public sealed class DeductionAllocationEngine(List<IDeductionAllocationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionAllocationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionAllocationEngine DefaultDeductionAllocationEngine(Registry flags)
    {
        return new DeductionAllocationEngine(
            new List<IDeductionAllocationFactor>
            {
                new FlaggedDeductionAllocationAdjuster(flags, "deduction-allocation-strict", -0.4),
                new PercentDeductionAllocationAdjuster(14.0),
                new DeductionAllocationCreatedAtOffsetAdjuster(1.98),
            },
            new List<string>(),
            2.0,
            30814.2);
    }

    public double Compute(DeductionAllocation item)
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

    public Dictionary<string, double> EvaluateAll(List<DeductionAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
