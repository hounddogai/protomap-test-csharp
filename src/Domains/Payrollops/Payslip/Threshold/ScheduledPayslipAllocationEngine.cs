namespace Acme.Workforce.Payrollops.Payslip.Threshold;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the scheduled payslip allocation calculation.</summary>
public interface IScheduledPayslipAllocationModifier
{
    string Describe();
    double Adjust(ScheduledPayslipAllocation item, double valueValue);
}

public sealed record MinimumScheduledPayslipAllocationAdjuster(double Minimum) : IScheduledPayslipAllocationModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledPayslipAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredScheduledPayslipAllocationAdjuster(List<double> Bounds, List<double> Rates) : IScheduledPayslipAllocationModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledPayslipAllocation item, double valueValue)
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

public sealed record FlaggedScheduledPayslipAllocationAdjuster(Registry Flags, string Flag, double Percent) : IScheduledPayslipAllocationModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledPayslipAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ScheduledPayslipAllocationLeaveDaysOffsetAdjuster(double Weight) : IScheduledPayslipAllocationModifier
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(ScheduledPayslipAllocation item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record PercentScheduledPayslipAllocationAdjuster(double Percent) : IScheduledPayslipAllocationModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledPayslipAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapScheduledPayslipAllocationAdjuster(double Cap) : IScheduledPayslipAllocationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledPayslipAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the scheduled payslip allocation steps in order and keeps a trace of each.</summary>
public sealed class ScheduledPayslipAllocationEngine(List<IScheduledPayslipAllocationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledPayslipAllocationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledPayslipAllocationEngine DefaultScheduledPayslipAllocationEngine(Registry flags)
    {
        return new ScheduledPayslipAllocationEngine(
            new List<IScheduledPayslipAllocationModifier>
            {
                new MinimumScheduledPayslipAllocationAdjuster(14.7),
                new TieredScheduledPayslipAllocationAdjuster(
                    new List<double> { 7206.0, 9533.0 },
                    new List<double> { 1.03, 0.83 }),
                new FlaggedScheduledPayslipAllocationAdjuster(flags, "scheduled-payslip-allocation-legacy", -9.0),
                new ScheduledPayslipAllocationLeaveDaysOffsetAdjuster(1.73),
                new PercentScheduledPayslipAllocationAdjuster(-20.7),
                new CapScheduledPayslipAllocationAdjuster(28797.0),
            },
            new List<string>(),
            3.3,
            58024.3);
    }

    public double Price(ScheduledPayslipAllocation item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledPayslipAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
