namespace Acme.Workforce.Compliance.Certification.Forecast;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the scheduled certification adjustment calculation.</summary>
public interface IScheduledCertificationAdjustmentModifier
{
    string Describe();
    double Adjust(ScheduledCertificationAdjustment item, double valueValue);
}

public sealed record ChannelScheduledCertificationAdjustmentAdjuster(Dictionary<string, double> Factors) : IScheduledCertificationAdjustmentModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(ScheduledCertificationAdjustment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record ScheduledCertificationAdjustmentExpiresAtOffsetAdjuster(double Weight) : IScheduledCertificationAdjustmentModifier
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(ScheduledCertificationAdjustment item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record FlaggedScheduledCertificationAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : IScheduledCertificationAdjustmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledCertificationAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled certification adjustment steps in order and keeps a trace of each.</summary>
public sealed class ScheduledCertificationAdjustmentEngine(List<IScheduledCertificationAdjustmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledCertificationAdjustmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledCertificationAdjustmentEngine DefaultScheduledCertificationAdjustmentEngine(Registry flags)
    {
        return new ScheduledCertificationAdjustmentEngine(
            new List<IScheduledCertificationAdjustmentModifier>
            {
                new ChannelScheduledCertificationAdjustmentAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.83, ["east"] = 1.1, ["online"] = 1.0 }),
                new ScheduledCertificationAdjustmentExpiresAtOffsetAdjuster(0.04),
                new FlaggedScheduledCertificationAdjustmentAdjuster(
                    flags,
                    "scheduled-certification-adjustment-fallback",
                    -0.9),
            },
            new List<string>(),
            3.3,
            25234.1);
    }

    public double Price(ScheduledCertificationAdjustment item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledCertificationAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
