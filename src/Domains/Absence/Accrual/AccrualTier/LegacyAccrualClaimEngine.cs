namespace Acme.Workforce.Absence.Accrual.AccrualTier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy accrual claim calculation.</summary>
public interface ILegacyAccrualClaimAdjuster
{
    string Describe();
    double Adjust(LegacyAccrualClaim item, double valueValue);
}

public sealed record RoundingLegacyAccrualClaimAdjuster(double Step) : ILegacyAccrualClaimAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyAccrualClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentLegacyAccrualClaimAdjuster(double Percent) : ILegacyAccrualClaimAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyAccrualClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLegacyAccrualClaimAdjuster(double Cap) : ILegacyAccrualClaimAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyAccrualClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedLegacyAccrualClaimAdjuster(Registry Flags, string Flag, double Percent) : ILegacyAccrualClaimAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyAccrualClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LegacyAccrualClaimExpiresAtOffsetAdjuster(double Weight) : ILegacyAccrualClaimAdjuster
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(LegacyAccrualClaim item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

/// <summary>Runs the legacy accrual claim steps in order and keeps a trace of each.</summary>
public sealed class LegacyAccrualClaimEngine(List<ILegacyAccrualClaimAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyAccrualClaimAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyAccrualClaimEngine DefaultLegacyAccrualClaimEngine(Registry flags)
    {
        return new LegacyAccrualClaimEngine(
            new List<ILegacyAccrualClaimAdjuster>
            {
                new RoundingLegacyAccrualClaimAdjuster(0.5),
                new PercentLegacyAccrualClaimAdjuster(-19.8),
                new CapLegacyAccrualClaimAdjuster(48975.0),
                new FlaggedLegacyAccrualClaimAdjuster(flags, "legacy-accrual-claim-capped", 9.6),
                new LegacyAccrualClaimExpiresAtOffsetAdjuster(0.34),
            },
            new List<string>(),
            1.5,
            74716.1);
    }

    public double Price(LegacyAccrualClaim item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyAccrualClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
