namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending equity grant override calculation.</summary>
public interface IPendingEquityGrantOverrideAdjuster
{
    string Describe();
    double Adjust(PendingEquityGrantOverride item, double valueValue);
}

public sealed record TieredPendingEquityGrantOverrideAdjuster(List<double> Bounds, List<double> Rates) : IPendingEquityGrantOverrideAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingEquityGrantOverride item, double valueValue)
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

public sealed record MinimumPendingEquityGrantOverrideAdjuster(double Minimum) : IPendingEquityGrantOverrideAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingEquityGrantOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentPendingEquityGrantOverrideAdjuster(double Percent) : IPendingEquityGrantOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingEquityGrantOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPendingEquityGrantOverrideAdjuster(Registry Flags, string Flag, double Percent) : IPendingEquityGrantOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingEquityGrantOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPendingEquityGrantOverrideAdjuster(double Step) : IPendingEquityGrantOverrideAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingEquityGrantOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the pending equity grant override steps in order and keeps a trace of each.</summary>
public sealed class PendingEquityGrantOverrideEngine(List<IPendingEquityGrantOverrideAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingEquityGrantOverrideAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingEquityGrantOverrideEngine DefaultPendingEquityGrantOverrideEngine(Registry flags)
    {
        return new PendingEquityGrantOverrideEngine(
            new List<IPendingEquityGrantOverrideAdjuster>
            {
                new TieredPendingEquityGrantOverrideAdjuster(
                    new List<double> { 6075.0, 7336.0 },
                    new List<double> { 1.03, 0.96 }),
                new MinimumPendingEquityGrantOverrideAdjuster(15.1),
                new PercentPendingEquityGrantOverrideAdjuster(2.2),
                new FlaggedPendingEquityGrantOverrideAdjuster(flags, "pending-equity-grant-override-global", 1.1),
                new RoundingPendingEquityGrantOverrideAdjuster(5.0),
            },
            new List<string>(),
            1.8,
            63492.0);
    }

    public double Compute(PendingEquityGrantOverride item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingEquityGrantOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
