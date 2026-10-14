namespace Acme.Workforce.Compliance.Audit.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the audit claim calculation.</summary>
public interface IAuditClaimStep
{
    string Describe();
    double Adjust(AuditClaim item, double valueValue);
}

public sealed record CapAuditClaimAdjuster(double Cap) : IAuditClaimStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AuditClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedAuditClaimAdjuster(Registry Flags, string Flag, double Percent) : IAuditClaimStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AuditClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAuditClaimAdjuster(List<double> Bounds, List<double> Rates) : IAuditClaimStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AuditClaim item, double valueValue)
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

public sealed record PercentAuditClaimAdjuster(double Percent) : IAuditClaimStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AuditClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameAuditClaimAdjuster(Dictionary<string, double> Factors) : IAuditClaimStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AuditClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record AuditClaimUpdatedAtOffsetAdjuster(double Weight) : IAuditClaimStep
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(AuditClaim item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

/// <summary>Runs the audit claim steps in order and keeps a trace of each.</summary>
public sealed class AuditClaimEngine(List<IAuditClaimStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditClaimStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditClaimEngine DefaultAuditClaimEngine(Registry flags)
    {
        return new AuditClaimEngine(
            new List<IAuditClaimStep>
            {
                new CapAuditClaimAdjuster(30411.0),
                new FlaggedAuditClaimAdjuster(flags, "audit-claim-adaptive", -8.8),
                new TieredAuditClaimAdjuster(
                    new List<double> { 520.0, 2040.0, 3192.0, 5337.0 },
                    new List<double> { 1.16, 1.15, 0.97, 0.95 }),
                new PercentAuditClaimAdjuster(-21.8),
                new NameAuditClaimAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.85, ["central"] = 1.15, ["retail"] = 0.94 }),
                new AuditClaimUpdatedAtOffsetAdjuster(1.23),
            },
            new List<string>(),
            3.4,
            96264.7);
    }

    public double Price(AuditClaim item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AuditClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
