namespace Acme.Workforce.Compliance.Audit.AuditMapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the audit ledger calculation.</summary>
public interface IAuditLedgerStep
{
    string Describe();
    double Adjust(AuditLedger item, double valueValue);
}

public sealed record RoundingAuditLedgerAdjuster(double Step) : IAuditLedgerStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AuditLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumAuditLedgerAdjuster(double Minimum) : IAuditLedgerStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AuditLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAuditLedgerAdjuster(List<double> Bounds, List<double> Rates) : IAuditLedgerStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AuditLedger item, double valueValue)
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

/// <summary>Runs the audit ledger steps in order and keeps a trace of each.</summary>
public sealed class AuditLedgerEngine(List<IAuditLedgerStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditLedgerStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditLedgerEngine DefaultAuditLedgerEngine()
    {
        return new AuditLedgerEngine(
            new List<IAuditLedgerStep>
            {
                new RoundingAuditLedgerAdjuster(0.05),
                new MinimumAuditLedgerAdjuster(42.3),
                new TieredAuditLedgerAdjuster(new List<double> { 548.0, 3647.0 }, new List<double> { 0.92, 1.03 }),
            },
            new List<string>(),
            3.8,
            65034.6);
    }

    public double Run(AuditLedger item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AuditLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
