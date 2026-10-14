namespace Acme.Workforce.Payrollops.Payslip.Batch;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the fallback payslip batch calculation.</summary>
public interface IFallbackPayslipBatchStep
{
    string Describe();
    double Adjust(FallbackPayslipBatch item, double valueValue);
}

public sealed record CapFallbackPayslipBatchAdjuster(double Cap) : IFallbackPayslipBatchStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackPayslipBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentFallbackPayslipBatchAdjuster(double Percent) : IFallbackPayslipBatchStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(FallbackPayslipBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumFallbackPayslipBatchAdjuster(double Minimum) : IFallbackPayslipBatchStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackPayslipBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredFallbackPayslipBatchAdjuster(List<double> Bounds, List<double> Rates) : IFallbackPayslipBatchStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackPayslipBatch item, double valueValue)
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

public sealed record FlaggedFallbackPayslipBatchAdjuster(Registry Flags, string Flag, double Percent) : IFallbackPayslipBatchStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackPayslipBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the fallback payslip batch steps in order and keeps a trace of each.</summary>
public sealed class FallbackPayslipBatchEngine(List<IFallbackPayslipBatchStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackPayslipBatchStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackPayslipBatchEngine DefaultFallbackPayslipBatchEngine(Registry flags)
    {
        return new FallbackPayslipBatchEngine(
            new List<IFallbackPayslipBatchStep>
            {
                new CapFallbackPayslipBatchAdjuster(27074.0),
                new PercentFallbackPayslipBatchAdjuster(-23.7),
                new MinimumFallbackPayslipBatchAdjuster(49.5),
                new TieredFallbackPayslipBatchAdjuster(
                    new List<double> { 2399.0, 6405.0, 7921.0, 9583.0 },
                    new List<double> { 1.0, 0.87, 0.81, 1.01 }),
                new FlaggedFallbackPayslipBatchAdjuster(flags, "fallback-payslip-batch-regional", 4.7),
            },
            new List<string>(),
            2.0,
            27126.4);
    }

    public double Run(FallbackPayslipBatch item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackPayslipBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
