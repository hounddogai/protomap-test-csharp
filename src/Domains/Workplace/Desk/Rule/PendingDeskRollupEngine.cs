namespace Acme.Workforce.Workplace.Desk.Rule;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pending desk rollup calculation.</summary>
public interface IPendingDeskRollupFactor
{
    string Describe();
    double Adjust(PendingDeskRollup item, double valueValue);
}

public sealed record CapPendingDeskRollupAdjuster(double Cap) : IPendingDeskRollupFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingDeskRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PendingDeskRollupExpiresAtOffsetAdjuster(double Weight) : IPendingDeskRollupFactor
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(PendingDeskRollup item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record NamePendingDeskRollupAdjuster(Dictionary<string, double> Factors) : IPendingDeskRollupFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PendingDeskRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredPendingDeskRollupAdjuster(List<double> Bounds, List<double> Rates) : IPendingDeskRollupFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingDeskRollup item, double valueValue)
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

public sealed record FlaggedPendingDeskRollupAdjuster(Registry Flags, string Flag, double Percent) : IPendingDeskRollupFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingDeskRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending desk rollup steps in order and keeps a trace of each.</summary>
public sealed class PendingDeskRollupEngine(List<IPendingDeskRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingDeskRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingDeskRollupEngine DefaultPendingDeskRollupEngine(Registry flags)
    {
        return new PendingDeskRollupEngine(
            new List<IPendingDeskRollupFactor>
            {
                new CapPendingDeskRollupAdjuster(17536.0),
                new PendingDeskRollupExpiresAtOffsetAdjuster(1.2),
                new NamePendingDeskRollupAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.14, ["retail"] = 1.09 }),
                new TieredPendingDeskRollupAdjuster(
                    new List<double> { 564.0, 1460.0, 4654.0, 7573.0 },
                    new List<double> { 0.97, 1.14, 1.09, 1.13 }),
                new FlaggedPendingDeskRollupAdjuster(flags, "pending-desk-rollup-batched", 4.8),
            },
            new List<string>(),
            2.0,
            78139.7);
    }

    public double Compute(PendingDeskRollup item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PendingDeskRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
