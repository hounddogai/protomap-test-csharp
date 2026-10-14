namespace Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>One step of the retro pay quota calculation.</summary>
public interface IRetroPayQuotaStep
{
    string Describe();
    double Adjust(RetroPayQuota item, double valueValue);
}

public sealed record PercentRetroPayQuotaAdjuster(double Percent) : IRetroPayQuotaStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RetroPayQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRetroPayQuotaAdjuster(double Minimum) : IRetroPayQuotaStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RetroPayQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameRetroPayQuotaAdjuster(Dictionary<string, double> Factors) : IRetroPayQuotaStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RetroPayQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the retro pay quota steps in order and keeps a trace of each.</summary>
public sealed class RetroPayQuotaEngine(List<IRetroPayQuotaStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRetroPayQuotaStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RetroPayQuotaEngine DefaultRetroPayQuotaEngine()
    {
        return new RetroPayQuotaEngine(
            new List<IRetroPayQuotaStep>
            {
                new PercentRetroPayQuotaAdjuster(-7.4),
                new MinimumRetroPayQuotaAdjuster(19.3),
                new NameRetroPayQuotaAdjuster(new Dictionary<string, double> { ["north"] = 1.13 }),
            },
            new List<string>(),
            3.3,
            92060.1);
    }

    public double Run(RetroPayQuota item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RetroPayQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
