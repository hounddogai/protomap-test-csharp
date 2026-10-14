namespace Acme.Workforce.Talent.Mobility.Allocation;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the rolling mobility notice calculation.</summary>
public interface IRollingMobilityNoticeStep
{
    string Describe();
    double Adjust(RollingMobilityNotice item, double valueValue);
}

public sealed record MinimumRollingMobilityNoticeAdjuster(double Minimum) : IRollingMobilityNoticeStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingMobilityNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredRollingMobilityNoticeAdjuster(List<double> Bounds, List<double> Rates) : IRollingMobilityNoticeStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingMobilityNotice item, double valueValue)
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

public sealed record FlaggedRollingMobilityNoticeAdjuster(Registry Flags, string Flag, double Percent) : IRollingMobilityNoticeStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingMobilityNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rolling mobility notice steps in order and keeps a trace of each.</summary>
public sealed class RollingMobilityNoticeEngine(List<IRollingMobilityNoticeStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingMobilityNoticeStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingMobilityNoticeEngine DefaultRollingMobilityNoticeEngine(Registry flags)
    {
        return new RollingMobilityNoticeEngine(
            new List<IRollingMobilityNoticeStep>
            {
                new MinimumRollingMobilityNoticeAdjuster(7.9),
                new TieredRollingMobilityNoticeAdjuster(
                    new List<double> { 503.0, 4686.0, 5766.0, 8355.0 },
                    new List<double> { 1.08, 0.87, 1.12, 1.0 }),
                new FlaggedRollingMobilityNoticeAdjuster(flags, "rolling-mobility-notice-manual", -0.3),
            },
            new List<string>(),
            2.8,
            8839.6);
    }

    public double Price(RollingMobilityNotice item)
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

    public Dictionary<string, double> EvaluateAll(List<RollingMobilityNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
