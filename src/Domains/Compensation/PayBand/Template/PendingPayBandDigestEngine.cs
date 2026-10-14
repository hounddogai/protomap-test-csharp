namespace Acme.Workforce.Compensation.PayBand.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending pay band digest calculation.</summary>
public interface IPendingPayBandDigestStep
{
    string Describe();
    double Adjust(PendingPayBandDigest item, double valueValue);
}

public sealed record ChannelPendingPayBandDigestAdjuster(Dictionary<string, double> Factors) : IPendingPayBandDigestStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(PendingPayBandDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPendingPayBandDigestAdjuster(List<double> Bounds, List<double> Rates) : IPendingPayBandDigestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingPayBandDigest item, double valueValue)
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

public sealed record PendingPayBandDigestVersionOffsetAdjuster(double Weight) : IPendingPayBandDigestStep
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(PendingPayBandDigest item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record FlaggedPendingPayBandDigestAdjuster(Registry Flags, string Flag, double Percent) : IPendingPayBandDigestStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingPayBandDigest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPendingPayBandDigestAdjuster(double Minimum) : IPendingPayBandDigestStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingPayBandDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPendingPayBandDigestAdjuster(double Percent) : IPendingPayBandDigestStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingPayBandDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending pay band digest steps in order and keeps a trace of each.</summary>
public sealed class PendingPayBandDigestEngine(List<IPendingPayBandDigestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingPayBandDigestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingPayBandDigestEngine DefaultPendingPayBandDigestEngine(Registry flags)
    {
        return new PendingPayBandDigestEngine(
            new List<IPendingPayBandDigestStep>
            {
                new ChannelPendingPayBandDigestAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.86, ["east"] = 1.05 }),
                new TieredPendingPayBandDigestAdjuster(
                    new List<double> { 5920.0, 7540.0 },
                    new List<double> { 1.07, 1.05 }),
                new PendingPayBandDigestVersionOffsetAdjuster(1.78),
                new FlaggedPendingPayBandDigestAdjuster(flags, "pending-pay-band-digest-pending", -6.4),
                new MinimumPendingPayBandDigestAdjuster(20.7),
                new PercentPendingPayBandDigestAdjuster(-19.2),
            },
            new List<string>(),
            2.2,
            59638.3);
    }

    public double Compute(PendingPayBandDigest item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<PendingPayBandDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
