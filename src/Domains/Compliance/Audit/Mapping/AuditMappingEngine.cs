namespace Acme.Workforce.Compliance.Audit.Mapping;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the audit mapping calculation.</summary>
public interface IAuditMappingAdjuster
{
    string Describe();
    double Adjust(AuditMapping item, double valueValue);
}

public sealed record TieredAuditMappingAdjuster(List<double> Bounds, List<double> Rates) : IAuditMappingAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AuditMapping item, double valueValue)
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

public sealed record FlaggedAuditMappingAdjuster(Registry Flags, string Flag, double Percent) : IAuditMappingAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AuditMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record AuditMappingRetriesOffsetAdjuster(double Weight) : IAuditMappingAdjuster
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(AuditMapping item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record MinimumAuditMappingAdjuster(double Minimum) : IAuditMappingAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AuditMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record SegmentAuditMappingAdjuster(Dictionary<string, double> Factors) : IAuditMappingAdjuster
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(AuditMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the audit mapping steps in order and keeps a trace of each.</summary>
public sealed class AuditMappingEngine(List<IAuditMappingAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditMappingAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditMappingEngine DefaultAuditMappingEngine(Registry flags)
    {
        return new AuditMappingEngine(
            new List<IAuditMappingAdjuster>
            {
                new TieredAuditMappingAdjuster(
                    new List<double> { 2562.0, 3412.0, 6066.0, 9395.0 },
                    new List<double> { 1.05, 1.11, 1.06, 0.86 }),
                new FlaggedAuditMappingAdjuster(flags, "audit-mapping-batched", 7.1),
                new AuditMappingRetriesOffsetAdjuster(1.67),
                new MinimumAuditMappingAdjuster(45.5),
                new SegmentAuditMappingAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.79, ["online"] = 1.04 }),
            },
            new List<string>(),
            4.3,
            34259.9);
    }

    public double Evaluate(AuditMapping item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AuditMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
