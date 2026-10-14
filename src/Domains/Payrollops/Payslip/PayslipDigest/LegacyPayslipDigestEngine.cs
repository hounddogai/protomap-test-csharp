namespace Acme.Workforce.Payrollops.Payslip.PayslipDigest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy payslip digest calculation.</summary>
public interface ILegacyPayslipDigestFactor
{
    string Describe();
    double Adjust(LegacyPayslipDigest item, double valueValue);
}

public sealed record FlaggedLegacyPayslipDigestAdjuster(Registry Flags, string Flag, double Percent) : ILegacyPayslipDigestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyPayslipDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLegacyPayslipDigestAdjuster(double Cap) : ILegacyPayslipDigestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyPayslipDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingLegacyPayslipDigestAdjuster(double Step) : ILegacyPayslipDigestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyPayslipDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitleLegacyPayslipDigestAdjuster(Dictionary<string, double> Factors) : ILegacyPayslipDigestFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LegacyPayslipDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentLegacyPayslipDigestAdjuster(double Percent) : ILegacyPayslipDigestFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyPayslipDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLegacyPayslipDigestAdjuster(double Minimum) : ILegacyPayslipDigestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyPayslipDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the legacy payslip digest steps in order and keeps a trace of each.</summary>
public sealed class LegacyPayslipDigestEngine(List<ILegacyPayslipDigestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyPayslipDigestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyPayslipDigestEngine DefaultLegacyPayslipDigestEngine(Registry flags)
    {
        return new LegacyPayslipDigestEngine(
            new List<ILegacyPayslipDigestFactor>
            {
                new FlaggedLegacyPayslipDigestAdjuster(flags, "legacy-payslip-digest-scheduled", 9.8),
                new CapLegacyPayslipDigestAdjuster(28534.0),
                new RoundingLegacyPayslipDigestAdjuster(1.0),
                new TitleLegacyPayslipDigestAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.29, ["east"] = 0.71, ["central"] = 0.85 }),
                new PercentLegacyPayslipDigestAdjuster(-17.3),
                new MinimumLegacyPayslipDigestAdjuster(24.7),
            },
            new List<string>(),
            4.9,
            4110.6);
    }

    public double Price(LegacyPayslipDigest item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyPayslipDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
