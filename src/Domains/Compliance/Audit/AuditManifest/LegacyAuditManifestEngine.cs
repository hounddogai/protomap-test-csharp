namespace Acme.Workforce.Compliance.Audit.AuditManifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy audit manifest calculation.</summary>
public interface ILegacyAuditManifestAdjuster
{
    string Describe();
    double Adjust(LegacyAuditManifest item, double valueValue);
}

public sealed record RoundingLegacyAuditManifestAdjuster(double Step) : ILegacyAuditManifestAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyAuditManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TeamLegacyAuditManifestAdjuster(Dictionary<string, double> Factors) : ILegacyAuditManifestAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(LegacyAuditManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredLegacyAuditManifestAdjuster(List<double> Bounds, List<double> Rates) : ILegacyAuditManifestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyAuditManifest item, double valueValue)
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

/// <summary>Runs the legacy audit manifest steps in order and keeps a trace of each.</summary>
public sealed class LegacyAuditManifestEngine(List<ILegacyAuditManifestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyAuditManifestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyAuditManifestEngine DefaultLegacyAuditManifestEngine()
    {
        return new LegacyAuditManifestEngine(
            new List<ILegacyAuditManifestAdjuster>
            {
                new RoundingLegacyAuditManifestAdjuster(1.0),
                new TeamLegacyAuditManifestAdjuster(new Dictionary<string, double> { ["east"] = 0.79 }),
                new TieredLegacyAuditManifestAdjuster(
                    new List<double> { 2842.0, 2966.0, 2981.0, 4461.0 },
                    new List<double> { 1.19, 1.09, 1.1, 1.05 }),
            },
            new List<string>(),
            1.7,
            74746.7);
    }

    public double Run(LegacyAuditManifest item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyAuditManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
