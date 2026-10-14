namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback payslip digest calculation.</summary>
public interface IFallbackPayslipDigestAdjuster
{
    string Describe();
    double Adjust(FallbackPayslipDigest item, double valueValue);
}

public sealed record MinimumFallbackPayslipDigestAdjuster(double Minimum) : IFallbackPayslipDigestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackPayslipDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedFallbackPayslipDigestAdjuster(Registry Flags, string Flag, double Percent) : IFallbackPayslipDigestAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackPayslipDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameFallbackPayslipDigestAdjuster(Dictionary<string, double> Factors) : IFallbackPayslipDigestAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(FallbackPayslipDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the fallback payslip digest steps in order and keeps a trace of each.</summary>
public sealed class FallbackPayslipDigestEngine(List<IFallbackPayslipDigestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackPayslipDigestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackPayslipDigestEngine DefaultFallbackPayslipDigestEngine(Registry flags)
    {
        return new FallbackPayslipDigestEngine(
            new List<IFallbackPayslipDigestAdjuster>
            {
                new MinimumFallbackPayslipDigestAdjuster(32.9),
                new FlaggedFallbackPayslipDigestAdjuster(flags, "fallback-payslip-digest-scheduled", 9.4),
                new NameFallbackPayslipDigestAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.39, ["west"] = 0.98, ["central"] = 0.8 }),
            },
            new List<string>(),
            3.5,
            83509.3);
    }

    public double Run(FallbackPayslipDigest item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackPayslipDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
