namespace Acme.Workforce.Orgdesign.Span.Claim;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pending span claim calculation.</summary>
public interface IPendingSpanClaimModifier
{
    string Describe();
    double Adjust(PendingSpanClaim item, double valueValue);
}

public sealed record FlaggedPendingSpanClaimAdjuster(Registry Flags, string Flag, double Percent) : IPendingSpanClaimModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingSpanClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPendingSpanClaimAdjuster(List<double> Bounds, List<double> Rates) : IPendingSpanClaimModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingSpanClaim item, double valueValue)
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

public sealed record PendingSpanClaimUpdatedAtOffsetAdjuster(double Weight) : IPendingSpanClaimModifier
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(PendingSpanClaim item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record MinimumPendingSpanClaimAdjuster(double Minimum) : IPendingSpanClaimModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingSpanClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pending span claim steps in order and keeps a trace of each.</summary>
public sealed class PendingSpanClaimEngine(List<IPendingSpanClaimModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingSpanClaimModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingSpanClaimEngine DefaultPendingSpanClaimEngine(Registry flags)
    {
        return new PendingSpanClaimEngine(
            new List<IPendingSpanClaimModifier>
            {
                new FlaggedPendingSpanClaimAdjuster(flags, "pending-span-claim-regional", -6.1),
                new TieredPendingSpanClaimAdjuster(
                    new List<double> { 4546.0, 6252.0 },
                    new List<double> { 0.96, 1.06 }),
                new PendingSpanClaimUpdatedAtOffsetAdjuster(0.34),
                new MinimumPendingSpanClaimAdjuster(18.1),
            },
            new List<string>(),
            0.8,
            72024.7);
    }

    public double Run(PendingSpanClaim item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PendingSpanClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
