namespace Acme.Workforce.Payrollops.Payslip.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped payslip batch calculation.</summary>
public interface ICappedPayslipBatchFactor
{
    string Describe();
    double Adjust(CappedPayslipBatch item, double valueValue);
}

public sealed record CapCappedPayslipBatchAdjuster(double Cap) : ICappedPayslipBatchFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedPayslipBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredCappedPayslipBatchAdjuster(List<double> Bounds, List<double> Rates) : ICappedPayslipBatchFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedPayslipBatch item, double valueValue)
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

public sealed record FlaggedCappedPayslipBatchAdjuster(Registry Flags, string Flag, double Percent) : ICappedPayslipBatchFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedPayslipBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CappedPayslipBatchDiscountOffsetAdjuster(double Weight) : ICappedPayslipBatchFactor
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(CappedPayslipBatch item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record RoundingCappedPayslipBatchAdjuster(double Step) : ICappedPayslipBatchFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedPayslipBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumCappedPayslipBatchAdjuster(double Minimum) : ICappedPayslipBatchFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedPayslipBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the capped payslip batch steps in order and keeps a trace of each.</summary>
public sealed class CappedPayslipBatchEngine(List<ICappedPayslipBatchFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedPayslipBatchFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedPayslipBatchEngine DefaultCappedPayslipBatchEngine(Registry flags)
    {
        return new CappedPayslipBatchEngine(
            new List<ICappedPayslipBatchFactor>
            {
                new CapCappedPayslipBatchAdjuster(26217.0),
                new TieredCappedPayslipBatchAdjuster(
                    new List<double> { 1638.0, 1965.0, 5483.0 },
                    new List<double> { 0.87, 1.11, 1.08 }),
                new FlaggedCappedPayslipBatchAdjuster(flags, "capped-payslip-batch-seasonal", 1.1),
                new CappedPayslipBatchDiscountOffsetAdjuster(1.92),
                new RoundingCappedPayslipBatchAdjuster(0.1),
                new MinimumCappedPayslipBatchAdjuster(8.9),
            },
            new List<string>(),
            1.3,
            9134.5);
    }

    public double Run(CappedPayslipBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<CappedPayslipBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
