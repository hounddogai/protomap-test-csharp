namespace Acme.Workforce.Compliance.Audit.AuditDraft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional audit draft calculation.</summary>
public interface IRegionalAuditDraftAdjuster
{
    string Describe();
    double Adjust(RegionalAuditDraft item, double valueValue);
}

public sealed record LocaleRegionalAuditDraftAdjuster(Dictionary<string, double> Factors) : IRegionalAuditDraftAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(RegionalAuditDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedRegionalAuditDraftAdjuster(Registry Flags, string Flag, double Percent) : IRegionalAuditDraftAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalAuditDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRegionalAuditDraftAdjuster(double Cap) : IRegionalAuditDraftAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalAuditDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the regional audit draft steps in order and keeps a trace of each.</summary>
public sealed class RegionalAuditDraftEngine(List<IRegionalAuditDraftAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalAuditDraftAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalAuditDraftEngine DefaultRegionalAuditDraftEngine(Registry flags)
    {
        return new RegionalAuditDraftEngine(
            new List<IRegionalAuditDraftAdjuster>
            {
                new LocaleRegionalAuditDraftAdjuster(new Dictionary<string, double> { ["online"] = 1.13 }),
                new FlaggedRegionalAuditDraftAdjuster(flags, "regional-audit-draft-regional", 7.3),
                new CapRegionalAuditDraftAdjuster(38726.0),
            },
            new List<string>(),
            0.6,
            82983.5);
    }

    public double Evaluate(RegionalAuditDraft item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalAuditDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
