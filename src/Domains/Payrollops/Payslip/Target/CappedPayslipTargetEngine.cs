namespace Acme.Workforce.Payrollops.Payslip.Target;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the capped payslip target calculation.</summary>
public interface ICappedPayslipTargetAdjuster
{
    string Describe();
    double Adjust(CappedPayslipTarget item, double valueValue);
}

public sealed record CappedPayslipTargetWindowDaysOffsetAdjuster(double Weight) : ICappedPayslipTargetAdjuster
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(CappedPayslipTarget item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record FlaggedCappedPayslipTargetAdjuster(Registry Flags, string Flag, double Percent) : ICappedPayslipTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedPayslipTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentCappedPayslipTargetAdjuster(double Percent) : ICappedPayslipTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedPayslipTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelCappedPayslipTargetAdjuster(Dictionary<string, double> Factors) : ICappedPayslipTargetAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(CappedPayslipTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredCappedPayslipTargetAdjuster(List<double> Bounds, List<double> Rates) : ICappedPayslipTargetAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedPayslipTarget item, double valueValue)
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

/// <summary>Runs the capped payslip target steps in order and keeps a trace of each.</summary>
public sealed class CappedPayslipTargetEngine(List<ICappedPayslipTargetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedPayslipTargetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedPayslipTargetEngine DefaultCappedPayslipTargetEngine(Registry flags)
    {
        return new CappedPayslipTargetEngine(
            new List<ICappedPayslipTargetAdjuster>
            {
                new CappedPayslipTargetWindowDaysOffsetAdjuster(0.57),
                new FlaggedCappedPayslipTargetAdjuster(flags, "capped-payslip-target-fallback", -0.9),
                new PercentCappedPayslipTargetAdjuster(-24.7),
                new LabelCappedPayslipTargetAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.11, ["central"] = 0.93, ["south"] = 0.97 }),
                new TieredCappedPayslipTargetAdjuster(
                    new List<double> { 826.0, 1495.0, 1975.0, 6827.0 },
                    new List<double> { 0.98, 1.08, 0.85, 0.88 }),
            },
            new List<string>(),
            0.1,
            72534.6);
    }

    public double Run(CappedPayslipTarget item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CappedPayslipTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
