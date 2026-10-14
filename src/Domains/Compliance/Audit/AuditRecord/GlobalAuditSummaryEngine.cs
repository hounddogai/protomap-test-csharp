namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global audit summary calculation.</summary>
public interface IGlobalAuditSummaryFactor
{
    string Describe();
    double Adjust(GlobalAuditSummary item, double valueValue);
}

public sealed record CapGlobalAuditSummaryAdjuster(double Cap) : IGlobalAuditSummaryFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalAuditSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumGlobalAuditSummaryAdjuster(double Minimum) : IGlobalAuditSummaryFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalAuditSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingGlobalAuditSummaryAdjuster(double Step) : IGlobalAuditSummaryFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalAuditSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the global audit summary steps in order and keeps a trace of each.</summary>
public sealed class GlobalAuditSummaryEngine(List<IGlobalAuditSummaryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalAuditSummaryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalAuditSummaryEngine DefaultGlobalAuditSummaryEngine()
    {
        return new GlobalAuditSummaryEngine(
            new List<IGlobalAuditSummaryFactor>
            {
                new CapGlobalAuditSummaryAdjuster(23430.0),
                new MinimumGlobalAuditSummaryAdjuster(2.3),
                new RoundingGlobalAuditSummaryAdjuster(0.5),
            },
            new List<string>(),
            4.1,
            84398.0);
    }

    public double Price(GlobalAuditSummary item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalAuditSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
