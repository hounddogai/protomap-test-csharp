namespace Acme.Workforce.Payrollops.Payslip.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the payslip digest calculation.</summary>
public interface IPayslipDigestModifier
{
    string Describe();
    double Adjust(PayslipDigest item, double valueValue);
}

public sealed record RoundingPayslipDigestAdjuster(double Step) : IPayslipDigestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayslipDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredPayslipDigestAdjuster(List<double> Bounds, List<double> Rates) : IPayslipDigestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayslipDigest item, double valueValue)
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

public sealed record CapPayslipDigestAdjuster(double Cap) : IPayslipDigestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayslipDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedPayslipDigestAdjuster(Registry Flags, string Flag, double Percent) : IPayslipDigestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayslipDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPayslipDigestAdjuster(double Minimum) : IPayslipDigestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayslipDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the payslip digest steps in order and keeps a trace of each.</summary>
public sealed class PayslipDigestEngine(List<IPayslipDigestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayslipDigestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayslipDigestEngine DefaultPayslipDigestEngine(Registry flags)
    {
        return new PayslipDigestEngine(
            new List<IPayslipDigestModifier>
            {
                new RoundingPayslipDigestAdjuster(0.05),
                new TieredPayslipDigestAdjuster(
                    new List<double> { 1287.0, 4806.0, 9878.0 },
                    new List<double> { 1.02, 0.98, 0.97 }),
                new CapPayslipDigestAdjuster(28095.0),
                new FlaggedPayslipDigestAdjuster(flags, "payslip-digest-strict", 6.6),
                new MinimumPayslipDigestAdjuster(7.6),
            },
            new List<string>(),
            0.9,
            93515.5);
    }

    public double Run(PayslipDigest item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PayslipDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
