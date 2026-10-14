namespace Acme.Workforce.Payrollops.Payslip.Slot;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the payslip request calculation.</summary>
public interface IPayslipRequestModifier
{
    string Describe();
    double Adjust(PayslipRequest item, double valueValue);
}

public sealed record PayslipRequestHoursOffsetAdjuster(double Weight) : IPayslipRequestModifier
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(PayslipRequest item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record CapPayslipRequestAdjuster(double Cap) : IPayslipRequestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayslipRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPayslipRequestAdjuster(Registry Flags, string Flag, double Percent) : IPayslipRequestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayslipRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CategoryPayslipRequestAdjuster(Dictionary<string, double> Factors) : IPayslipRequestModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(PayslipRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPayslipRequestAdjuster(List<double> Bounds, List<double> Rates) : IPayslipRequestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayslipRequest item, double valueValue)
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

public sealed record PercentPayslipRequestAdjuster(double Percent) : IPayslipRequestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayslipRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the payslip request steps in order and keeps a trace of each.</summary>
public sealed class PayslipRequestEngine(List<IPayslipRequestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayslipRequestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayslipRequestEngine DefaultPayslipRequestEngine(Registry flags)
    {
        return new PayslipRequestEngine(
            new List<IPayslipRequestModifier>
            {
                new PayslipRequestHoursOffsetAdjuster(1.08),
                new CapPayslipRequestAdjuster(15974.0),
                new FlaggedPayslipRequestAdjuster(flags, "payslip-request-fallback", -5.2),
                new CategoryPayslipRequestAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.92, ["east"] = 1.1, ["retail"] = 1.13 }),
                new TieredPayslipRequestAdjuster(
                    new List<double> { 4499.0, 9383.0, 9989.0 },
                    new List<double> { 0.92, 0.87, 0.99 }),
                new PercentPayslipRequestAdjuster(-22.5),
            },
            new List<string>(),
            0.2,
            69349.7);
    }

    public double Run(PayslipRequest item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<PayslipRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
