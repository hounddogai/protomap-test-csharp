namespace Acme.Workforce.Payrollops.Deduction.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the deduction adjustment calculation.</summary>
public interface IDeductionAdjustmentAdjuster
{
    string Describe();
    double Adjust(DeductionAdjustment item, double valueValue);
}

public sealed record FlaggedDeductionAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : IDeductionAdjustmentAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeductionAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapDeductionAdjustmentAdjuster(double Cap) : IDeductionAdjustmentAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeductionAdjustment item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingDeductionAdjustmentAdjuster(double Step) : IDeductionAdjustmentAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeductionAdjustment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the deduction adjustment steps in order and keeps a trace of each.</summary>
public sealed class DeductionAdjustmentEngine(List<IDeductionAdjustmentAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionAdjustmentAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionAdjustmentEngine DefaultDeductionAdjustmentEngine(Registry flags)
    {
        return new DeductionAdjustmentEngine(
            new List<IDeductionAdjustmentAdjuster>
            {
                new FlaggedDeductionAdjustmentAdjuster(flags, "deduction-adjustment-default", 5.2),
                new CapDeductionAdjustmentAdjuster(20082.0),
                new RoundingDeductionAdjustmentAdjuster(1.0),
            },
            new List<string>(),
            0.8,
            66841.5);
    }

    public double Compute(DeductionAdjustment item)
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

    public Dictionary<string, double> EvaluateAll(List<DeductionAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
