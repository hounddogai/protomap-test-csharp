namespace Acme.Workforce.Payrollops.Payslip.PayslipNotice;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling payslip digest calculation.</summary>
public interface IRollingPayslipDigestModifier
{
    string Describe();
    double Adjust(RollingPayslipDigest item, double valueValue);
}

public sealed record RoundingRollingPayslipDigestAdjuster(double Step) : IRollingPayslipDigestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingPayslipDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record RollingPayslipDigestMaximumOffsetAdjuster(double Weight) : IRollingPayslipDigestModifier
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(RollingPayslipDigest item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record TieredRollingPayslipDigestAdjuster(List<double> Bounds, List<double> Rates) : IRollingPayslipDigestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingPayslipDigest item, double valueValue)
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

public sealed record ChannelRollingPayslipDigestAdjuster(Dictionary<string, double> Factors) : IRollingPayslipDigestModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(RollingPayslipDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapRollingPayslipDigestAdjuster(double Cap) : IRollingPayslipDigestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingPayslipDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the rolling payslip digest steps in order and keeps a trace of each.</summary>
public sealed class RollingPayslipDigestEngine(List<IRollingPayslipDigestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingPayslipDigestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingPayslipDigestEngine DefaultRollingPayslipDigestEngine()
    {
        return new RollingPayslipDigestEngine(
            new List<IRollingPayslipDigestModifier>
            {
                new RoundingRollingPayslipDigestAdjuster(1.0),
                new RollingPayslipDigestMaximumOffsetAdjuster(1.94),
                new TieredRollingPayslipDigestAdjuster(
                    new List<double> { 6001.0, 6163.0 },
                    new List<double> { 0.99, 0.91 }),
                new ChannelRollingPayslipDigestAdjuster(new Dictionary<string, double> { ["central"] = 1.4 }),
                new CapRollingPayslipDigestAdjuster(44540.0),
            },
            new List<string>(),
            1.5,
            33644.3);
    }

    public double Evaluate(RollingPayslipDigest item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RollingPayslipDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
