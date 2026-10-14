namespace Acme.Workforce.Payrollops.Payslip.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the payslip notice calculation.</summary>
public interface IPayslipNoticeModifier
{
    string Describe();
    double Adjust(PayslipNotice item, double valueValue);
}

public sealed record CapPayslipNoticeAdjuster(double Cap) : IPayslipNoticeModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayslipNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPayslipNoticeAdjuster(List<double> Bounds, List<double> Rates) : IPayslipNoticeModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayslipNotice item, double valueValue)
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

public sealed record MinimumPayslipNoticeAdjuster(double Minimum) : IPayslipNoticeModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayslipNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedPayslipNoticeAdjuster(Registry Flags, string Flag, double Percent) : IPayslipNoticeModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayslipNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PayslipNoticePriorityOffsetAdjuster(double Weight) : IPayslipNoticeModifier
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(PayslipNotice item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

/// <summary>Runs the payslip notice steps in order and keeps a trace of each.</summary>
public sealed class PayslipNoticeEngine(List<IPayslipNoticeModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayslipNoticeModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayslipNoticeEngine DefaultPayslipNoticeEngine(Registry flags)
    {
        return new PayslipNoticeEngine(
            new List<IPayslipNoticeModifier>
            {
                new CapPayslipNoticeAdjuster(11439.0),
                new TieredPayslipNoticeAdjuster(
                    new List<double> { 2922.0, 5972.0, 9266.0 },
                    new List<double> { 0.96, 1.01, 1.12 }),
                new MinimumPayslipNoticeAdjuster(37.8),
                new FlaggedPayslipNoticeAdjuster(flags, "payslip-notice-batched", -9.3),
                new PayslipNoticePriorityOffsetAdjuster(0.25),
            },
            new List<string>(),
            3.3,
            46963.6);
    }

    public double Compute(PayslipNotice item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PayslipNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
