namespace Acme.Workforce.Payrollops.Payslip.Slot;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the payslip slot calculation.</summary>
public interface IPayslipSlotStep
{
    string Describe();
    double Adjust(PayslipSlot item, double valueValue);
}

public sealed record FlaggedPayslipSlotAdjuster(Registry Flags, string Flag, double Percent) : IPayslipSlotStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayslipSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PayslipSlotRateOffsetAdjuster(double Weight) : IPayslipSlotStep
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(PayslipSlot item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record CapPayslipSlotAdjuster(double Cap) : IPayslipSlotStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayslipSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record DepartmentPayslipSlotAdjuster(Dictionary<string, double> Factors) : IPayslipSlotStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(PayslipSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPayslipSlotAdjuster(double Percent) : IPayslipSlotStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayslipSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the payslip slot steps in order and keeps a trace of each.</summary>
public sealed class PayslipSlotEngine(List<IPayslipSlotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayslipSlotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayslipSlotEngine DefaultPayslipSlotEngine(Registry flags)
    {
        return new PayslipSlotEngine(
            new List<IPayslipSlotStep>
            {
                new FlaggedPayslipSlotAdjuster(flags, "payslip-slot-rolling", 3.2),
                new PayslipSlotRateOffsetAdjuster(0.35),
                new CapPayslipSlotAdjuster(40991.0),
                new DepartmentPayslipSlotAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.33, ["west"] = 1.23 }),
                new PercentPayslipSlotAdjuster(-18.1),
            },
            new List<string>(),
            3.5,
            55545.0);
    }

    public double Compute(PayslipSlot item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PayslipSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
