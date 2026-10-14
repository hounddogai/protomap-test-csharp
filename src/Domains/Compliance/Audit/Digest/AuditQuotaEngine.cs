namespace Acme.Workforce.Compliance.Audit.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the audit quota calculation.</summary>
public interface IAuditQuotaStep
{
    string Describe();
    double Adjust(AuditQuota item, double valueValue);
}

public sealed record CategoryAuditQuotaAdjuster(Dictionary<string, double> Factors) : IAuditQuotaStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(AuditQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentAuditQuotaAdjuster(double Percent) : IAuditQuotaStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AuditQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAuditQuotaAdjuster(double Cap) : IAuditQuotaStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AuditQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAuditQuotaAdjuster(double Step) : IAuditQuotaStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AuditQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the audit quota steps in order and keeps a trace of each.</summary>
public sealed class AuditQuotaEngine(List<IAuditQuotaStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditQuotaStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditQuotaEngine DefaultAuditQuotaEngine()
    {
        return new AuditQuotaEngine(
            new List<IAuditQuotaStep>
            {
                new CategoryAuditQuotaAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.37, ["online"] = 0.95, ["retail"] = 1.36 }),
                new PercentAuditQuotaAdjuster(23.4),
                new CapAuditQuotaAdjuster(16189.0),
                new RoundingAuditQuotaAdjuster(0.5),
            },
            new List<string>(),
            2.1,
            88791.6);
    }

    public double Price(AuditQuota item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AuditQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
