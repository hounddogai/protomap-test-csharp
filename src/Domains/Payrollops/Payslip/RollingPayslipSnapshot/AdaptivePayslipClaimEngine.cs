namespace Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the adaptive payslip claim calculation.</summary>
public interface IAdaptivePayslipClaimStep
{
    string Describe();
    double Adjust(AdaptivePayslipClaim item, double valueValue);
}

public sealed record PercentAdaptivePayslipClaimAdjuster(double Percent) : IAdaptivePayslipClaimStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptivePayslipClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAdaptivePayslipClaimAdjuster(double Minimum) : IAdaptivePayslipClaimStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptivePayslipClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record AdaptivePayslipClaimRetriesOffsetAdjuster(double Weight) : IAdaptivePayslipClaimStep
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(AdaptivePayslipClaim item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record TeamAdaptivePayslipClaimAdjuster(Dictionary<string, double> Factors) : IAdaptivePayslipClaimStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(AdaptivePayslipClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedAdaptivePayslipClaimAdjuster(Registry Flags, string Flag, double Percent) : IAdaptivePayslipClaimStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptivePayslipClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adaptive payslip claim steps in order and keeps a trace of each.</summary>
public sealed class AdaptivePayslipClaimEngine(List<IAdaptivePayslipClaimStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptivePayslipClaimStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptivePayslipClaimEngine DefaultAdaptivePayslipClaimEngine(Registry flags)
    {
        return new AdaptivePayslipClaimEngine(
            new List<IAdaptivePayslipClaimStep>
            {
                new PercentAdaptivePayslipClaimAdjuster(-23.4),
                new MinimumAdaptivePayslipClaimAdjuster(5.6),
                new AdaptivePayslipClaimRetriesOffsetAdjuster(0.78),
                new TeamAdaptivePayslipClaimAdjuster(new Dictionary<string, double> { ["south"] = 1.2 }),
                new FlaggedAdaptivePayslipClaimAdjuster(flags, "adaptive-payslip-claim-manual", -9.8),
            },
            new List<string>(),
            2.3,
            83014.9);
    }

    public double Compute(AdaptivePayslipClaim item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptivePayslipClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
