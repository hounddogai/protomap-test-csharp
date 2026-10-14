namespace Acme.Workforce.Payrollops.Payslip.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal payslip baseline calculation.</summary>
public interface ISeasonalPayslipBaselineStep
{
    string Describe();
    double Adjust(SeasonalPayslipBaseline item, double valueValue);
}

public sealed record MinimumSeasonalPayslipBaselineAdjuster(double Minimum) : ISeasonalPayslipBaselineStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalPayslipBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedSeasonalPayslipBaselineAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalPayslipBaselineStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalPayslipBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapSeasonalPayslipBaselineAdjuster(double Cap) : ISeasonalPayslipBaselineStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalPayslipBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingSeasonalPayslipBaselineAdjuster(double Step) : ISeasonalPayslipBaselineStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalPayslipBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record DepartmentSeasonalPayslipBaselineAdjuster(Dictionary<string, double> Factors) : ISeasonalPayslipBaselineStep
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(SeasonalPayslipBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the seasonal payslip baseline steps in order and keeps a trace of each.</summary>
public sealed class SeasonalPayslipBaselineEngine(List<ISeasonalPayslipBaselineStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalPayslipBaselineStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalPayslipBaselineEngine DefaultSeasonalPayslipBaselineEngine(Registry flags)
    {
        return new SeasonalPayslipBaselineEngine(
            new List<ISeasonalPayslipBaselineStep>
            {
                new MinimumSeasonalPayslipBaselineAdjuster(5.5),
                new FlaggedSeasonalPayslipBaselineAdjuster(flags, "seasonal-payslip-baseline-batched", 4.0),
                new CapSeasonalPayslipBaselineAdjuster(28286.0),
                new RoundingSeasonalPayslipBaselineAdjuster(0.05),
                new DepartmentSeasonalPayslipBaselineAdjuster(new Dictionary<string, double> { ["retail"] = 0.71 }),
            },
            new List<string>(),
            2.2,
            67050.0);
    }

    public double Run(SeasonalPayslipBaseline item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalPayslipBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
