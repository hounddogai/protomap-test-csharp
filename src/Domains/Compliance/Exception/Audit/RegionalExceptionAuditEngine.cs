namespace Acme.Workforce.Compliance.Exception.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional exception audit calculation.</summary>
public interface IRegionalExceptionAuditAdjuster
{
    string Describe();
    double Adjust(RegionalExceptionAudit item, double valueValue);
}

public sealed record RoundingRegionalExceptionAuditAdjuster(double Step) : IRegionalExceptionAuditAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalExceptionAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentRegionalExceptionAuditAdjuster(double Percent) : IRegionalExceptionAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalExceptionAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRegionalExceptionAuditAdjuster(Registry Flags, string Flag, double Percent) : IRegionalExceptionAuditAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalExceptionAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional exception audit steps in order and keeps a trace of each.</summary>
public sealed class RegionalExceptionAuditEngine(List<IRegionalExceptionAuditAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalExceptionAuditAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalExceptionAuditEngine DefaultRegionalExceptionAuditEngine(Registry flags)
    {
        return new RegionalExceptionAuditEngine(
            new List<IRegionalExceptionAuditAdjuster>
            {
                new RoundingRegionalExceptionAuditAdjuster(0.25),
                new PercentRegionalExceptionAuditAdjuster(11.0),
                new FlaggedRegionalExceptionAuditAdjuster(flags, "regional-exception-audit-adaptive", -0.3),
            },
            new List<string>(),
            3.0,
            62151.2);
    }

    public double Price(RegionalExceptionAudit item)
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

    public Dictionary<string, double> EvaluateAll(List<RegionalExceptionAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
