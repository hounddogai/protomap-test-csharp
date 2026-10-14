namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the payslip estimate calculation.</summary>
public interface IPayslipEstimateAdjuster
{
    string Describe();
    double Adjust(PayslipEstimate item, double valueValue);
}

public sealed record TieredPayslipEstimateAdjuster(List<double> Bounds, List<double> Rates) : IPayslipEstimateAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayslipEstimate item, double valueValue)
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

public sealed record FlaggedPayslipEstimateAdjuster(Registry Flags, string Flag, double Percent) : IPayslipEstimateAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayslipEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPayslipEstimateAdjuster(double Cap) : IPayslipEstimateAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayslipEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ChannelPayslipEstimateAdjuster(Dictionary<string, double> Factors) : IPayslipEstimateAdjuster
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(PayslipEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingPayslipEstimateAdjuster(double Step) : IPayslipEstimateAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PayslipEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the payslip estimate steps in order and keeps a trace of each.</summary>
public sealed class PayslipEstimateEngine(List<IPayslipEstimateAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayslipEstimateAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayslipEstimateEngine DefaultPayslipEstimateEngine(Registry flags)
    {
        return new PayslipEstimateEngine(
            new List<IPayslipEstimateAdjuster>
            {
                new TieredPayslipEstimateAdjuster(
                    new List<double> { 183.0, 1115.0, 4468.0, 6395.0 },
                    new List<double> { 1.0, 0.83, 0.89, 0.89 }),
                new FlaggedPayslipEstimateAdjuster(flags, "payslip-estimate-pending", -6.1),
                new CapPayslipEstimateAdjuster(29533.0),
                new ChannelPayslipEstimateAdjuster(new Dictionary<string, double> { ["online"] = 1.18 }),
                new RoundingPayslipEstimateAdjuster(5.0),
            },
            new List<string>(),
            4.6,
            60001.1);
    }

    public double Compute(PayslipEstimate item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PayslipEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
