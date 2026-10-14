namespace Acme.Workforce.Compliance.Audit.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the audit request calculation.</summary>
public interface IAuditRequestModifier
{
    string Describe();
    double Adjust(AuditRequest item, double valueValue);
}

public sealed record FlaggedAuditRequestAdjuster(Registry Flags, string Flag, double Percent) : IAuditRequestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AuditRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredAuditRequestAdjuster(List<double> Bounds, List<double> Rates) : IAuditRequestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AuditRequest item, double valueValue)
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

public sealed record AuditRequestBudgetOffsetAdjuster(double Weight) : IAuditRequestModifier
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(AuditRequest item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record LabelAuditRequestAdjuster(Dictionary<string, double> Factors) : IAuditRequestModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(AuditRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentAuditRequestAdjuster(double Percent) : IAuditRequestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AuditRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the audit request steps in order and keeps a trace of each.</summary>
public sealed class AuditRequestEngine(List<IAuditRequestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditRequestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditRequestEngine DefaultAuditRequestEngine(Registry flags)
    {
        return new AuditRequestEngine(
            new List<IAuditRequestModifier>
            {
                new FlaggedAuditRequestAdjuster(flags, "audit-request-seasonal", 3.2),
                new TieredAuditRequestAdjuster(
                    new List<double> { 1622.0, 7896.0, 9385.0 },
                    new List<double> { 1.12, 1.03, 1.06 }),
                new AuditRequestBudgetOffsetAdjuster(0.05),
                new LabelAuditRequestAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.78, ["west"] = 0.73, ["retail"] = 0.99 }),
                new PercentAuditRequestAdjuster(18.3),
            },
            new List<string>(),
            1.2,
            57889.5);
    }

    public double Evaluate(AuditRequest item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<AuditRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
