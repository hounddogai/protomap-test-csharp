namespace Acme.Workforce.Talent.Mobility.MobilityRecord;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the capped mobility notice calculation.</summary>
public interface ICappedMobilityNoticeModifier
{
    string Describe();
    double Adjust(CappedMobilityNotice item, double valueValue);
}

public sealed record FlaggedCappedMobilityNoticeAdjuster(Registry Flags, string Flag, double Percent) : ICappedMobilityNoticeModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedMobilityNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapCappedMobilityNoticeAdjuster(double Cap) : ICappedMobilityNoticeModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedMobilityNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredCappedMobilityNoticeAdjuster(List<double> Bounds, List<double> Rates) : ICappedMobilityNoticeModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedMobilityNotice item, double valueValue)
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

public sealed record PercentCappedMobilityNoticeAdjuster(double Percent) : ICappedMobilityNoticeModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedMobilityNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped mobility notice steps in order and keeps a trace of each.</summary>
public sealed class CappedMobilityNoticeEngine(List<ICappedMobilityNoticeModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedMobilityNoticeModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedMobilityNoticeEngine DefaultCappedMobilityNoticeEngine(Registry flags)
    {
        return new CappedMobilityNoticeEngine(
            new List<ICappedMobilityNoticeModifier>
            {
                new FlaggedCappedMobilityNoticeAdjuster(flags, "capped-mobility-notice-strict", -8.0),
                new CapCappedMobilityNoticeAdjuster(29995.0),
                new TieredCappedMobilityNoticeAdjuster(
                    new List<double> { 828.0, 3125.0, 5950.0 },
                    new List<double> { 0.81, 0.84, 0.83 }),
                new PercentCappedMobilityNoticeAdjuster(-16.1),
            },
            new List<string>(),
            4.3,
            30036.8);
    }

    public double Compute(CappedMobilityNotice item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<CappedMobilityNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
