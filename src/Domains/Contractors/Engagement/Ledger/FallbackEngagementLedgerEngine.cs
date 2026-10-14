namespace Acme.Workforce.Contractors.Engagement.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback engagement ledger calculation.</summary>
public interface IFallbackEngagementLedgerFactor
{
    string Describe();
    double Adjust(FallbackEngagementLedger item, double valueValue);
}

public sealed record ChannelFallbackEngagementLedgerAdjuster(Dictionary<string, double> Factors) : IFallbackEngagementLedgerFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(FallbackEngagementLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingFallbackEngagementLedgerAdjuster(double Step) : IFallbackEngagementLedgerFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackEngagementLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FallbackEngagementLedgerWindowDaysOffsetAdjuster(double Weight) : IFallbackEngagementLedgerFactor
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(FallbackEngagementLedger item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record MinimumFallbackEngagementLedgerAdjuster(double Minimum) : IFallbackEngagementLedgerFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(FallbackEngagementLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapFallbackEngagementLedgerAdjuster(double Cap) : IFallbackEngagementLedgerFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackEngagementLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedFallbackEngagementLedgerAdjuster(Registry Flags, string Flag, double Percent) : IFallbackEngagementLedgerFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(FallbackEngagementLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the fallback engagement ledger steps in order and keeps a trace of each.</summary>
public sealed class FallbackEngagementLedgerEngine(List<IFallbackEngagementLedgerFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackEngagementLedgerFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackEngagementLedgerEngine DefaultFallbackEngagementLedgerEngine(Registry flags)
    {
        return new FallbackEngagementLedgerEngine(
            new List<IFallbackEngagementLedgerFactor>
            {
                new ChannelFallbackEngagementLedgerAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.15, ["north"] = 1.24 }),
                new RoundingFallbackEngagementLedgerAdjuster(0.25),
                new FallbackEngagementLedgerWindowDaysOffsetAdjuster(1.22),
                new MinimumFallbackEngagementLedgerAdjuster(0.1),
                new CapFallbackEngagementLedgerAdjuster(19061.0),
                new FlaggedFallbackEngagementLedgerAdjuster(flags, "fallback-engagement-ledger-rolling", 0.3),
            },
            new List<string>(),
            1.8,
            97172.0);
    }

    public double Compute(FallbackEngagementLedger item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackEngagementLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
