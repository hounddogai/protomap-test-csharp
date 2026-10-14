namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the strict pay band quota calculation.</summary>
public interface IStrictPayBandQuotaStep
{
    string Describe();
    double Adjust(StrictPayBandQuota item, double valueValue);
}

public sealed record FlaggedStrictPayBandQuotaAdjuster(Registry Flags, string Flag, double Percent) : IStrictPayBandQuotaStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictPayBandQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentStrictPayBandQuotaAdjuster(double Percent) : IStrictPayBandQuotaStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(StrictPayBandQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapStrictPayBandQuotaAdjuster(double Cap) : IStrictPayBandQuotaStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictPayBandQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the strict pay band quota steps in order and keeps a trace of each.</summary>
public sealed class StrictPayBandQuotaEngine(List<IStrictPayBandQuotaStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictPayBandQuotaStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictPayBandQuotaEngine DefaultStrictPayBandQuotaEngine(Registry flags)
    {
        return new StrictPayBandQuotaEngine(
            new List<IStrictPayBandQuotaStep>
            {
                new FlaggedStrictPayBandQuotaAdjuster(flags, "strict-pay-band-quota-batched", 3.6),
                new PercentStrictPayBandQuotaAdjuster(-15.2),
                new CapStrictPayBandQuotaAdjuster(5348.0),
            },
            new List<string>(),
            2.1,
            93169.7);
    }

    public double Evaluate(StrictPayBandQuota item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<StrictPayBandQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
