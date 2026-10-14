namespace Acme.Workforce.Assets.Refresh.Summary;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pending refresh claim calculation.</summary>
public interface IPendingRefreshClaimAdjuster
{
    string Describe();
    double Adjust(PendingRefreshClaim item, double valueValue);
}

public sealed record FlaggedPendingRefreshClaimAdjuster(Registry Flags, string Flag, double Percent) : IPendingRefreshClaimAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingRefreshClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPendingRefreshClaimAdjuster(List<double> Bounds, List<double> Rates) : IPendingRefreshClaimAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingRefreshClaim item, double valueValue)
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

public sealed record CapPendingRefreshClaimAdjuster(double Cap) : IPendingRefreshClaimAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingRefreshClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPendingRefreshClaimAdjuster(double Percent) : IPendingRefreshClaimAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingRefreshClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending refresh claim steps in order and keeps a trace of each.</summary>
public sealed class PendingRefreshClaimEngine(List<IPendingRefreshClaimAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingRefreshClaimAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingRefreshClaimEngine DefaultPendingRefreshClaimEngine(Registry flags)
    {
        return new PendingRefreshClaimEngine(
            new List<IPendingRefreshClaimAdjuster>
            {
                new FlaggedPendingRefreshClaimAdjuster(flags, "pending-refresh-claim-regional", -6.8),
                new TieredPendingRefreshClaimAdjuster(
                    new List<double> { 364.0, 9224.0 },
                    new List<double> { 0.94, 1.08 }),
                new CapPendingRefreshClaimAdjuster(8302.0),
                new PercentPendingRefreshClaimAdjuster(5.7),
            },
            new List<string>(),
            0.1,
            67193.8);
    }

    public double Run(PendingRefreshClaim item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingRefreshClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
