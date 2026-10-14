namespace Acme.Workforce.Compliance.Audit.Manifest;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the audit estimate calculation.</summary>
public interface IAuditEstimateModifier
{
    string Describe();
    double Adjust(AuditEstimate item, double valueValue);
}

public sealed record PercentAuditEstimateAdjuster(double Percent) : IAuditEstimateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AuditEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AuditEstimateWeightOffsetAdjuster(double Weight) : IAuditEstimateModifier
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(AuditEstimate item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record CapAuditEstimateAdjuster(double Cap) : IAuditEstimateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AuditEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredAuditEstimateAdjuster(List<double> Bounds, List<double> Rates) : IAuditEstimateModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AuditEstimate item, double valueValue)
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

public sealed record FlaggedAuditEstimateAdjuster(Registry Flags, string Flag, double Percent) : IAuditEstimateModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AuditEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAuditEstimateAdjuster(double Minimum) : IAuditEstimateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AuditEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the audit estimate steps in order and keeps a trace of each.</summary>
public sealed class AuditEstimateEngine(List<IAuditEstimateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditEstimateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditEstimateEngine DefaultAuditEstimateEngine(Registry flags)
    {
        return new AuditEstimateEngine(
            new List<IAuditEstimateModifier>
            {
                new PercentAuditEstimateAdjuster(16.6),
                new AuditEstimateWeightOffsetAdjuster(0.74),
                new CapAuditEstimateAdjuster(9508.0),
                new TieredAuditEstimateAdjuster(
                    new List<double> { 1954.0, 7109.0, 7895.0 },
                    new List<double> { 1.15, 0.9, 1.11 }),
                new FlaggedAuditEstimateAdjuster(flags, "audit-estimate-seasonal", -9.4),
                new MinimumAuditEstimateAdjuster(19.7),
            },
            new List<string>(),
            3.3,
            52744.5);
    }

    public double Run(AuditEstimate item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<AuditEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
