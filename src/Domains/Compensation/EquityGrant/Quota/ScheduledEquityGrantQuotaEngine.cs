namespace Acme.Workforce.Compensation.EquityGrant.Quota;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the scheduled equity grant quota calculation.</summary>
public interface IScheduledEquityGrantQuotaModifier
{
    string Describe();
    double Adjust(ScheduledEquityGrantQuota item, double valueValue);
}

public sealed record MinimumScheduledEquityGrantQuotaAdjuster(double Minimum) : IScheduledEquityGrantQuotaModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledEquityGrantQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredScheduledEquityGrantQuotaAdjuster(List<double> Bounds, List<double> Rates) : IScheduledEquityGrantQuotaModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledEquityGrantQuota item, double valueValue)
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

public sealed record ScheduledEquityGrantQuotaVersionOffsetAdjuster(double Weight) : IScheduledEquityGrantQuotaModifier
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(ScheduledEquityGrantQuota item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record PercentScheduledEquityGrantQuotaAdjuster(double Percent) : IScheduledEquityGrantQuotaModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledEquityGrantQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapScheduledEquityGrantQuotaAdjuster(double Cap) : IScheduledEquityGrantQuotaModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledEquityGrantQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedScheduledEquityGrantQuotaAdjuster(Registry Flags, string Flag, double Percent) : IScheduledEquityGrantQuotaModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledEquityGrantQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled equity grant quota steps in order and keeps a trace of each.</summary>
public sealed class ScheduledEquityGrantQuotaEngine(List<IScheduledEquityGrantQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledEquityGrantQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledEquityGrantQuotaEngine DefaultScheduledEquityGrantQuotaEngine(Registry flags)
    {
        return new ScheduledEquityGrantQuotaEngine(
            new List<IScheduledEquityGrantQuotaModifier>
            {
                new MinimumScheduledEquityGrantQuotaAdjuster(34.0),
                new TieredScheduledEquityGrantQuotaAdjuster(
                    new List<double> { 2827.0, 5784.0, 8731.0, 9835.0 },
                    new List<double> { 1.08, 1.17, 1.03, 0.89 }),
                new ScheduledEquityGrantQuotaVersionOffsetAdjuster(1.72),
                new PercentScheduledEquityGrantQuotaAdjuster(-0.6),
                new CapScheduledEquityGrantQuotaAdjuster(27480.0),
                new FlaggedScheduledEquityGrantQuotaAdjuster(flags, "scheduled-equity-grant-quota-legacy", -8.5),
            },
            new List<string>(),
            4.3,
            48906.4);
    }

    public double Run(ScheduledEquityGrantQuota item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledEquityGrantQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
