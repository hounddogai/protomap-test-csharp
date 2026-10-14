namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global payslip notice calculation.</summary>
public interface IGlobalPayslipNoticeFactor
{
    string Describe();
    double Adjust(GlobalPayslipNotice item, double valueValue);
}

public sealed record CountryGlobalPayslipNoticeAdjuster(Dictionary<string, double> Factors) : IGlobalPayslipNoticeFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(GlobalPayslipNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentGlobalPayslipNoticeAdjuster(double Percent) : IGlobalPayslipNoticeFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalPayslipNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredGlobalPayslipNoticeAdjuster(List<double> Bounds, List<double> Rates) : IGlobalPayslipNoticeFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalPayslipNotice item, double valueValue)
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

public sealed record FlaggedGlobalPayslipNoticeAdjuster(Registry Flags, string Flag, double Percent) : IGlobalPayslipNoticeFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalPayslipNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the global payslip notice steps in order and keeps a trace of each.</summary>
public sealed class GlobalPayslipNoticeEngine(List<IGlobalPayslipNoticeFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalPayslipNoticeFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalPayslipNoticeEngine DefaultGlobalPayslipNoticeEngine(Registry flags)
    {
        return new GlobalPayslipNoticeEngine(
            new List<IGlobalPayslipNoticeFactor>
            {
                new CountryGlobalPayslipNoticeAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.29, ["north"] = 1.08 }),
                new PercentGlobalPayslipNoticeAdjuster(-17.6),
                new TieredGlobalPayslipNoticeAdjuster(
                    new List<double> { 53.0, 1268.0, 1646.0, 3203.0 },
                    new List<double> { 0.94, 1.1, 0.99, 1.04 }),
                new FlaggedGlobalPayslipNoticeAdjuster(flags, "global-payslip-notice-weighted", -1.1),
            },
            new List<string>(),
            4.5,
            56024.1);
    }

    public double Run(GlobalPayslipNotice item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalPayslipNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
