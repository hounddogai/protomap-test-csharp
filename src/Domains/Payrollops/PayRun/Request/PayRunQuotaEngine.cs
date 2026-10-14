namespace Acme.Workforce.Payrollops.PayRun.Request;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pay run quota calculation.</summary>
public interface IPayRunQuotaStep
{
    string Describe();
    double Adjust(PayRunQuota item, double valueValue);
}

public sealed record FlaggedPayRunQuotaAdjuster(Registry Flags, string Flag, double Percent) : IPayRunQuotaStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayRunQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPayRunQuotaAdjuster(double Percent) : IPayRunQuotaStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayRunQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PayRunQuotaRatingOffsetAdjuster(double Weight) : IPayRunQuotaStep
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(PayRunQuota item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

/// <summary>Runs the pay run quota steps in order and keeps a trace of each.</summary>
public sealed class PayRunQuotaEngine(List<IPayRunQuotaStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayRunQuotaStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayRunQuotaEngine DefaultPayRunQuotaEngine(Registry flags)
    {
        return new PayRunQuotaEngine(
            new List<IPayRunQuotaStep>
            {
                new FlaggedPayRunQuotaAdjuster(flags, "pay-run-quota-tiered", -4.1),
                new PercentPayRunQuotaAdjuster(24.2),
                new PayRunQuotaRatingOffsetAdjuster(1.17),
            },
            new List<string>(),
            3.5,
            42707.0);
    }

    public double Price(PayRunQuota item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PayRunQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
