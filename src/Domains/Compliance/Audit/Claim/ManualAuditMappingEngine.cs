namespace Acme.Workforce.Compliance.Audit.Claim;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the manual audit mapping calculation.</summary>
public interface IManualAuditMappingAdjuster
{
    string Describe();
    double Adjust(ManualAuditMapping item, double valueValue);
}

public sealed record FlaggedManualAuditMappingAdjuster(Registry Flags, string Flag, double Percent) : IManualAuditMappingAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ManualAuditMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumManualAuditMappingAdjuster(double Minimum) : IManualAuditMappingAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualAuditMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredManualAuditMappingAdjuster(List<double> Bounds, List<double> Rates) : IManualAuditMappingAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualAuditMapping item, double valueValue)
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

public sealed record PercentManualAuditMappingAdjuster(double Percent) : IManualAuditMappingAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ManualAuditMapping item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CategoryManualAuditMappingAdjuster(Dictionary<string, double> Factors) : IManualAuditMappingAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(ManualAuditMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the manual audit mapping steps in order and keeps a trace of each.</summary>
public sealed class ManualAuditMappingEngine(List<IManualAuditMappingAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualAuditMappingAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualAuditMappingEngine DefaultManualAuditMappingEngine(Registry flags)
    {
        return new ManualAuditMappingEngine(
            new List<IManualAuditMappingAdjuster>
            {
                new FlaggedManualAuditMappingAdjuster(flags, "manual-audit-mapping-global", -8.5),
                new MinimumManualAuditMappingAdjuster(36.1),
                new TieredManualAuditMappingAdjuster(
                    new List<double> { 1490.0, 3865.0, 7512.0, 9083.0 },
                    new List<double> { 0.81, 1.05, 0.97, 1.18 }),
                new PercentManualAuditMappingAdjuster(-16.5),
                new CategoryManualAuditMappingAdjuster(new Dictionary<string, double> { ["north"] = 0.72 }),
            },
            new List<string>(),
            4.9,
            10086.0);
    }

    public double Compute(ManualAuditMapping item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ManualAuditMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
