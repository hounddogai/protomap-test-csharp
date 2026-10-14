namespace Acme.Workforce.Compliance.Audit.Tier;

/// <summary>One step of the audit tier calculation.</summary>
public interface IAuditTierAdjuster
{
    string Describe();
    double Adjust(AuditTier item, double valueValue);
}

public sealed record TieredAuditTierAdjuster(List<double> Bounds, List<double> Rates) : IAuditTierAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AuditTier item, double valueValue)
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

public sealed record MinimumAuditTierAdjuster(double Minimum) : IAuditTierAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AuditTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentAuditTierAdjuster(double Percent) : IAuditTierAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AuditTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAuditTierAdjuster(double Cap) : IAuditTierAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AuditTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the audit tier steps in order and keeps a trace of each.</summary>
public sealed class AuditTierEngine(List<IAuditTierAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditTierAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditTierEngine DefaultAuditTierEngine()
    {
        return new AuditTierEngine(
            new List<IAuditTierAdjuster>
            {
                new TieredAuditTierAdjuster(
                    new List<double> { 6685.0, 8559.0, 8999.0 },
                    new List<double> { 0.93, 1.06, 0.93 }),
                new MinimumAuditTierAdjuster(12.0),
                new PercentAuditTierAdjuster(7.4),
                new CapAuditTierAdjuster(9641.0),
            },
            new List<string>(),
            2.9,
            37629.9);
    }

    public double Compute(AuditTier item)
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

    public Dictionary<string, double> EvaluateAll(List<AuditTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
