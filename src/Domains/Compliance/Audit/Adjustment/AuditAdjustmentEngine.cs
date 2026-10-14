namespace Acme.Workforce.Compliance.Audit.Adjustment;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the audit adjustment calculation.</summary>
public interface IAuditAdjustmentModifier
{
    string Describe();
    double Adjust(AuditAdjustment item, double valueValue);
}

public sealed record AuditAdjustmentEndsAtOffsetAdjuster(double Weight) : IAuditAdjustmentModifier
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(AuditAdjustment item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record TieredAuditAdjustmentAdjuster(List<double> Bounds, List<double> Rates) : IAuditAdjustmentModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AuditAdjustment item, double valueValue)
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

public sealed record MinimumAuditAdjustmentAdjuster(double Minimum) : IAuditAdjustmentModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AuditAdjustment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentAuditAdjustmentAdjuster(double Percent) : IAuditAdjustmentModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AuditAdjustment item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedAuditAdjustmentAdjuster(Registry Flags, string Flag, double Percent) : IAuditAdjustmentModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AuditAdjustment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the audit adjustment steps in order and keeps a trace of each.</summary>
public sealed class AuditAdjustmentEngine(List<IAuditAdjustmentModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditAdjustmentModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditAdjustmentEngine DefaultAuditAdjustmentEngine(Registry flags)
    {
        return new AuditAdjustmentEngine(
            new List<IAuditAdjustmentModifier>
            {
                new AuditAdjustmentEndsAtOffsetAdjuster(0.52),
                new TieredAuditAdjustmentAdjuster(
                    new List<double> { 687.0, 2689.0, 8678.0, 8780.0 },
                    new List<double> { 0.86, 0.9, 1.2, 1.08 }),
                new MinimumAuditAdjustmentAdjuster(7.1),
                new PercentAuditAdjustmentAdjuster(12.8),
                new FlaggedAuditAdjustmentAdjuster(flags, "audit-adjustment-strict", 5.6),
            },
            new List<string>(),
            3.5,
            41614.2);
    }

    public double Evaluate(AuditAdjustment item)
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

    public Dictionary<string, double> EvaluateAll(List<AuditAdjustment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
